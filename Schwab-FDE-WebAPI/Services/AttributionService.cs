public class AttributionService
{
    private readonly IdempotencyStore _idempotencyStore;

    public AttributionService(IdempotencyStore idempotencyStore)
    {
        _idempotencyStore = idempotencyStore ?? throw new ArgumentNullException(nameof(idempotencyStore));
    }

    public AttributionResponse Calculate(AttributionRequest request)
    {
        // Input validation
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Idempotency check
        if (_idempotencyStore.TryGetValue(request.RequestId, out var cached))
            return cached;

        var response = new AttributionResponse
        {
            RequestId = request.RequestId,
            PortifolioId = request.PortifolioId,
            ValuationDate = request.ValuationDate,
            ProcessedAt = DateTime.UtcNow
        };

        var errors = AttributionValidator.Validate(request);
        if (errors.Any())
        {
            response.Status = "INVALID_INPUT";
            response.Warnings.AddRange(errors);
            _idempotencyStore.Set(request.RequestId, response);
            return response;
        }

        int missing = 0;
        decimal totalContribution = 0m;

        foreach (var group in request.Groups)
        {
            decimal? usedReturn = group.ReturnPct;
            string pricingMode = "PRIMARY";
            if (usedReturn == null)
            {
                if (group.FallbackReturnPct != null)
                {
                    usedReturn = group.FallbackReturnPct;
                    pricingMode = "FALLBACK_USED";
                    response.Warnings.Add($"Fallback pricing used for {group.GroupName}");
                }
                else
                {
                    missing++;
                    response.Warnings.Add($"Missing return for {group.GroupName}");
                    continue;
                }
            }
            var contribution = group.WeightPct * (usedReturn.Value / 100);
            totalContribution += contribution;
            response.GroupContributions.Add(new AttributionGroupContribution
            {
                GroupName = group.GroupName,
                ContributionPct = Math.Round(contribution, 4),
                PricingMode = pricingMode
            });
        }

        response.TotalContributionPct = Math.Round(totalContribution, 4);

        // Status logic
        if (missing == 0)
        {
            response.Status = "VALID";
            response.Degraded = false;
        }
        else if (missing == 1)
        {
            response.Status = "DEGRADED";
            response.Degraded = true;
        }
        else
        {
            response.Status = "REVIEW_REQUIRED";
            response.Degraded = true;
            response.Warnings.Add("Multiple groups missing returns");
        }

        _idempotencyStore.Set(request.RequestId, response);
        return response;
    }
}
