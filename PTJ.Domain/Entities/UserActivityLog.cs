namespace PTJ.Domain.Entities;

/// <summary>
/// Entity to track user activity logs
/// </summary>
public class UserActivityLog
{
    public long Id { get; set; }
    
    /// <summary>
    /// User ID from JWT claims (nullable for anonymous requests)
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Request path (e.g., /api/auth/login)
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// Query string parameters
    /// </summary>
    public string? QueryString { get; set; }
    
    /// <summary>
    /// Client IP address
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// User agent string from request header
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// HTTP status code of the response
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Request processing duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; }
    
    /// <summary>
    /// Timestamp when the request was made (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Additional context or metadata (JSON format)
    /// </summary>
    public string? AdditionalData { get; set; }
    
    // Navigation property
    public User? User { get; set; }
}
