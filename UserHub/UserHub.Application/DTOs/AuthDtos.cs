using System.ComponentModel.DataAnnotations;

namespace UserHub.Application.DTOs;

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class SessionUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public bool IsSuperAdmin { get; set; }
    public IDictionary<string, PermissionSetDto> Permissions { get; set; } = new Dictionary<string, PermissionSetDto>();
}

public class ModuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-grid";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
