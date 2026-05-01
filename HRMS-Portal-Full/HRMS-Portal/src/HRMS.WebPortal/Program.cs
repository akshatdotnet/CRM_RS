using Microsoft.AspNetCore.Authentication.Cookies;
using HRMS.WebPortal.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ───────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/portal-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ── MVC + Cookie Auth ─────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath    = "/Auth/Login";
        options.LogoutPath   = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly  = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// ── ApiClient (typed HTTP client → HRMS.Web API) ──────────────────────────────
var apiBase = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/";
builder.Services.AddHttpClient<ApiClient>(c => c.BaseAddress = new Uri(apiBase));

builder.Services.AddAntiforgery();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

Log.Information("HRMS Web Portal started → http://localhost:5001");
app.Run();
