using PTJ.Domain.Common;

namespace PTJ.Domain.Entities;

public class ChatMessage : BaseAuditableEntity
{
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public virtual ChatConversation Conversation { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
}
