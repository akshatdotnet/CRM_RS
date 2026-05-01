namespace HRMS.Domain.Common;

/// <summary>
/// Base entity with audit trail. Every aggregate root inherits this.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = "system";
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void SetAuditOnCreate(string createdBy)
    {
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetAuditOnUpdate(string updatedBy)
    {
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
