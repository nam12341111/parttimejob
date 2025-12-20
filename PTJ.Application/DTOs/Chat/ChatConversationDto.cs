namespace PTJ.Application.DTOs.Chat;

public class ChatConversationDto
{
    public int Id { get; set; }
    public int EmployerId { get; set; }
    public string EmployerName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int? JobPostId { get; set; }
    public string? JobPostTitle { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
