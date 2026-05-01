using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence;

public class HrmsDbContext : DbContext
{
    public HrmsDbContext(DbContextOptions<HrmsDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<SalarySlip> SalarySlips => Set<SalarySlip>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmsDbContext).Assembly);

        // Global query filter: soft deletes
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Document>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<SalarySlip>().HasQueryFilter(s => !s.IsDeleted);
    }
}
