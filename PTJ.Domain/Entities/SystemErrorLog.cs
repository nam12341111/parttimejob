namespace PTJ.Domain.Entities;

/// <summary>
/// Entity to store system error logs
/// </summary>
public class SystemErrorLog
{
    public long Id { get; set; }
    
    /// <summary>
    /// Error severity level (Error, Critical, Warning, etc.)
    /// </summary>
    public string Level { get; set; } = string.Empty;
    
    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Exception type (e.g., System.NullReferenceException)
    /// </summary>
    public string? ExceptionType { get; set; }
    
    /// <summary>
    /// Full stack trace of the error
    /// </summary>
    public string? StackTrace { get; set; }
    
    /// <summary>
    /// Inner exception details
    /// </summary>
    public string? InnerException { get; set; }
    
    /// <summary>
    /// User ID if the error occurred in an authenticated context
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Request path where the error occurred
    /// </summary>
    public string? RequestPath { get; set; }
    
    /// <summary>
    /// HTTP method of the request
    /// </summary>
    public string? HttpMethod { get; set; }
    
    /// <summary>
    /// Query string parameters
    /// </summary>
    public string? QueryString { get; set; }
    
    /// <summary>
    /// Client IP address
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Timestamp when the error occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Additional context or metadata (JSON format)
    /// </summary>
    public string? AdditionalData { get; set; }
    
    /// <summary>
    /// Source of the error (e.g., controller name, service name)
    /// </summary>
    public string? Source { get; set; }
    
    // Navigation property
    public User? User { get; set; }
}
