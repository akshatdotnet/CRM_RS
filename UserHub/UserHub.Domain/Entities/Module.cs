namespace UserHub.Domain.Entities;

public class Module
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string ControllerName { get; private set; } = string.Empty;
    public string Icon { get; private set; } = "bi-grid";
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<RoleModulePermission> Permissions { get; private set; } = new List<RoleModulePermission>();

    private Module() { }

    public static Module Create(string name, string displayName, string controllerName, string icon = "bi-grid", int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Module name required.");
        return new Module
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            DisplayName = displayName.Trim(),
            ControllerName = controllerName.Trim(),
            Icon = icon,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string displayName, string icon, int sortOrder)
    {
        DisplayName = displayName.Trim();
        Icon = icon;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
}
