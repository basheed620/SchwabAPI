using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
[ApiController]
[Route("api/[controller]")]
public class AttributionController : ControllerBase
{
    private readonly AttributionService _attributionService;
    private readonly PerformanceCalculationService _performanceService;
    private readonly ILogger<PerformanceController> _logger;
    private const string ErrorMessage = "An error occurred processing your request. Please contact support.";

    public AttributionController(AttributionService attributionService, ILogger<PerformanceController> logger)
    {
        _attributionService = attributionService ?? throw new ArgumentNullException(nameof(attributionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    }

    /// <summary>
    /// Calculate performance attribution by asset groups
    /// </summary>
    /// <remarks>
    /// Calculates contribution to return by asset groups (e.g., Equity, Fixed Income, Cash)
    /// with support for fallback pricing when primary returns are unavailable.
    /// Supports idempotent requests using RequestId for deduplication.
    /// 
    /// ## Validation Rules
    /// - Total group weights must be between 99% and 101%
    /// - Uses fallback returns if primary returns unavailable
    /// - Returns DEGRADED status if 1 group missing (no fallback)
    /// - Returns REVIEW_REQUIRED if 2+ groups missing (no fallback)
    /// - Supports idempotency: same RequestId returns cached result
    /// 
    /// ## Sample Request
    /// ```json
    /// {
    ///   "requestId": "ATTR-2001",
    ///   "portifolioId": "PF-2201",
    ///   "valuationdate": "2026-06-14",
    ///   "groups": [
    ///     {"groupName": "Equity", "weightPct": 60, "returnPct": 1.5},
    ///     {"groupName": "FixedIncome", "weightPct": 30, "returnPct": 0.4},
    ///     {"groupName": "Cash", "weightPct": 10, "returnPct": 0.05}
    ///   ],
    ///   "currency": "USD",
    ///   "requestedBy": "advisor02"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Attribution calculation request with asset groups</param>
    /// <returns>Attribution response with group contributions and pricing modes</returns>
    /// <response code="200">Successfully calculated attribution</response>
    /// <response code="400">Invalid request input (e.g., weights don't sum to 100%)</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("attribution")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AttributionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public ActionResult<AttributionResponse> Attribution([FromBody] AttributionRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request body is required." });

        try
        {
            _logger.LogInformation("Processing attribution request {RequestId}", request.RequestId);
            var result = _attributionService.Calculate(request);
            return Ok(result);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Validation error in attribution request {RequestId}", request.RequestId);
            return BadRequest(new { error = "Invalid request input." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing attribution request {RequestId}", request.RequestId);
            return StatusCode(500, new { error = ErrorMessage });
        }
    }
}
