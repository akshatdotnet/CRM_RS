using PersonalBrand.API.Extensions;
using PersonalBrand.API.Middleware;
using Serilog;

// ─── Serilog Bootstrap ────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ─── Services ─────────────────────────────────────────
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddRepositories();
    builder.Services.AddApplicationServices();
    builder.Services.AddMemoryCache();
    builder.Services.AddApiVersioningConfig();
    builder.Services.AddSwaggerConfig();
    builder.Services.AddCorsConfig(builder.Configuration);
    builder.Services.AddControllers()
        .AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            opt.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddResponseCompression();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // ─── Middleware Pipeline ───────────────────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonalBrand API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseResponseCompression();
    app.UseHttpsRedirection();
    app.UseCors("MvcPolicy");
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    // ─── Seed Database ─────────────────────────────────────
    await app.MigrateAndSeedAsync();

    Log.Information("PersonalBrand API starting on {Urls}", string.Join(", ", app.Urls));
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
