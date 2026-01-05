using PTJ.Domain.Common;

namespace PTJ.Domain.Entities;

public class AIChatSession : BaseAuditableEntity
{
    public int UserId { get; set; }
    public string Title { get; set; } = "New Chat";
    public bool IsActive { get; set; } = true;
    public DateTime? EndedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual ICollection<AIChatMessage> Messages { get; set; } = new List<AIChatMessage>();
}
