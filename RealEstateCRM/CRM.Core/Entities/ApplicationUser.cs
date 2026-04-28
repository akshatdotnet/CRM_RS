using Microsoft.AspNetCore.Identity;

namespace CRM.Core.Entities;

/// <summary>
/// Extends IdentityUser with CRM-specific profile fields.
/// Roles are managed via ASP.NET Core Identity RoleManager.
/// Roles: Admin, SeniorAgent, Agent
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePhoto { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Link to Agent entity (nullable — Admins may not be agents)
    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }
}

/// <summary>CRM roles — stored in AspNetRoles table.</summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string SeniorAgent = "SeniorAgent";
    public const string Agent = "Agent";

    public static readonly string[] All = { Admin, SeniorAgent, Agent };
}
