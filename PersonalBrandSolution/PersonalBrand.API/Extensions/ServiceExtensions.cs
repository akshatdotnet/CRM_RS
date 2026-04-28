using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PersonalBrand.API.Data;
using PersonalBrand.API.Models.Entities;
using PersonalBrand.API.Repositories.Implementations;
using PersonalBrand.API.Repositories.Interfaces;
using PersonalBrand.API.Services.Implementations;
using PersonalBrand.API.Services.Interfaces;

namespace PersonalBrand.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        // Use SQLite in dev (easy local run), SQL Server in production
        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains(".db"))
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlite(connectionString ?? "Data Source=personalbrand.db")
                   .EnableSensitiveDataLogging(false)
                   .EnableDetailedErrors(false));
        }
        else
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(connectionString,
                    sqlOpts => sqlOpts.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));
        }
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPersonaRepository, PersonaRepository>();
        services.AddScoped<IRepository<Skill>, Repository<Skill>>();
        services.AddScoped<IRepository<RoadmapItem>, Repository<RoadmapItem>>();
        services.AddScoped<IRepository<Course>, Repository<Course>>();
        services.AddScoped<IRepository<Project>, Repository<Project>>();
        services.AddScoped<IQARepository, QARepository>();
        services.AddScoped<IRepository<PersonalBrand.API.Models.Entities.ConsultingService>, Repository<PersonalBrand.API.Models.Entities.ConsultingService>>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IRepository<Testimonial>, Repository<Testimonial>>();
        services.AddScoped<ISubscriberRepository, SubscriberRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPersonaService, PersonaService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<IRoadmapService, RoadmapService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IQAService, QAServiceImpl>();
        services.AddScoped<IConsultingService, ConsultingServiceImpl>();
        services.AddScoped<ILeadService, LeadServiceImpl>();
        services.AddScoped<IBlogService, BlogServiceImpl>();
        services.AddScoped<ITestimonialService, TestimonialServiceImpl>();
        services.AddScoped<INewsletterService, NewsletterServiceImpl>();
        return services;
    }

    public static IServiceCollection AddApiVersioningConfig(this IServiceCollection services)
    {
        services.AddApiVersioning(opt =>
        {
            opt.DefaultApiVersion = new ApiVersion(1, 0);
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.ReportApiVersions = true;
        }).AddMvc().AddApiExplorer(opt =>
        {
            opt.GroupNameFormat = "'v'VVV";
            opt.SubstituteApiVersionInUrl = true;
        });
        return services;
    }

    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PersonalBrand API",
                Version = "v1",
                Description = "API for the Arjun Sharma Personal Brand Website — .NET Core & Azure Expert",
                Contact = new OpenApiContact { Name = "Arjun Sharma", Email = "arjun@dotnetpro.dev" }
            });

        });
        return services;
    }

    public static IServiceCollection AddCorsConfig(this IServiceCollection services, IConfiguration config)
    {
        var allowedOrigins = config.GetSection("AllowedOrigins").Get<string[]>()
                             ?? ["http://localhost:5001", "https://localhost:7001"];
        services.AddCors(opt =>
            opt.AddPolicy("MvcPolicy", policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()));
        return services;
    }

    // ─── Database Migration + Seed ────────────────────────
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync(); // For SQLite dev — use MigrateAsync() for prod
    }
}
