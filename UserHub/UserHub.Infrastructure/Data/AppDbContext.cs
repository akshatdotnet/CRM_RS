// ────────────────────────────────────────────────────────────────────────────
// EF CORE DbContext — FUTURE USE
// Uncomment and install packages when switching from InMemory to SQL Server:
//   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
//   dotnet add package Microsoft.EntityFrameworkCore.Tools
//   dotnet ef migrations add InitialCreate
//   dotnet ef database update
// ────────────────────────────────────────────────────────────────────────────

// using Microsoft.EntityFrameworkCore;
// using UserHub.Domain.Entities;
//
// namespace UserHub.Infrastructure.Data;
//
// public class AppDbContext : DbContext
// {
//     public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
//
//     public DbSet<User> Users => Set<User>();
//     public DbSet<Role> Roles => Set<Role>();
//     public DbSet<Module> Modules => Set<Module>();
//     public DbSet<UserRole> UserRoles => Set<UserRole>();
//     public DbSet<RoleModulePermission> RoleModulePermissions => Set<RoleModulePermission>();
//     public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
//
//     protected override void OnModelCreating(ModelBuilder builder)
//     {
//         // User
//         builder.Entity<User>(e => {
//             e.HasKey(x => x.Id);
//             e.HasIndex(x => x.Username).IsUnique();
//             e.HasIndex(x => x.Email).IsUnique();
//             e.Property(x => x.Username).HasMaxLength(50).IsRequired();
//             e.Property(x => x.Email).HasMaxLength(200).IsRequired();
//             e.Property(x => x.PasswordHash).IsRequired();
//             e.Property(x => x.FirstName).HasMaxLength(100);
//             e.Property(x => x.LastName).HasMaxLength(100);
//             e.HasQueryFilter(x => !x.IsDeleted);
//         });
//
//         // Role
//         builder.Entity<Role>(e => {
//             e.HasKey(x => x.Id);
//             e.HasIndex(x => x.Name).IsUnique();
//             e.Property(x => x.Name).HasMaxLength(50).IsRequired();
//             e.Property(x => x.Description).HasMaxLength(250);
//         });
//
//         // Module
//         builder.Entity<Module>(e => {
//             e.HasKey(x => x.Id);
//             e.HasIndex(x => x.Name).IsUnique();
//             e.Property(x => x.Name).HasMaxLength(50).IsRequired();
//             e.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
//             e.Property(x => x.ControllerName).HasMaxLength(100).IsRequired();
//             e.Property(x => x.Icon).HasMaxLength(50);
//         });
//
//         // UserRole
//         builder.Entity<UserRole>(e => {
//             e.HasKey(x => x.Id);
//             e.HasIndex(x => new { x.UserId, x.RoleId });
//             e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
//             e.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId);
//         });
//
//         // RoleModulePermission
//         builder.Entity<RoleModulePermission>(e => {
//             e.HasKey(x => x.Id);
//             e.HasIndex(x => new { x.RoleId, x.ModuleId }).IsUnique();
//             e.HasOne(x => x.Role).WithMany(r => r.Permissions).HasForeignKey(x => x.RoleId);
//             e.HasOne(x => x.Module).WithMany(m => m.Permissions).HasForeignKey(x => x.ModuleId);
//         });
//
//         // AuditLog
//         builder.Entity<AuditLog>(e => {
//             e.HasKey(x => x.Id);
//             e.Property(x => x.Username).HasMaxLength(50).IsRequired();
//             e.Property(x => x.Action).HasMaxLength(100).IsRequired();
//             e.Property(x => x.Module).HasMaxLength(50).IsRequired();
//             e.Property(x => x.Details).HasMaxLength(500);
//             e.Property(x => x.IpAddress).HasMaxLength(45);
//         });
//     }
// }
