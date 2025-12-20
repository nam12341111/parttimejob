using System.Diagnostics;
using System.Security.Claims;
using PTJ.Application.Services;

namespace PTJ.API.Middleware;

/// <summary>
/// Middleware for logging all HTTP requests and extracting user info from JWT
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IActivityLogService activityLogService)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Get original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            // Execute the request pipeline
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Extract user ID from JWT claims
            int? userId = GetUserIdFromClaims(context.User);

            // Get client IP address
            string ipAddress = GetClientIpAddress(context);

            // Get user agent
            string? userAgent = context.Request.Headers["User-Agent"].ToString();

            // Log the activity
            try
            {
                await activityLogService.LogActivityAsync(
                    userId: userId,
                    httpMethod: context.Request.Method,
                    path: context.Request.Path,
                    queryString: context.Request.QueryString.ToString(),
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    statusCode: context.Response.StatusCode,
                    durationMs: stopwatch.ElapsedMilliseconds
                );

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {Duration}ms | User: {UserId} | IP: {IpAddress}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    userId?.ToString() ?? "Anonymous",
                    ipAddress
                );
            }
            catch (Exception ex)
            {
                // Don't let logging errors break the request
                _logger.LogError(ex, "Failed to log request activity");
            }
        }
    }

    /// <summary>
    /// Extracts user ID from JWT claims
    /// </summary>
    private int? GetUserIdFromClaims(ClaimsPrincipal user)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            return null;

        // Try to get user ID from standard claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                         ?? user.FindFirst("sub") 
                         ?? user.FindFirst("userId")
                         ?? user.FindFirst("id");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Gets the client's real IP address, considering proxies
    /// </summary>
    private string GetClientIpAddress(HttpContext context)
    {
        // Try to get IP from X-Forwarded-For header (for proxies/load balancers)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Try X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
