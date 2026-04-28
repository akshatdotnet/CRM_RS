using CRM.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Data;

public class CrmDbContext : IdentityDbContext<ApplicationUser>
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options) { }

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<LeadActivity> LeadActivities => Set<LeadActivity>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyPhoto> PropertyPhotos => Set<PropertyPhoto>();
    public DbSet<PropertyEnquiry> PropertyEnquiries => Set<PropertyEnquiry>();
    public DbSet<DealConfirmation> DealConfirmations => Set<DealConfirmation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Required for Identity tables

        modelBuilder.Entity<Lead>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Stage).HasMaxLength(50).HasDefaultValue("New");
            e.Property(x => x.BudgetMin).HasColumnType("decimal(18,2)");
            e.Property(x => x.BudgetMax).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Agent).WithMany(a => a.Leads).HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Activities).WithOne(a => a.Lead).HasForeignKey(a => a.LeadId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Customer).WithOne(c => c.Lead).HasForeignKey<Customer>(c => c.LeadId);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            e.Property(x => x.DealValue).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Agent).WithMany(a => a.Customers).HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Agent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            e.Property(x => x.Email).HasMaxLength(150);
        });

        modelBuilder.Entity<LeadActivity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ActivityType).HasMaxLength(100);
            e.Property(x => x.FromStage).HasMaxLength(50);
            e.Property(x => x.ToStage).HasMaxLength(50);
        });

        modelBuilder.Entity<Property>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(300);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.PublicSlug).HasMaxLength(400);
            e.HasIndex(x => x.PublicSlug).IsUnique();
            e.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedByAgentId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Photos).WithOne(p => p.Property).HasForeignKey(p => p.PropertyId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Enquiries).WithOne(p => p.Property).HasForeignKey(p => p.PropertyId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.DealConfirmations).WithOne(d => d.Property).HasForeignKey(d => d.PropertyId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropertyPhoto>(e => { e.HasKey(x => x.Id); e.Property(x => x.FilePath).HasMaxLength(500); });
        modelBuilder.Entity<PropertyEnquiry>(e => { e.HasKey(x => x.Id); e.Property(x => x.ClientName).HasMaxLength(150); e.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("New"); });
        modelBuilder.Entity<DealConfirmation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ClientName).HasMaxLength(150);
            e.Property(x => x.OfferedPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.ConfirmationToken).HasMaxLength(64);
            e.HasIndex(x => x.ConfirmationToken).IsUnique();
            e.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Pending");
            e.HasOne(x => x.ReviewedBy).WithMany().HasForeignKey(x => x.ReviewedByAgentId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        // Seed data
        modelBuilder.Entity<Agent>().HasData(
            new Agent { Id = 1, FullName = "Ajay Sharma",  Email = "ajay@crm.com",  Phone = "+91 98200 11111" },
            new Agent { Id = 2, FullName = "Meera Rao",    Email = "meera@crm.com", Phone = "+91 98200 22222" },
            new Agent { Id = 3, FullName = "Rajan Patel",  Email = "rajan@crm.com", Phone = "+91 98200 33333" }
        );
        modelBuilder.Entity<Lead>().HasData(
            new Lead { Id = 1, FullName = "Ramesh Kumar", Phone = "+91 98200 11234", Email = "ramesh@email.com",  LeadSource = "99acres",     PropertyType = "3BHK",  LocationPreference = "Andheri West", BudgetMin = 1800000, BudgetMax = 2200000, Stage = "New",         AgentId = 1, FollowUpDeadline = DateTime.UtcNow.AddDays(7),  CloseByDate = DateTime.UtcNow.AddDays(30),  CreatedAt = DateTime.UtcNow },
            new Lead { Id = 2, FullName = "Priya Shah",   Phone = "+91 98700 55678", Email = "priya@email.com",  LeadSource = "MagicBricks", PropertyType = "2BHK",  LocationPreference = "Powai",        BudgetMin = 1100000, BudgetMax = 1400000, Stage = "Contacted",   AgentId = 2, FollowUpDeadline = DateTime.UtcNow.AddDays(2),  CloseByDate = DateTime.UtcNow.AddDays(20),  CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new Lead { Id = 3, FullName = "Amit Mehta",   Phone = "+91 99100 77890", Email = "amit@email.com",   LeadSource = "Referral",    PropertyType = "4BHK",  LocationPreference = "Juhu",         BudgetMin = 5000000, BudgetMax = 6000000, Stage = "Negotiation", AgentId = 1, FollowUpDeadline = DateTime.UtcNow.AddDays(1),  CloseByDate = DateTime.UtcNow.AddDays(10),  CreatedAt = DateTime.UtcNow.AddDays(-12) },
            new Lead { Id = 4, FullName = "Sunita Joshi", Phone = "+91 98450 33456", Email = "sunita@email.com", LeadSource = "Referral",    PropertyType = "Villa", LocationPreference = "Lonavala",     BudgetMin = 2400000, BudgetMax = 2400000, Stage = "Closed",      AgentId = 2, FollowUpDeadline = DateTime.UtcNow.AddDays(-5), CloseByDate = DateTime.UtcNow.AddDays(-2),  CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new Lead { Id = 5, FullName = "Deepa Nair",   Phone = "+91 98600 44321", Email = "deepa@email.com",  LeadSource = "99acres",     PropertyType = "2BHK",  LocationPreference = "Bandra",       BudgetMin = 2000000, BudgetMax = 2500000, Stage = "Site Visit",  AgentId = 3, FollowUpDeadline = DateTime.UtcNow.AddDays(-1), CloseByDate = DateTime.UtcNow.AddDays(15),  CreatedAt = DateTime.UtcNow.AddDays(-8) }
        );
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, FullName = "Sunita Joshi", Phone = "+91 98450 33456", Email = "sunita@email.com", PropertyPurchased = "Villa, Lonavala", DealValue = 2400000, DealClosedDate = DateTime.UtcNow.AddDays(-2), Source = "Referral", LeadId = 4, AgentId = 2, CreatedAt = DateTime.UtcNow.AddDays(-2) }
        );
        modelBuilder.Entity<Property>().HasData(
            new Property { Id = 1, Title = "Spacious 3BHK with Sea View", PropertyType = "3BHK", Status = "Available", Price = 18000000, PriceLabel = "₹1.8 Cr", Location = "Andheri West", Address = "Flat 12B, Sea Breeze Tower, Versova Road, Andheri West", City = "Mumbai", AreaSqFt = 1450, Bedrooms = 3, Bathrooms = 2, Floors = 12, YearBuilt = 2019, IsFurnished = true, HasParking = true, HasGym = true, HasPool = false, HasSecurity = true, Amenities = "Club House, Power Backup, Children Play Area", PublicSlug = "3bhk-andheri-west-ab12cd", IsPublished = true, IsActive = true, CreatedByAgentId = 1, CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow.AddDays(-10), Description = "A stunning 3BHK apartment with panoramic sea views in the heart of Andheri West." },
            new Property { Id = 2, Title = "Luxury Villa with Private Pool", PropertyType = "Villa", Status = "Available", Price = 45000000, PriceLabel = "₹4.5 Cr", Location = "Lonavala", Address = "Plot 7, Green Valley Estate, Lonavala", City = "Pune", AreaSqFt = 4200, Bedrooms = 4, Bathrooms = 4, Floors = 2, YearBuilt = 2021, IsFurnished = true, HasParking = true, HasGym = false, HasPool = true, HasSecurity = true, Amenities = "Private Pool, Garden, Servant Quarters, Solar Power", PublicSlug = "villa-lonavala-ef34gh", IsPublished = true, IsActive = true, CreatedByAgentId = 2, CreatedAt = DateTime.UtcNow.AddDays(-7), UpdatedAt = DateTime.UtcNow.AddDays(-7), Description = "An exquisite luxury villa in Lonavala with private pool and valley views." }
        );
    }
}
