using PTJ.Domain.Common;

namespace PTJ.Domain.Entities;

public class ChatConversation : BaseAuditableEntity
{
    public int EmployerId { get; set; }
    public int StudentId { get; set; }
    public int? JobPostId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessage { get; set; }
    public bool IsEmployerTyping { get; set; }
    public bool IsStudentTyping { get; set; }

    // Navigation properties
    public virtual User Employer { get; set; } = null!;
    public virtual User Student { get; set; } = null!;
    public virtual JobPost? JobPost { get; set; }
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
