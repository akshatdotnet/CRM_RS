using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Email).IsUnique();
        b.HasIndex(e => e.EmployeeCode).IsUnique();
        b.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        b.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        b.Property(e => e.Email).IsRequired().HasMaxLength(255);
        b.Property(e => e.Phone).IsRequired().HasMaxLength(20);
        b.Property(e => e.Designation).IsRequired().HasMaxLength(150);
        b.Property(e => e.PanNumber).HasMaxLength(10);
        b.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
        b.HasOne(e => e.SalaryStructure).WithOne(s => s.Employee)
            .HasForeignKey<SalaryStructure>(s => s.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(e => e.Documents).WithOne(d => d.Employee)
            .HasForeignKey(d => d.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(e => e.SalarySlips).WithOne(s => s.Employee)
            .HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> b)
    {
        b.HasKey(s => s.Id);
        b.Property(s => s.GrossMonthly).HasColumnType("decimal(18,2)");
        b.Property(s => s.BasicPercent).HasColumnType("decimal(5,2)");
        b.Property(s => s.HRAPercent).HasColumnType("decimal(5,2)");
        b.Property(s => s.SpecialAllowancePercent).HasColumnType("decimal(5,2)");
        b.Property(s => s.PFPercent).HasColumnType("decimal(5,2)");
        b.Property(s => s.ProfessionalTax).HasColumnType("decimal(10,2)");
        b.Property(s => s.IncomeTaxMonthly).HasColumnType("decimal(10,2)");
        b.Ignore(s => s.Basic); b.Ignore(s => s.HRA); b.Ignore(s => s.SpecialAllowance);
        b.Ignore(s => s.EmployeePF); b.Ignore(s => s.EmployerPF);
        b.Ignore(s => s.NetMonthly); b.Ignore(s => s.AnnualCTC);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);
        b.HasIndex(u => u.Email).IsUnique();
        b.HasIndex(u => u.Username).IsUnique();
        b.Property(u => u.Email).IsRequired().HasMaxLength(255);
        b.Property(u => u.Username).IsRequired().HasMaxLength(100);
        b.Property(u => u.PasswordHash).IsRequired();
        b.HasOne(u => u.Employee).WithOne()
            .HasForeignKey<User>(u => u.EmployeeId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.HasKey(d => d.Id);
        b.Property(d => d.TemplateSnapshot).IsRequired();
    }
}

public class SalarySlipConfiguration : IEntityTypeConfiguration<SalarySlip>
{
    public void Configure(EntityTypeBuilder<SalarySlip> b)
    {
        b.HasKey(s => s.Id);
        b.HasIndex(s => new { s.EmployeeId, s.Month, s.Year }).IsUnique();
        foreach (var col in new[] { "Basic","HRA","SpecialAllowance","OtherAllowances",
            "EmployeePF","ProfessionalTax","IncomeTax","OtherDeductions" })
            b.Property(col).HasColumnType("decimal(18,2)");
        b.Ignore(s => s.GrossEarnings); b.Ignore(s => s.TotalDeductions);
        b.Ignore(s => s.NetPay); b.Ignore(s => s.MonthYear);
    }
}
