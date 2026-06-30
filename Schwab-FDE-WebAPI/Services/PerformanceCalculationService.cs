// C#
public class PerformanceCalculationService
{
    public DailyReturnResponse Calculate(DailyReturnRequest req)
    {
        // Input validation
        if (req == null)
            throw new ArgumentNullException(nameof(req));

        var reasons = ValidationAssistant.Validate(req);
        var status = "valid";
        decimal portfolioReturn = 0;
        decimal excessReturn = 0;

        if (reasons.Any())
        {
            status = "invalid_input";
        }
        else if (req.BeginMarketValue > 0)
        {
            portfolioReturn = ((req.EndMarketValue - req.BeginMarketValue - req.NetCashFlow) / req.BeginMarketValue) * 100;
            excessReturn = portfolioReturn - req.BenchMarketReturnPct;

            if (Math.Abs(portfolioReturn - req.BenchMarketReturnPct) > 5)
            {
                status = "review_required";
                reasons.Add("Portfolio return deviates from benchmark by more than 5%.");
            }
            if (Math.Abs(req.NetCashFlow) > 0.2m * req.BeginMarketValue)
            {
                status = "review_required";
                reasons.Add("Net cash flow exceeds 20% of begin market value.");
            }
        }

        return new DailyReturnResponse
        {
            PortfolioId = req.PortfolioId,
            ValuationDate = req.ValuationDate,
            PortfolioReturnPct = Math.Round(portfolioReturn, 2),
            BenchMarketReturnPct = req.BenchMarketReturnPct,
            ExcessReturnPct = Math.Round(excessReturn, 2),
            Status = status,
            Reasons = reasons,
            ProcessedAt = DateTime.UtcNow
        };
    }
}
