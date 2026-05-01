using System.Text;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Application.Mappings;
using HRMS.Application.Services;
using HRMS.Infrastructure.Persistence;
using HRMS.Infrastructure.Repositories;
using HRMS.Infrastructure.Services;
using HRMS.Infrastructure.Services.Email;
//using HRMS.Infrastructure.Services.Jwt;
using HRMS.Infrastructure.Services.Pdf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace HRMS.Web.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<HrmsDbContext>(options =>
        {
            if (config.GetValue<bool>("Database:UseInMemory", true))
                options.UseInMemoryDatabase("HrmsDb");
            else
                options.UseSqlServer(config.GetConnectionString("Default"));
        });
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISalarySlipRepository, SalarySlipRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ISalarySlipService, SalarySlipService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var key = Encoding.UTF8.GetBytes(config["Jwt:Secret"]!);
        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true, ValidIssuer = config["Jwt:Issuer"],
                ValidateAudience = true, ValidAudience = config["Jwt:Audience"],
                ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

        services.AddAuthorization(o =>
        {
            o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            o.AddPolicy("HROrAdmin", p => p.RequireRole("Admin", "HR"));
            o.AddPolicy("AllRoles", p => p.RequireRole("Admin", "HR", "Employee"));
        });
        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "HRMS Portal API", Version = "v1",
                Description = "Human Resource Management System - Phase 1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization", Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer", BearerFormat = "JWT", In = ParameterLocation.Header,
                Description = "Enter: Bearer {your_jwt_token}"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {{ 
                new OpenApiSecurityScheme { Reference = new OpenApiReference 
                    { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>() }});
        });
        return services;
    }
}
