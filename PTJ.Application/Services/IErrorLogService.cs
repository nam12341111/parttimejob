using PTJ.Domain.Entities;

namespace PTJ.Application.Services;

/// <summary>
/// Service interface for logging system errors
/// </summary>
public interface IErrorLogService
{
    /// <summary>
    /// Logs a system error with full context
    /// </summary>
    Task LogErrorAsync(
        string level,
        string message,
        Exception? exception = null,
        int? userId = null,
        string? requestPath = null,
        string? httpMethod = null,
        string? queryString = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? source = null,
        string? additionalData = null);
    
    /// <summary>
    /// Gets error logs with pagination (for admin dashboard)
    /// </summary>
    Task<(List<SystemErrorLog> logs, int totalCount)> GetErrorLogsAsync(
        string? level = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50);
}
