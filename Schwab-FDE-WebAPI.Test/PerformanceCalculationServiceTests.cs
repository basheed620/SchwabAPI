using Xunit;
namespace Schwab_FDE_WebAPI.Test
{
    public class PerformanceCalculationServiceTests
    {
        private readonly PerformanceCalculationService _service = new PerformanceCalculationService();

        [Fact]
        public void ValidInput_ReturnsValid()
        {
            var req = new DailyReturnRequest
            {
                PortfolioId = "PF-1001",
                ValuationDate = DateTime.Parse("2026-06-14"),
                BeginMarketValue = 1000000,
                EndMarketValue = 1035000,
                NetCashFlow = 10000,
                BenchMarketReturnPct = 1.8m,
                Currency = "USD",
                RequestedBy = "advisor01"
            };
            var res = _service.Calculate(req);
            Assert.Equal("valid", res.Status);
            Assert.Equal(2.5m, res.PortfolioReturnPct, 2);
            Assert.Equal(0.7m, res.ExcessReturnPct, 2);
        }

        [Fact]
        public void LargeDeviation_ReturnsReviewRequired()
        {
            var req = new DailyReturnRequest
            {
                PortfolioId = "PF-1002",
                ValuationDate = DateTime.Parse("2026-06-14"),
                BeginMarketValue = 1000000,
                EndMarketValue = 1100000,
                NetCashFlow = 0,
                BenchMarketReturnPct = 1.8m,
                Currency = "USD",
                RequestedBy = "advisor01"
            };
            var res = _service.Calculate(req);
            Assert.Equal("review_required", res.Status);
            Assert.Contains("Portfolio return deviates", res.Reasons[0]);
        }

        [Fact]
        public void InvalidInput_ReturnsInvalid()
        {
            var req = new DailyReturnRequest
            {
                PortfolioId = "PF-1003",
                ValuationDate = DateTime.Parse("2026-06-14"),
                BeginMarketValue = -1,
                EndMarketValue = 1000,
                NetCashFlow = 0,
                BenchMarketReturnPct = 1.8m,
                Currency = "",
                RequestedBy = "advisor01"
            };
            var res = _service.Calculate(req);
            Assert.Equal("invalid_input", res.Status);
            Assert.Contains("Begin market value cannot be negative.", res.Reasons);
            Assert.Contains("Currency is required.", res.Reasons);
        }
    }
    
}