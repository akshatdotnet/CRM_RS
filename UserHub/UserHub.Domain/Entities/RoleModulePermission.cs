namespace UserHub.Domain.Entities;

public class RoleModulePermission
{
    public Guid Id { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid ModuleId { get; private set; }
    public bool CanView { get; private set; }
    public bool CanCreate { get; private set; }
    public bool CanEdit { get; private set; }
    public bool CanDelete { get; private set; }
    public bool CanList { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Role? Role { get; private set; }
    public Module? Module { get; private set; }

    private RoleModulePermission() { }

    public static RoleModulePermission Create(Guid roleId, Guid moduleId,
        bool canView, bool canCreate, bool canEdit, bool canDelete, bool canList)
    {
        return new RoleModulePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            ModuleId = moduleId,
            CanView = canView,
            CanCreate = canCreate,
            CanEdit = canEdit,
            CanDelete = canDelete,
            CanList = canList,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(bool canView, bool canCreate, bool canEdit, bool canDelete, bool canList)
    {
        CanView = canView;
        CanCreate = canCreate;
        CanEdit = canEdit;
        CanDelete = canDelete;
        CanList = canList;
        UpdatedAt = DateTime.UtcNow;
    }
}
