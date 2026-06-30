using System.Text.Json.Serialization;

public class AttributionRequest
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }

    [JsonPropertyName("portifolioId")]
    public string PortifolioId { get; set; }

    [JsonPropertyName("valuationdate")]
    public DateTime ValuationDate { get; set; }

    [JsonPropertyName("groups")]
    public List<AttributionGroupRequest> Groups { get; set; } = new();

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("requestedBy")]
    public string RequestedBy { get; set; }
}

public class AttributionGroupRequest
{
    [JsonPropertyName("groupName")]
    public string GroupName { get; set; }

    [JsonPropertyName("weightPct")]
    public decimal WeightPct { get; set; }

    [JsonPropertyName("returnPct")]
    public decimal? ReturnPct { get; set; }

    [JsonPropertyName("fallbackReturnedPct")]
    public decimal? FallbackReturnPct { get; set; }
}

public class AttributionResponse
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }

    [JsonPropertyName("portifolioId")]
    public string PortifolioId { get; set; }

    [JsonPropertyName("valuationdate")]
    public DateTime ValuationDate { get; set; }

    [JsonPropertyName("totalContributionPct")]
    public decimal TotalContributionPct { get; set; }

    [JsonPropertyName("groupContributions")]
    public List<AttributionGroupContribution> GroupContributions { get; set; } = new();

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("degraded")]
    public bool Degraded { get; set; }

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();

    [JsonPropertyName("processedAt")]
    public DateTime ProcessedAt { get; set; }
}

public class AttributionGroupContribution
{
    [JsonPropertyName("groupName")]
    public string GroupName { get; set; }

    [JsonPropertyName("contributionPct")]
    public decimal ContributionPct { get; set; }

    [JsonPropertyName("pricingMode")]
    public string PricingMode { get; set; }
}
