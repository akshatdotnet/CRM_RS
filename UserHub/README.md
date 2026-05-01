# UserHub — ASP.NET Core 8 MVC User Management System

## Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 / VS Code / Rider

### Run (InMemory mode — no DB needed)
```bash
cd UserHub.Web
dotnet run
```
Navigate to `https://localhost:5001`. Login with:

| User         | Password      | Role       |
|-------------|---------------|------------|
| superadmin  | Admin@1234    | SuperAdmin |
| admin       | Admin@1234    | Admin      |
| manager     | Manager@1234  | Manager    |
| viewer      | Viewer@1234   | Viewer     |

---

## Architecture

```
UserHub.Domain          → Entities, Interfaces (pure — no EF, no UI)
UserHub.Application     → Services, DTOs, Use-case orchestration
UserHub.Infrastructure  → Repositories (InMemory now, EF Core ready)
UserHub.Web             → MVC Controllers, Razor Views, Filters
UserHub.Shared          → Constants, Extensions
```

### Dependency Rule
```
Web → Application → Domain
Infrastructure → Domain
```

---

## Switching to EF Core + SQL Server

1. **Uncomment** `AppDbContext.cs` in `UserHub.Infrastructure/Data/`
2. **Update** `UserHub.Infrastructure.csproj` — uncomment EF packages
3. **Replace** InMemory registrations in `Program.cs`:
```csharp
// Remove:
builder.Services.AddSingleton<IUserRepository>(userRepo);
// Add:
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
// etc.
```
4. **Create** EF Core repositories in `Infrastructure/Repositories/EFCore/`
   matching the same `IUserRepository`, `IRoleRepository` etc. interfaces.
5. **Run migrations**:
```bash
dotnet ef migrations add InitialCreate --project UserHub.Infrastructure --startup-project UserHub.Web
dotnet ef database update --startup-project UserHub.Web
```

---

## Key Design Decisions

**Session auth (not ASP.NET Identity)** — intentional per spec. Session data
is stored server-side; `SessionUserDto` is serialized to session on login and
refreshed on each permission-sensitive request.

**Permission union merge** — if a user has multiple roles, permissions are
OR'd across all roles. Granting beats denying. SuperAdmin bypasses all checks.

**Soft delete** — `IsDeleted` flag; data is never physically removed from DB
(when using EF Core). Unique constraints still apply to non-deleted records.

**InMemory repos are Singleton** — required because InMemory state lives in
memory. When switching to EF Core, change to `AddScoped<>`.

**No GET deletes** — all destructive actions are POST + AntiForgeryToken.

---

## Security Checklist
- [x] BCrypt password hashing (cost factor 11 default)
- [x] Session cookie: HttpOnly, SameSite=Strict, Secure
- [x] CSRF token on all POST forms
- [x] RequireLogin filter on all authenticated routes
- [x] RequirePermission filter with module+action granularity
- [x] Soft delete (no accidental permanent loss)
- [x] Redirect to ReturnUrl only for local URLs (Url.IsLocalUrl check)
- [x] No GET deletes
