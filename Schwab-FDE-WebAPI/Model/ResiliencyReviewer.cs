public class ResiliencyReviewer
{
    public (string status, bool degraded, List<string> warnings) Review(List<AttributionGroupRequest> groups)
    {
        var missing = groups.Where(g => g.ReturnPct == null && g.FallbackReturnPct == null).ToList();
        var warnings = new List<string>();
        if (missing.Count == 1)
        {
            warnings.Add($"Missing return for {missing[0].GroupName}");
            return ("DEGRADED", true, warnings);
        }
        if (missing.Count > 1)
        {
            warnings.Add("Multiple groups missing returns");
            return ("REVIEW_REQUIRED", true, warnings);
        }
        return ("VALID", false, warnings);
    }
}
