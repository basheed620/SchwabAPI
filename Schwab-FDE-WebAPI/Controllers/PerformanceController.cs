using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Portfolio Performance API Controller
/// Provides endpoints for calculating daily returns and attribution analysis
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly AttributionService _attributionService;
    private readonly PerformanceCalculationService _performanceService;
    private readonly ILogger<PerformanceController> _logger;
    private const string ErrorMessage = "An error occurred processing your request. Please contact support.";

    public PerformanceController(AttributionService attributionService, PerformanceCalculationService performanceService, ILogger<PerformanceController> logger)
    {
        _attributionService = attributionService ?? throw new ArgumentNullException(nameof(attributionService));
        _performanceService = performanceService ?? throw new ArgumentNullException(nameof(performanceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculate daily return summary for a portfolio
    /// </summary>
    /// <remarks>
    /// Calculates portfolio return percentage, benchmark return percentage, excess return, and validation status
    /// based on begin/end market values, net cash flow, and benchmark performance.
    /// 
    /// ## Validation Rules
    /// - Rejects if begin or end market value is negative
    /// - Rejects if currency is missing
    /// - Rejects if begin value is zero but end value is non-zero
    /// - Returns REVIEW_REQUIRED if return deviation > 5% from benchmark
    /// - Returns REVIEW_REQUIRED if net cash flow > 20% of begin value
    /// 
    /// ## Sample Request
    /// ```json
    /// {
    ///   "portfolioId": "PF-1001",
    ///   "valuationDate": "2026-06-14",
    ///   "beginMarketValue": 1000000,
    ///   "endMarketValue": 1035000,
    ///   "netCashFlow": 10000,
    ///   "benchMarketReturnPct": 1.8,
    ///   "currency": "USD",
    ///   "requestedBy": "advisor01"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Daily return calculation request with portfolio metrics</param>
    /// <returns>Daily return response with calculated returns and status</returns>
    /// <response code="200">Successfully calculated daily return</response>
    /// <response code="400">Invalid request input</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("daily-return")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DailyReturnResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public ActionResult<DailyReturnResponse> DailyReturn([FromBody] DailyReturnRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request body is required." });

        try
        {
            _logger.LogInformation("Processing daily-return request for portfolio {PortfolioId}", request.PortfolioId);
            var result = _performanceService.Calculate(request);
            return Ok(result);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Validation error in daily-return request for portfolio {PortfolioId}", request.PortfolioId);
            return BadRequest(new { error = "Invalid request input." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing daily-return request for portfolio {PortfolioId}", request.PortfolioId);
            return StatusCode(500, new { error = ErrorMessage });
        }
    }
}
