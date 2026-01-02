using PTJ.Domain.Common;

namespace PTJ.Domain.Entities;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

