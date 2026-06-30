using System;
using System.Collections.Generic;
using Xunit;

public class AttributionServiceTests
{

    private AttributionService CreateService()
        => new AttributionService(new IdempotencyStore());
    
    [Fact]
    public void ValidInput_ReturnsValidStatus()
    {
        var service = CreateService();
        var req = new AttributionRequest
        {
            RequestId = "REQ-1",
            PortifolioId = "PF-1",
            ValuationDate = DateTime.Today,
            Groups = new List<AttributionGroupRequest>
            {
                new() { GroupName = "Equity", WeightPct = 60, ReturnPct = 1.5m },
                new() { GroupName = "FixedIncome", WeightPct = 30, ReturnPct = 0.4m },
                new() { GroupName = "Cash", WeightPct = 10, ReturnPct = 0.05m }
            },
            Currency = "USD",
            RequestedBy = "user"
        };

        var result = service.Calculate(req);

        Assert.Equal("VALID", result.Status);
        Assert.False(result.Degraded);
        Assert.Equal(3, result.GroupContributions.Count);
    }

    [Fact]
    public void DegradedStatus_OneMissingNoFallback()
    {
        var service = CreateService();
        var req = new AttributionRequest
        {
            RequestId = "REQ-2",
            PortifolioId = "PF-2",
            ValuationDate = DateTime.Today,
            Groups = new List<AttributionGroupRequest>
            {
                new() { GroupName = "Equity", WeightPct = 60, ReturnPct = 1.5m },
                new() { GroupName = "FixedIncome", WeightPct = 30, ReturnPct = null },
                new() { GroupName = "Cash", WeightPct = 10, ReturnPct = 0.05m }
            },
            Currency = "USD",
            RequestedBy = "user"
        };

        var result = service.Calculate(req);

        Assert.Equal("DEGRADED", result.Status);
        Assert.True(result.Degraded);
        Assert.Contains("FixedIncome", string.Join(",", result.Warnings));
    }

    [Fact]
    public void ReviewRequiredStatus_MultipleMissingNoFallback()
    {
        var service = CreateService();
        var req = new AttributionRequest
        {
            RequestId = "REQ-3",
            PortifolioId = "PF-3",
            ValuationDate = DateTime.Today,
            Groups = new List<AttributionGroupRequest>
            {
                new() { GroupName = "Equity", WeightPct = 60, ReturnPct = null },
                new() { GroupName = "FixedIncome", WeightPct = 30, ReturnPct = null },
                new() { GroupName = "Cash", WeightPct = 10, ReturnPct = 0.05m }
            },
            Currency = "USD",
            RequestedBy = "user"
        };

        var result = service.Calculate(req);

        Assert.Equal("REVIEW_REQUIRED", result.Status);
        Assert.True(result.Degraded);
        Assert.Contains("Multiple groups missing returns", string.Join(",", result.Warnings));
    }

    [Fact]
    public void InvalidWeight_ReturnsInvalidInput()
    {
        var service = CreateService();
        var req = new AttributionRequest
        {
            RequestId = "REQ-4",
            PortifolioId = "PF-4",
            ValuationDate = DateTime.Today,
            Groups = new List<AttributionGroupRequest>
            {
                new() { GroupName = "Equity", WeightPct = 80, ReturnPct = 1.5m },
                new() { GroupName = "FixedIncome", WeightPct = 10, ReturnPct = 0.4m },
                new() { GroupName = "Cash", WeightPct = 5, ReturnPct = 0.05m }
            },
            Currency = "USD",
            RequestedBy = "user"
        };

        var result = service.Calculate(req);

        Assert.Equal("INVALID_INPUT", result.Status);
        Assert.Contains("weights", string.Join(",", result.Warnings));
    }

    [Fact]
    public void IdempotentRequest_ReturnsSameResult()
    {
        var store = new IdempotencyStore();
        var service = new AttributionService(store);
        var req = new AttributionRequest
        {
            RequestId = "REQ-5",
            PortifolioId = "PF-5",
            ValuationDate = DateTime.Today,
            Groups = new List<AttributionGroupRequest>
            {
                new() { GroupName = "Equity", WeightPct = 60, ReturnPct = 1.5m },
                new() { GroupName = "FixedIncome", WeightPct = 30, ReturnPct = 0.4m },
                new() { GroupName = "Cash", WeightPct = 10, ReturnPct = 0.05m }
            },
            Currency = "USD",
            RequestedBy = "user"
        };

        var result1 = service.Calculate(req);
        var result2 = service.Calculate(req);

        Assert.Equal(result1.TotalContributionPct, result2.TotalContributionPct);
        Assert.Equal(result1.Status, result2.Status);
        Assert.Equal(result1.ProcessedAt, result2.ProcessedAt);
    }
}
    