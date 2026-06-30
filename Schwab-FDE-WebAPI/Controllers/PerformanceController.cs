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

    ///// <summary>
    ///// Calculate performance attribution by asset groups
    ///// </summary>
    ///// <remarks>
    ///// Calculates contribution to return by asset groups (e.g., Equity, Fixed Income, Cash)
    ///// with support for fallback pricing when primary returns are unavailable.
    ///// Supports idempotent requests using RequestId for deduplication.
    ///// 
    ///// ## Validation Rules
    ///// - Total group weights must be between 99% and 101%
    ///// - Uses fallback returns if primary returns unavailable
    ///// - Returns DEGRADED status if 1 group missing (no fallback)
    ///// - Returns REVIEW_REQUIRED if 2+ groups missing (no fallback)
    ///// - Supports idempotency: same RequestId returns cached result
    ///// 
    ///// ## Sample Request
    ///// ```json
    ///// {
    /////   "requestId": "ATTR-2001",
    /////   "portifolioId": "PF-2201",
    /////   "valuationdate": "2026-06-14",
    /////   "groups": [
    /////     {"groupName": "Equity", "weightPct": 60, "returnPct": 1.5},
    /////     {"groupName": "FixedIncome", "weightPct": 30, "returnPct": 0.4},
    /////     {"groupName": "Cash", "weightPct": 10, "returnPct": 0.05}
    /////   ],
    /////   "currency": "USD",
    /////   "requestedBy": "advisor02"
    ///// }
    ///// ```
    ///// </remarks>
    ///// <param name="request">Attribution calculation request with asset groups</param>
    ///// <returns>Attribution response with group contributions and pricing modes</returns>
    ///// <response code="200">Successfully calculated attribution</response>
    ///// <response code="400">Invalid request input (e.g., weights don't sum to 100%)</response>
    ///// <response code="500">Internal server error</response>
    //[HttpPost("attribution")]
    //[Produces("application/json")]
    //[ProducesResponseType(typeof(AttributionResponse), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    //public ActionResult<AttributionResponse> Attribution([FromBody] AttributionRequest request)
    //{
    //    if (request == null)
    //        return BadRequest(new { error = "Request body is required." });

    //    try
    //    {
    //        _logger.LogInformation("Processing attribution request {RequestId}", request.RequestId);
    //        var result = _attributionService.Calculate(request);
    //        return Ok(result);
    //    }
    //    catch (ArgumentNullException ex)
    //    {
    //        _logger.LogWarning(ex, "Validation error in attribution request {RequestId}", request.RequestId);
    //        return BadRequest(new { error = "Invalid request input." });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error processing attribution request {RequestId}", request.RequestId);
    //        return StatusCode(500, new { error = ErrorMessage });
    //    }
    //}
}
