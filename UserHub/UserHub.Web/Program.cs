using UserHub.Application.Interfaces;
using UserHub.Application.Services;
using UserHub.Domain.Interfaces;
using UserHub.Infrastructure.Repositories.InMemory;
using UserHub.Infrastructure.Seed;
using UserHub.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Session ───────────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(AppConstants.SessionTimeoutMin);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Name = ".UserHub.Session";
});

builder.Services.AddHttpContextAccessor();

// ── InMemory Repositories (Singleton — shared state across requests) ──────
// TO SWITCH TO EF CORE:
//   1. Uncomment AppDbContext in Infrastructure/Data/AppDbContext.cs
//   2. Replace these registrations with:
//      builder.Services.AddScoped<IUserRepository, EfUserRepository>();
//      builder.Services.AddScoped<IRoleRepository, EfRoleRepository>();  etc.
//   3. Run: dotnet ef migrations add Init && dotnet ef database update

var userRepo   = new InMemoryUserRepository();
var roleRepo   = new InMemoryRoleRepository();
var moduleRepo = new InMemoryModuleRepository();
var auditRepo  = new InMemoryAuditLogRepository();

DataSeeder.Seed(userRepo, roleRepo, moduleRepo, auditRepo);

builder.Services.AddSingleton<IUserRepository>(userRepo);
builder.Services.AddSingleton<IRoleRepository>(roleRepo);
builder.Services.AddSingleton<IModuleRepository>(moduleRepo);
builder.Services.AddSingleton<IAuditLogRepository>(auditRepo);

// ── Application Services ──────────────────────────────────────────────────
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService,  AuthService>();
builder.Services.AddScoped<IUserService,  UserService>();
builder.Services.AddScoped<IRoleService,  RoleService>();
builder.Services.AddScoped<IModuleService, ModuleService>();

// ── Antiforgery ───────────────────────────────────────────────────────────
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
