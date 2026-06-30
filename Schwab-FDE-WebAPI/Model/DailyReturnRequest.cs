// C#
/// <summary>
/// Request model for daily return calculation
/// </summary>
public class DailyReturnRequest
{
    /// <summary>
    /// Unique identifier for the portfolio
    /// </summary>
    public string PortfolioId { get; set; }

    /// <summary>
    /// Valuation date for the calculation
    /// </summary>
    public DateTime ValuationDate { get; set; }

    /// <summary>
    /// Market value at the beginning of the period (cannot be negative)
    /// </summary>
    public decimal BeginMarketValue { get; set; }

    /// <summary>
    /// Market value at the end of the period (cannot be negative)
    /// </summary>
    public decimal EndMarketValue { get; set; }

    /// <summary>
    /// Net cash flow during the period (deposits/withdrawals)
    /// </summary>
    public decimal NetCashFlow { get; set; }

    /// <summary>
    /// Benchmark return percentage for comparison
    /// </summary>
    public decimal BenchMarketReturnPct { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR, GBP) - required
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// User who requested the calculation
    /// </summary>
    public string RequestedBy { get; set; }
}

/// <summary>
/// Response model for daily return calculation
/// </summary>
public class DailyReturnResponse
{
    /// <summary>
    /// Portfolio identifier from request
    /// </summary>
    public string PortfolioId { get; set; }

    /// <summary>
    /// Valuation date from request
    /// </summary>
    public DateTime ValuationDate { get; set; }

    /// <summary>
    /// Calculated portfolio return percentage
    /// Formula: ((EndValue - BeginValue - NetCashFlow) / BeginValue) * 100
    /// </summary>
    public decimal PortfolioReturnPct { get; set; }

    /// <summary>
    /// Benchmark return percentage from request
    /// </summary>
    public decimal BenchMarketReturnPct { get; set; }

    /// <summary>
    /// Excess return: Portfolio return minus benchmark return
    /// </summary>
    public decimal ExcessReturnPct { get; set; }

    /// <summary>
    /// Validation status: "valid", "review_required", or "invalid_input"
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// List of validation messages or reasons for status
    /// </summary>
    public List<string> Reasons { get; set; }

    /// <summary>
    /// Timestamp when the calculation was processed (UTC)
    /// </summary>
    public DateTime ProcessedAt { get; set; }
}
