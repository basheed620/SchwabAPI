public static class AttributionValidator
{
    public static List<string> Validate(AttributionRequest request)
    {
        var errors = new List<string>(5); // Pre-allocate with expected capacity

        if (request == null)
        {
            errors.Add("Request is null.");
            return errors;
        }

        // Check required fields first
        if (string.IsNullOrWhiteSpace(request.RequestId))
            errors.Add("RequestId is required.");
        
        if (string.IsNullOrWhiteSpace(request.PortifolioId))
            errors.Add("PortfolioId is required.");
        
        if (request.Groups == null || request.Groups.Count == 0)
            errors.Add("At least one group is required.");
        else
        {
            // Only validate weights if groups exist
            decimal weightSum = 0;
            foreach (var group in request.Groups)
            {
                weightSum += group.WeightPct;
                if (group.WeightPct < 0)
                {
                    errors.Add($"Group weight for {group.GroupName} cannot be negative.");
                    break;
                }
            }

            if (weightSum < 99 || weightSum > 101)
                errors.Add($"Sum of group weights must be between 99 and 101. Current sum: {weightSum:F2}");
        }

        return errors;
    }
}
