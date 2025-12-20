namespace PTJ.Application.DTOs.Chat;

public class SendMessageDto
{
    public int? ConversationId { get; set; }
    public int? RecipientId { get; set; }
    public int? JobPostId { get; set; }
    public string Content { get; set; } = string.Empty;
}
