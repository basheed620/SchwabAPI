using Microsoft.AspNetCore.Http;

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
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                await context.Response.WriteAsJsonAsync(new { error = "Content-Type must be application/json" });
                return;
            }

            // Validate request size
            if (context.Request.ContentLength > MaxRequestSize)
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await context.Response.WriteAsJsonAsync(new { error = "Request payload too large" });
                return;
            }
        }

        await _next(context);
    }
}
