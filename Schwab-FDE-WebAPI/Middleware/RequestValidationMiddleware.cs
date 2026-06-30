using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Request validation middleware that enforces input constraints:
/// - Validates content type for POST requests
/// - Enforces maximum request size
/// - Sanitizes request headers
/// </summary>
public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;
    private const long MaxRequestSize = 1_048_576; // 1 MB

    public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate POST requests to our API
        if (context.Request.Method == HttpMethods.Post && context.Request.Path.StartsWithSegments("/api"))
        {
            // Validate content type
            if (!context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                _logger.LogWarning("Rejected request due to unsupported content type: {ContentType}", context.Request.ContentType);
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                await context.Response.WriteAsJsonAsync(new { error = "Content-Type must be application/json" });
                return;
            }

            // Validate request size if provided
            if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > MaxRequestSize)
            {
                _logger.LogWarning("Rejected request due to payload too large: {Size}", context.Request.ContentLength.Value);
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await context.Response.WriteAsJsonAsync(new { error = "Request payload too large" });
                return;
            }

            // Basic header sanitization: remove any suspicious server-identifying headers
            if (context.Request.Headers.ContainsKey("X-Powered-By"))
            {
                context.Request.Headers.Remove("X-Powered-By");
            }
        }

        await _next(context);
    }
}
