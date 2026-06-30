// C#
/// <summary>
/// Validation for daily return requests.
/// Uses early returns and cached validations for optimal performance.
/// </summary>
public static class ValidationAssistant
{
    // Cache regex patterns at class level for performance
    private static readonly HashSet<string> ValidCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY", "INR", "MXN"
    };

    public static List<string> Validate(DailyReturnRequest req)
    {
        var reasons = new List<string>(4); // Pre-allocate with expected capacity

        // Early exits for critical errors
        if (req == null)
        {
            reasons.Add("Request is null.");
            return reasons;
        }

        // Validate numeric bounds (most common checks first)
        if (req.BeginMarketValue < 0)
            reasons.Add("Begin market value cannot be negative.");
        
        if (req.EndMarketValue < 0)
            reasons.Add("End market value cannot be negative.");

        // Validate required strings
        if (string.IsNullOrWhiteSpace(req.Currency))
            reasons.Add("Currency is required.");
        else if (!ValidCurrencies.Contains(req.Currency.Trim()))
            reasons.Add("Currency is not supported.");

        // Complex validation rules
        if (req.BeginMarketValue == 0 && req.EndMarketValue != 0)
            reasons.Add("Begin market value is zero but end market value is not zero.");

        return reasons;
    }
}
