using UserHub.Domain.Entities;
using UserHub.Infrastructure.Repositories.InMemory;
using BCrypt.Net;

namespace UserHub.Infrastructure.Seed;

/// <summary>
/// Seeds in-memory repositories with realistic mock data.
/// Replace with EF Core migrations + HasData() when switching to SQL Server.
/// </summary>
public static class DataSeeder
{
    public static void Seed(
        InMemoryUserRepository userRepo,
        InMemoryRoleRepository roleRepo,
        InMemoryModuleRepository moduleRepo,
        InMemoryAuditLogRepository auditRepo)
    {
        // ── Modules ──────────────────────────────────────────────────────────
        var modules = new List<Module>
        {
            Module.Create("Dashboard",  "Dashboard",    "Home",        "bi-speedometer2",    1),
            Module.Create("Users",      "Users",        "Users",       "bi-people-fill",     2),
            Module.Create("Roles",      "Roles",        "Roles",       "bi-shield-lock-fill",3),
            Module.Create("Modules",    "Modules",      "Modules",     "bi-grid-3x3-gap",    4),
            Module.Create("AuditLogs",  "Audit Logs",   "AuditLogs",   "bi-journal-text",    5),
        };
        moduleRepo.Seed(modules);

        var modDash    = modules[0];
        var modUsers   = modules[1];
        var modRoles   = modules[2];
        var modModules = modules[3];
        var modAudit   = modules[4];

        // ── Roles ─────────────────────────────────────────────────────────────
        var superAdminRole = Role.Create("SuperAdmin",  "Full system access",          isSuperAdmin: true);
        var adminRole      = Role.Create("Admin",       "Manage users and roles",      isSuperAdmin: false);
        var managerRole    = Role.Create("Manager",     "View and manage users",       isSuperAdmin: false);
        var viewerRole     = Role.Create("Viewer",      "Read-only access",            isSuperAdmin: false);

        var roles = new List<Role> { superAdminRole, adminRole, managerRole, viewerRole };
        roleRepo.Seed(roles);

        // ── Permissions for Admin ─────────────────────────────────────────────
        var adminPerms = new List<RoleModulePermission>
        {
            RoleModulePermission.Create(adminRole.Id, modDash.Id,    true,  false, false, false, true),
            RoleModulePermission.Create(adminRole.Id, modUsers.Id,   true,  true,  true,  true,  true),
            RoleModulePermission.Create(adminRole.Id, modRoles.Id,   true,  true,  true,  false, true),
            RoleModulePermission.Create(adminRole.Id, modModules.Id, true,  false, false, false, true),
            RoleModulePermission.Create(adminRole.Id, modAudit.Id,   true,  false, false, false, true),
        };

        // ── Permissions for Manager ───────────────────────────────────────────
        var managerPerms = new List<RoleModulePermission>
        {
            RoleModulePermission.Create(managerRole.Id, modDash.Id,  true,  false, false, false, true),
            RoleModulePermission.Create(managerRole.Id, modUsers.Id, true,  true,  true,  false, true),
        };

        // ── Permissions for Viewer ────────────────────────────────────────────
        var viewerPerms = new List<RoleModulePermission>
        {
            RoleModulePermission.Create(viewerRole.Id, modDash.Id,   true,  false, false, false, true),
            RoleModulePermission.Create(viewerRole.Id, modUsers.Id,  true,  false, false, false, true),
        };

        roleRepo.SeedPermissions(adminPerms);
        roleRepo.SeedPermissions(managerPerms);
        roleRepo.SeedPermissions(viewerPerms);

        // ── Users ─────────────────────────────────────────────────────────────
        var superUser   = User.Create("superadmin", "superadmin@userhub.local",
            BCrypt.Net.BCrypt.HashPassword("Admin@1234"), "Super", "Admin");
        var adminUser   = User.Create("admin", "admin@userhub.local",
            BCrypt.Net.BCrypt.HashPassword("Admin@1234"), "John", "Admin");
        var managerUser = User.Create("manager", "manager@userhub.local",
            BCrypt.Net.BCrypt.HashPassword("Manager@1234"), "Sarah", "Manager");
        var viewerUser  = User.Create("viewer", "viewer@userhub.local",
            BCrypt.Net.BCrypt.HashPassword("Viewer@1234"), "Mike", "Viewer");
        var extraUser1  = User.Create("jdoe", "jdoe@userhub.local",
            BCrypt.Net.BCrypt.HashPassword("User@1234"), "Jane", "Doe");
        var extraUser2  = User.Create("asmith", "asmith@userhub.local",
            BCrypt.Net.BCrypt.HashPassword("User@1234"), "Alex", "Smith");

        var users = new List<User> { superUser, adminUser, managerUser, viewerUser, extraUser1, extraUser2 };
        userRepo.SeedRoles(roles);
        userRepo.SeedUsers(users);

        // ── UserRoles ─────────────────────────────────────────────────────────
        var userRoles = new List<UserRole>
        {
            UserRole.Create(superUser.Id,   superAdminRole.Id),
            UserRole.Create(adminUser.Id,   adminRole.Id),
            UserRole.Create(managerUser.Id, managerRole.Id),
            UserRole.Create(viewerUser.Id,  viewerRole.Id),
            UserRole.Create(extraUser1.Id,  viewerRole.Id),
            UserRole.Create(extraUser2.Id,  managerRole.Id),
        };
        userRepo.SeedUserRoles(userRoles);

        // ── Audit Logs ────────────────────────────────────────────────────────
        var now = DateTime.UtcNow;
        auditRepo.AddAsync(AuditLog.Create(superUser.Id, "superadmin", "Login",      "Auth",  null, "127.0.0.1")).Wait();
        auditRepo.AddAsync(AuditLog.Create(adminUser.Id, "admin",      "Login",      "Auth",  null, "127.0.0.1")).Wait();
        auditRepo.AddAsync(AuditLog.Create(adminUser.Id, "admin",      "CreateUser", "Users", "Created user: jdoe")).Wait();
        auditRepo.AddAsync(AuditLog.Create(adminUser.Id, "admin",      "CreateUser", "Users", "Created user: asmith")).Wait();
    }
}
