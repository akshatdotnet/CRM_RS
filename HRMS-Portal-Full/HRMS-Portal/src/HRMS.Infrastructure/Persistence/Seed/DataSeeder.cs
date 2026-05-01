using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRMS.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HrmsDbContext>>();

        // InMemory does not support migrations; EnsureCreated is sufficient
        await ctx.Database.EnsureCreatedAsync();

        if (await ctx.Users.AnyAsync()) return; // Already seeded

        logger.LogInformation("Seeding database with demo data...");

        // ── Admin User ──────────────────────────────────────────────────────
        var adminUser = new User
        {
            //Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Username = "admin",
            Email = "admin@acmecorp.in",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedBy = "seed"
        };

        // ── HR User ─────────────────────────────────────────────────────────
        var hrEmployee = new Employee
        {
            //Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            EmployeeCode = "EMP-0001",
            FirstName = "Priya", LastName = "Sharma",
            Email = "priya.sharma@acmecorp.in", Phone = "+91-9876543210",
            Department = Department.HumanResources, Designation = "HR Manager",
            JoiningDate = new DateTime(2022, 4, 1), Gender = Gender.Female,
            DateOfBirth = new DateTime(1990, 5, 15),
            PanNumber = "ABCPS1234D", Status = EmploymentStatus.Active,
            CreatedBy = "seed",
            SalaryStructure = new SalaryStructure
            {
                GrossMonthly = 80000m, BasicPercent = 40m, HRAPercent = 20m,
                SpecialAllowancePercent = 30m, ProfessionalTax = 200m,
                EffectiveFrom = new DateTime(2022, 4, 1), IsCurrent = true, CreatedBy = "seed"
            }
        };
        var hrUser = new User
        {
            //Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Username = "priya.sharma", Email = "priya.sharma@acmecorp.in",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Hr@12345"),
            Role = UserRole.HR, IsActive = true, EmployeeId = hrEmployee.Id, CreatedBy = "seed"
        };

        // ── Sample Employees ────────────────────────────────────────────────
        var emp1 = new Employee
        {
           // Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
            EmployeeCode = "EMP-0002",
            FirstName = "Rahul", LastName = "Verma",
            Email = "rahul.verma@acmecorp.in", Phone = "+91-9123456789",
            Department = Department.Engineering, Designation = "Senior Software Engineer",
            JoiningDate = new DateTime(2023, 1, 15), Gender = Gender.Male,
            DateOfBirth = new DateTime(1995, 8, 22),
            PanNumber = "ABCRV5678E", Status = EmploymentStatus.Active, CreatedBy = "seed",
            SalaryStructure = new SalaryStructure
            {
                GrossMonthly = 120000m, BasicPercent = 40m, HRAPercent = 20m,
                SpecialAllowancePercent = 30m, ProfessionalTax = 200m,
                EffectiveFrom = new DateTime(2023, 1, 15), IsCurrent = true, CreatedBy = "seed"
            }
        };
        var emp1User = new User
        {
            Username = "rahul.verma", Email = "rahul.verma@acmecorp.in",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Welcome@123"),
            Role = UserRole.Employee, IsActive = true, EmployeeId = emp1.Id, CreatedBy = "seed"
        };

        var emp2 = new Employee
        {
            //Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
            EmployeeCode = "EMP-0003",
            FirstName = "Anjali", LastName = "Mehta",
            Email = "anjali.mehta@acmecorp.in", Phone = "+91-9988776655",
            Department = Department.Finance, Designation = "Finance Analyst",
            JoiningDate = new DateTime(2021, 7, 5), Gender = Gender.Female,
            DateOfBirth = new DateTime(1993, 3, 11),
            PanNumber = "ABCAM9012F",
            Status = EmploymentStatus.OnNotice,
            ResignationDate = new DateTime(2025, 3, 1),
            LastWorkingDate = new DateTime(2025, 4, 30),
            CreatedBy = "seed",
            SalaryStructure = new SalaryStructure
            {
                GrossMonthly = 70000m, BasicPercent = 40m, HRAPercent = 20m,
                SpecialAllowancePercent = 30m, ProfessionalTax = 200m,
                EffectiveFrom = new DateTime(2021, 7, 5), IsCurrent = true, CreatedBy = "seed"
            }
        };
        var emp2User = new User
        {
            Username = "anjali.mehta", Email = "anjali.mehta@acmecorp.in",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Welcome@123"),
            Role = UserRole.Employee, IsActive = true, EmployeeId = emp2.Id, CreatedBy = "seed"
        };

        ctx.Users.AddRange(adminUser, hrUser, emp1User, emp2User);
        ctx.Employees.AddRange(hrEmployee, emp1, emp2);

        // ── Salary Slips (last 3 months for Rahul) ──────────────────────────
        var today = DateTime.UtcNow;
        for (int i = 1; i <= 3; i++)
        {
            var dt = today.AddMonths(-i);
            var sal = emp1.SalaryStructure!;
            ctx.SalarySlips.Add(new SalarySlip
            {
                EmployeeId = emp1.Id,
                Month = dt.Month, Year = dt.Year, PaidDays = DateTime.DaysInMonth(dt.Year, dt.Month),
                TotalDays = DateTime.DaysInMonth(dt.Year, dt.Month),
                Basic = sal.Basic, HRA = sal.HRA, SpecialAllowance = sal.SpecialAllowance,
                EmployeePF = sal.EmployeePF, ProfessionalTax = sal.ProfessionalTax,
                CreatedBy = "seed"
            });
        }

        await ctx.SaveChangesAsync();
        logger.LogInformation("Demo data seeded. Admin: admin@acmecorp.in / Admin@123");
    }
}
