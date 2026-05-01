using FluentValidation;
using FluentValidation.AspNetCore;
using HRMS.Application.Validators;
using HRMS.Infrastructure.Persistence.Seed;
using HRMS.Web.Extensions;
using HRMS.Web.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/hrms-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services
    .AddDatabase(builder.Configuration)
    .AddRepositories()
    .AddApplicationServices()
    .AddJwtAuthentication(builder.Configuration)
    .AddSwaggerWithJwt();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ── Seed demo data ────────────────────────────────────────────────────────────
await DataSeeder.SeedAsync(app.Services);

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRMS Portal API v1"));
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Log.Information("HRMS Portal started. Swagger: http://localhost:5000/swagger");
app.Run();
