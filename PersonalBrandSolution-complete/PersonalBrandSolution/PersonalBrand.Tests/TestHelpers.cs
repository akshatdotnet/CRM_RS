using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PersonalBrand.API.Data;
using PersonalBrand.API.Models.Entities;
using PersonalBrand.Shared.DTOs;
using PersonalBrand.Shared.Models;

namespace PersonalBrand.Tests;

// ─── InMemory DB Factory ──────────────────────────────
public static class TestDbFactory
{
    public static AppDbContext CreateInMemory(string? dbName = null)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        var ctx = new AppDbContext(opts);
        ctx.Database.EnsureCreated(); // applies seed data
        return ctx;
    }

    /// <summary>Create fresh context WITHOUT seed data for controlled tests</summary>
    public static AppDbContext CreateEmpty(string? dbName = null)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }
}

// ─── Mock Memory Cache ────────────────────────────────
public static class MockCache
{
    /// <summary>Returns a real MemoryCache (simpler than mocking the interface)</summary>
    public static IMemoryCache Real() => new MemoryCache(new MemoryCacheOptions());

    /// <summary>Returns a Moq mock of IMemoryCache that always misses (forces DB hit)</summary>
    public static IMemoryCache AlwaysMiss()
    {
        var mock = new Mock<IMemoryCache>();
        object? outVal = null;
        mock.Setup(c => c.TryGetValue(It.IsAny<object>(), out outVal)).Returns(false);
        mock.Setup(c => c.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());
        return mock.Object;
    }
}

// ─── Seed / Builder helpers ───────────────────────────
public static class TestData
{
    public static Persona MakePersona(int id = 1) => new()
    {
        Id = id,
        FirstName = "Arjun", LastName = "Sharma",
        Title = "Senior .NET Developer",
        ShortTitle = "Full-Stack .NET Expert",
        Experience = 15,
        Location = "Mumbai, India",
        Email = "arjun@dotnetpro.dev",
        Phone = "+91 98765 43210",
        Github = "https://github.com/arjunsharma",
        LinkedIn = "https://linkedin.com/in/arjunsharma",
        Twitter = "https://twitter.com/arjun",
        YouTube = "https://youtube.com/@arjun",
        Tagline = "Building Scalable Solutions",
        Bio = "15 years in .NET ecosystem",
        StatsJson = """{"projects":"120+","clients":"45+"}""",
        CertificationsJson = """["AZ-900","AZ-204"]""",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public static Skill MakeSkill(int id = 1, string name = ".NET Core", int pct = 98) => new()
    {
        Id = id, Name = name, Percentage = pct, SortOrder = id,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    public static Lead MakeLead(int id = 1, string status = "new") => new()
    {
        Id = id,
        Name = "Test Lead",
        Email = $"lead{id}@test.com",
        Role = "CTO",
        Service = "Architecture Consulting",
        Budget = "₹5L – ₹15L",
        Message = "Need help with microservices",
        Status = status,
        Value = "₹10L",
        Source = "contact-form",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public static Course MakeCourse(int id = 1) => new()
    {
        Id = id, Icon = "🔷", Title = "C# Mastery",
        Level = "beginner", Duration = "40 hrs", Students = 2000,
        ModulesJson = """["Module 1","Module 2","Module 3"]""",
        SortOrder = id, IsActive = true,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    public static Project MakeProject(int id = 1, bool featured = false) => new()
    {
        Id = id, Emoji = "🏦", Title = "Banking Platform",
        Industry = "BFSI", Problem = "Legacy monolith",
        Description = "Microservices migration",
        StackJson = """[".NET 8","Azure","Kubernetes"]""",
        GithubUrl = "https://github.com/test",
        DemoUrl = "https://demo.test",
        Highlight = "99.99% uptime",
        IsFeatured = featured, SortOrder = id,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    public static QAItem MakeQA(int id = 1, string level = "basic") => new()
    {
        Id = id, Level = level, Category = "C#",
        Question = $"Test question {id}",
        Answer = $"Test answer {id}",
        SortOrder = id, IsActive = true,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    public static BlogPost MakeBlogPost(int id = 1, bool published = true) => new()
    {
        Id = id, Emoji = "📝", Category = "Architecture",
        Title = $"Test Post {id}",
        Excerpt = "Test excerpt",
        Content = "Full content here",
        Slug = $"test-post-{id}",
        PublishedDate = DateTime.UtcNow.AddDays(-id),
        ReadTime = "5 min", ViewCount = id * 100,
        IsPublished = published,
        TagsJson = """["tag1","tag2"]""",
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    public static Testimonial MakeTestimonial(int id = 1) => new()
    {
        Id = id, Initials = "AB", Name = "Test Client",
        Company = "Test Corp", Text = "Excellent work!",
        Stars = 5, SortOrder = id, IsActive = true,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    public static ConsultingService MakeService(int id = 1) => new()
    {
        Id = id, Icon = "💻", Title = "Contract Dev",
        Price = "₹8,000", Period = "/ day",
        Description = "Full-stack development",
        IsFeatured = id == 2,
        FeaturesJson = """["Feature 1","Feature 2","Feature 3"]""",
        SortOrder = id,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    public static ContactFormDto MakeContactForm(string name = "Priya Mehta", string email = "priya@test.com") =>
        new()
        {
            Name = name, Email = email,
            Service = "Architecture Consulting",
            Budget = "₹5L – ₹15L",
            Message = "Need microservices architecture help"
        };
}
