using Microsoft.EntityFrameworkCore;
using PTJ.Application.Services;
using PTJ.Domain.Entities;
using PTJ.Infrastructure.Persistence;

namespace PTJ.Infrastructure.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly AppDbContext _context;

    public ActivityLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogActivityAsync(
        int? userId,
        string httpMethod,
        string path,
        string? queryString,
        string ipAddress,
        string? userAgent,
        int statusCode,
        long durationMs,
        string? additionalData = null)
    {
        try
        {
            var log = new UserActivityLog
            {
                UserId = userId,
                HttpMethod = httpMethod,
                Path = path,
                QueryString = queryString,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                StatusCode = statusCode,
                DurationMs = durationMs,
                Timestamp = DateTime.UtcNow,
                AdditionalData = additionalData
            };

            _context.UserActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log to console/file but don't throw - logging should never crash the app
            Console.WriteLine($"Failed to log user activity: {ex.Message}");
        }
    }

    public async Task<(List<UserActivityLog> logs, int totalCount)> GetActivityLogsAsync(
        int? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = _context.UserActivityLogs
            .Include(l => l.User)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId.Value);

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
