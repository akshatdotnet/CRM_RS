using Microsoft.Extensions.Http;
using PersonalBrand.MVC.Services;
using Polly;
using Polly.Extensions.Http;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/mvc-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ─── API Base URL from config ─────────────────────────
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/";

    // ─── Polly Retry + Circuit Breaker Policies ───────────
    var retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(300 * attempt),
            (outcome, delay, attempt, _) =>
                Log.Warning("API retry {Attempt} after {Delay}ms — {Reason}", attempt, delay.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()));

    var circuitBreakerPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
            (outcome, duration) => Log.Warning("Circuit breaker OPEN for {Duration}s", duration.TotalSeconds),
            () => Log.Information("Circuit breaker CLOSED — API is healthy again"));

    // ─── Typed HttpClient ─────────────────────────────────
    builder.Services.AddHttpClient<PersonalBrandApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "PersonalBrand-MVC/1.0");
        client.Timeout = TimeSpan.FromSeconds(15);
    })
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

    // ─── MVC Services ─────────────────────────────────────
    builder.Services.AddControllersWithViews()
        .AddRazorRuntimeCompilation()
        .AddJsonOptions(opt =>
            opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

    builder.Services.AddMemoryCache();
    builder.Services.AddSession(opt =>
    {
        opt.IdleTimeout = TimeSpan.FromMinutes(30);
        opt.Cookie.HttpOnly = true;
        opt.Cookie.IsEssential = true;
    });
    builder.Services.AddResponseCompression();
    builder.Services.AddAntiforgery(opt => opt.HeaderName = "X-CSRF-TOKEN");

    var app = builder.Build();

    // ─── Middleware Pipeline ───────────────────────────────
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseResponseCompression();
    app.UseHttpsRedirection();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Cache static assets 30 days
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=2592000");
        }
    });
    app.UseRouting();
    app.UseSession();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("PersonalBrand MVC starting");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MVC host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
