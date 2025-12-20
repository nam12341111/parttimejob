using PTJ.Domain.Entities;

namespace PTJ.Application.Services;

/// <summary>
/// Service interface for logging user activities
/// </summary>
public interface IActivityLogService
{
    /// <summary>
    /// Logs a user activity (HTTP request)
    /// </summary>
    Task LogActivityAsync(
        int? userId,
        string httpMethod,
        string path,
        string? queryString,
        string ipAddress,
        string? userAgent,
        int statusCode,
        long durationMs,
        string? additionalData = null);
    
    /// <summary>
    /// Gets activity logs with pagination (for admin dashboard)
    /// </summary>
    Task<(List<UserActivityLog> logs, int totalCount)> GetActivityLogsAsync(
        int? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50);
}
