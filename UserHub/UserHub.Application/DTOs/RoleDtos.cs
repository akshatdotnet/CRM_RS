using System.ComponentModel.DataAnnotations;

namespace UserHub.Application.DTOs;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
}

public class CreateRoleDto
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string Description { get; set; } = string.Empty;

    public bool IsSuperAdmin { get; set; }
}

public class EditRoleDto
{
    public Guid Id { get; set; }

    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string Description { get; set; } = string.Empty;
}

public class PermissionSetDto
{
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleDisplayName { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanList { get; set; }
}

public class SetPermissionsDto
{
    public Guid RoleId { get; set; }
    public IEnumerable<PermissionSetDto> Permissions { get; set; } = Enumerable.Empty<PermissionSetDto>();
}
