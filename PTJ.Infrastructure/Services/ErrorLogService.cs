using Microsoft.EntityFrameworkCore;
using PTJ.Application.Services;
using PTJ.Domain.Entities;
using PTJ.Infrastructure.Persistence;

namespace PTJ.Infrastructure.Services;

public class ErrorLogService : IErrorLogService
{
    private readonly AppDbContext _context;

    public ErrorLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogErrorAsync(
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
        string? additionalData = null)
    {
        try
        {
            var log = new SystemErrorLog
            {
                Level = level,
                Message = message,
                ExceptionType = exception?.GetType().FullName,
                StackTrace = exception?.StackTrace,
                InnerException = exception?.InnerException?.ToString(),
                UserId = userId,
                RequestPath = requestPath,
                HttpMethod = httpMethod,
                QueryString = queryString,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Source = source,
                Timestamp = DateTime.UtcNow,
                AdditionalData = additionalData
            };

            _context.SystemErrorLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log to console/file but don't throw - logging should never crash the app
            Console.WriteLine($"Failed to log system error: {ex.Message}");
        }
    }

    public async Task<(List<SystemErrorLog> logs, int totalCount)> GetErrorLogsAsync(
        string? level = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = _context.SystemErrorLogs
            .Include(l => l.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);

        if (startDate.HasValue)
            query = query.Where(l => l.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.Timestamp <= endDate.Value);

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }
}
