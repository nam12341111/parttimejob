using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using PTJ.Application.Common;
using PTJ.Application.Services;

namespace PTJ.API.Middleware;

/// <summary>
/// Middleware for global exception handling
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;
    
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "access_token",
        "accessToken",
        "refresh_token",
        "refreshToken",
        "password",
        "token",
        "authorization",
        "auth",
        "secret",
        "apikey",
        "api_key"
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, IErrorLogService errorLogService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            
            // Log to database
            await LogErrorToDatabaseAsync(context, ex, errorLogService);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task LogErrorToDatabaseAsync(HttpContext context, Exception exception, IErrorLogService errorLogService)
    {
        try
        {
            // Extract user ID from JWT claims
            int? userId = GetUserIdFromClaims(context.User);
            
            // Get client IP address
            string ipAddress = GetClientIpAddress(context);
            
            // Get user agent
            string? userAgent = context.Request.Headers["User-Agent"].ToString();

            // Determine error level based on exception type
            string level = exception switch
            {
                UnauthorizedAccessException => "Warning",
                KeyNotFoundException => "Warning",
                ArgumentException => "Warning",
                InvalidOperationException => "Error",
                _ => "Critical"
            };

            var sanitizedQueryString = SanitizeQueryString(context.Request.QueryString.ToString());
            
            await errorLogService.LogErrorAsync(
                level: level,
                message: exception.Message,
                exception: exception,
                userId: userId,
                requestPath: context.Request.Path,
                httpMethod: context.Request.Method,
                queryString: sanitizedQueryString,
                ipAddress: ipAddress,
                userAgent: userAgent,
                source: exception.Source
            );
        }
        catch (Exception logEx)
        {
            // Don't let logging errors break the exception handling
            _logger.LogError(logEx, "Failed to log error to database");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new Result<object>();

        switch (exception)
        {
            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = Result<object>.FailureResult("Unauthorized access");
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = Result<object>.FailureResult("Resource not found");
                break;

            case ArgumentException:
            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = Result<object>.FailureResult(exception.Message);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = _environment.IsDevelopment()
                    ? Result<object>.FailureResult($"Internal server error: {exception.Message}\n{exception.StackTrace}")
                    : Result<object>.FailureResult("An error occurred while processing your request");
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private int? GetUserIdFromClaims(ClaimsPrincipal user)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            return null;

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

    private string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Sanitizes query string by masking sensitive parameter values
    /// </summary>
    private string SanitizeQueryString(string queryString)
    {
        if (string.IsNullOrEmpty(queryString))
            return string.Empty;

        if (queryString.StartsWith("?"))
            queryString = queryString.Substring(1);

        var sanitized = new StringBuilder();
        var parameters = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var param in parameters)
        {
            var parts = param.Split('=', 2);
            var key = parts[0];
            var value = parts.Length > 1 ? parts[1] : string.Empty;

            if (sanitized.Length > 0)
                sanitized.Append('&');

            sanitized.Append(key);
            sanitized.Append('=');

            if (SensitiveKeys.Contains(key))
            {
                sanitized.Append("***REDACTED***");
            }
            else
            {
                const int maxValueLength = 100;
                if (value.Length > maxValueLength)
                {
                    sanitized.Append(value.Substring(0, maxValueLength));
                    sanitized.Append("...");
                }
                else
                {
                    sanitized.Append(value);
                }
            }
        }

        return sanitized.ToString();
    }
}

