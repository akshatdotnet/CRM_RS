using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PersonalBrand.API.Data;
using PersonalBrand.API.Models.Entities;
using PersonalBrand.API.Repositories.Implementations;
using PersonalBrand.API.Repositories.Interfaces;
using PersonalBrand.API.Services.Implementations;
using PersonalBrand.Shared.Constants;
using PersonalBrand.Shared.DTOs;
using PersonalBrand.Shared.Models;
using Xunit;

namespace PersonalBrand.Tests.Services;

// ══════════════════════════════════════════════════════
//  PERSONA SERVICE TESTS
// ══════════════════════════════════════════════════════
public class PersonaServiceTests
{
    [Fact]
    public async Task GetPersonaAsync_ReturnsPersona_WhenExists()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Personas.Add(TestData.MakePersona());
        await ctx.SaveChangesAsync();
        var repo = new PersonaRepository(ctx);
        var svc  = new PersonaService(repo, MockCache.Real());

        // Act
        var result = await svc.GetPersonaAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Arjun Sharma");
        result.Experience.Should().Be(15);
        result.Email.Should().Be("arjun@dotnetpro.dev");
        result.Certifications.Should().Contain("AZ-900").And.Contain("AZ-204");
        result.Stats.Should().ContainKey("projects");
    }

    [Fact]
    public async Task GetPersonaAsync_ReturnsNull_WhenNoPersonaExists()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var repo = new PersonaRepository(ctx);
        var svc  = new PersonaService(repo, MockCache.Real());

        // Act
        var result = await svc.GetPersonaAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPersonaAsync_ReturnsCachedResult_OnSecondCall()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Personas.Add(TestData.MakePersona());
        await ctx.SaveChangesAsync();
        var repo  = new PersonaRepository(ctx);
        var cache = MockCache.Real();
        var svc   = new PersonaService(repo, cache);

        // Act — two calls
        var first  = await svc.GetPersonaAsync();
        var second = await svc.GetPersonaAsync();

        // Assert — both return same result (second is from cache)
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        first!.Email.Should().Be(second!.Email);
    }

    [Fact]
    public async Task GetPersonaAsync_MapsStats_Correctly()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var persona = TestData.MakePersona();
        persona.StatsJson = """{"projects":"120+","clients":"45+","courses":"12+","students":"5000+"}""";
        ctx.Personas.Add(persona);
        await ctx.SaveChangesAsync();
        var repo = new PersonaRepository(ctx);
        var svc  = new PersonaService(repo, MockCache.Real());

        // Act
        var result = await svc.GetPersonaAsync();

        // Assert
        result!.Stats.Should().HaveCount(4);
        result.Stats["projects"].Should().Be("120+");
        result.Stats["students"].Should().Be("5000+");
    }
}

// ══════════════════════════════════════════════════════
//  SKILL SERVICE TESTS
// ══════════════════════════════════════════════════════
public class SkillServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllSkills_OrderedBySortOrder()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Skills.AddRange(
            TestData.MakeSkill(1, ".NET Core", 98),
            TestData.MakeSkill(2, "Azure", 92),
            TestData.MakeSkill(3, "Docker", 85)
        );
        await ctx.SaveChangesAsync();
        var repo = new Repository<Skill>(ctx);
        var svc  = new SkillService(repo, MockCache.Real());

        // Act
        var result = (await svc.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be(".NET Core");
        result[0].Percentage.Should().Be(98);
        result[2].Name.Should().Be("Docker");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoSkills()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var repo = new Repository<Skill>(ctx);
        var svc  = new SkillService(repo, MockCache.Real());

        // Act
        var result = await svc.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(0)]
    [InlineData(75)]
    public async Task GetAllAsync_ReturnsCorrectPercentage(int pct)
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Skills.Add(TestData.MakeSkill(1, "Test Skill", pct));
        await ctx.SaveChangesAsync();
        var repo = new Repository<Skill>(ctx);
        var svc  = new SkillService(repo, MockCache.Real());

        // Act
        var result = (await svc.GetAllAsync()).First();

        // Assert
        result.Percentage.Should().Be(pct);
    }
}

// ══════════════════════════════════════════════════════
//  COURSE SERVICE TESTS
// ══════════════════════════════════════════════════════
public class CourseServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsCoursesWithParsedModules()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Courses.Add(TestData.MakeCourse(1));
        await ctx.SaveChangesAsync();
        var repo = new Repository<Course>(ctx);
        var svc  = new CourseService(repo, MockCache.Real());

        // Act
        var result = (await svc.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Modules.Should().HaveCount(3);
        result[0].Modules.Should().Contain("Module 1");
    }

    [Fact]
    public async Task GetAllAsync_FormatsStudentsCorrectly_Over1000()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var course = TestData.MakeCourse(1);
        course.Students = 2100;
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();
        var repo = new Repository<Course>(ctx);
        var svc  = new CourseService(repo, MockCache.Real());

        // Act
        var result = (await svc.GetAllAsync()).First();

        // Assert
        result.Students.Should().Contain("K");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var repo = new Repository<Course>(ctx);
        var svc  = new CourseService(repo, MockCache.Real());

        // Act
        var result = await svc.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCourse_WhenFound()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Courses.Add(TestData.MakeCourse(42));
        await ctx.SaveChangesAsync();
        var repo = new Repository<Course>(ctx);
        var svc  = new CourseService(repo, MockCache.Real());

        // Act
        var result = await svc.GetByIdAsync(42);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
        result.Title.Should().Be("C# Mastery");
    }
}

// ══════════════════════════════════════════════════════
//  PROJECT SERVICE TESTS
// ══════════════════════════════════════════════════════
public class ProjectServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllProjects()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Projects.AddRange(
            TestData.MakeProject(1, featured: true),
            TestData.MakeProject(2, featured: false),
            TestData.MakeProject(3, featured: true)
        );
        await ctx.SaveChangesAsync();
        var repo = new Repository<Project>(ctx);
        var svc  = new ProjectService(repo, MockCache.Real());

        // Act
        var result = (await svc.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetFeaturedAsync_ReturnsOnlyFeatured()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Projects.AddRange(
            TestData.MakeProject(1, featured: true),
            TestData.MakeProject(2, featured: false),
            TestData.MakeProject(3, featured: true)
        );
        await ctx.SaveChangesAsync();
        var repo = new Repository<Project>(ctx);
        var svc  = new ProjectService(repo, MockCache.Real());

        // Act
        var result = (await svc.GetFeaturedAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.IsFeatured).Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ParsesStackJson_Correctly()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Projects.Add(TestData.MakeProject(1));
        await ctx.SaveChangesAsync();
        var repo = new Repository<Project>(ctx);
        var svc  = new ProjectService(repo, MockCache.Real());

        // Act
        var result = (await svc.GetAllAsync()).First();

        // Assert
        result.Stack.Should().Contain(".NET 8")
              .And.Contain("Azure")
              .And.Contain("Kubernetes");
    }
}

// ══════════════════════════════════════════════════════
//  Q&A SERVICE TESTS
// ══════════════════════════════════════════════════════
public class QAServiceTests
{
    private static (QARepository repo, QAServiceImpl svc) Setup(AppDbContext ctx)
    {
        var repo = new QARepository(ctx);
        var svc  = new QAServiceImpl(repo, MockCache.Real());
        return (repo, svc);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllActiveItems()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.QAItems.AddRange(
            TestData.MakeQA(1, "basic"),
            TestData.MakeQA(2, "intermediate"),
            TestData.MakeQA(3, "advanced")
        );
        await ctx.SaveChangesAsync();
        var (_, svc) = Setup(ctx);

        // Act
        var result = (await svc.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("basic",        1)]
    [InlineData("intermediate", 1)]
    [InlineData("advanced",     1)]
    public async Task GetByLevelAsync_ReturnsCorrectItems(string level, int expectedCount)
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.QAItems.AddRange(
            TestData.MakeQA(1, "basic"),
            TestData.MakeQA(2, "intermediate"),
            TestData.MakeQA(3, "advanced")
        );
        await ctx.SaveChangesAsync();
        var (_, svc) = Setup(ctx);

        // Act
        var result = await svc.GetByLevelAsync(level);

        // Assert
        result.Should().HaveCount(expectedCount);
        result.All(q => q.Level == level).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_FindsMatchingQuestion()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var qa = TestData.MakeQA(1, "basic");
        qa.Question = "What is dependency injection?";
        qa.Answer   = "DI is a pattern for loose coupling.";
        ctx.QAItems.Add(qa);
        await ctx.SaveChangesAsync();
        var (_, svc) = Setup(ctx);

        // Act
        var result = (await svc.SearchAsync("dependency")).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Question.Should().Contain("dependency injection");
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenNoMatch()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.QAItems.Add(TestData.MakeQA(1, "basic"));
        await ctx.SaveChangesAsync();
        var (_, svc) = Setup(ctx);

        // Act
        var result = await svc.SearchAsync("kubernetes-operator-xyz-notexist");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_IsCaseInsensitive()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var qa = TestData.MakeQA(1);
        qa.Question = "What is CQRS pattern?";
        ctx.QAItems.Add(qa);
        await ctx.SaveChangesAsync();
        var (_, svc) = Setup(ctx);

        // Act — search lowercase
        var result = (await svc.SearchAsync("cqrs")).ToList();

        // Assert
        result.Should().HaveCount(1);
    }
}

// ══════════════════════════════════════════════════════
//  LEAD SERVICE TESTS
// ══════════════════════════════════════════════════════
public class LeadServiceTests
{
    private static LeadServiceImpl MakeService(AppDbContext ctx)
        => new(new LeadRepository(ctx));

    [Fact]
    public async Task CreateLeadAsync_CreatesLeadWithNewStatus()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var svc = MakeService(ctx);
        var dto = TestData.MakeContactForm();

        // Act
        var result = await svc.CreateLeadAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Priya Mehta");
        result.Email.Should().Be("priya@test.com");
        result.Status.Should().Be(LeadStatus.New);
        result.Source.Should().Be("contact-form");
    }

    [Fact]
    public async Task CreateLeadAsync_AddsInitialNote()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var svc = MakeService(ctx);

        // Act
        var result = await svc.CreateLeadAsync(TestData.MakeContactForm());

        // Assert
        result.Notes.Should().HaveCount(1);
        result.Notes[0].Note.Should().Contain("contact form");
    }

    [Fact]
    public async Task AddNoteAsync_AddsNoteToLead()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var lead = TestData.MakeLead(1, "new");
        ctx.Leads.Add(lead);
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        await svc.AddNoteAsync(1, "Followed up by email");
        var result = await svc.GetByIdAsync(1);

        // Assert
        result!.Notes.Should().HaveCount(1);
        result.Notes[0].Note.Should().Be("Followed up by email");
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesLeadStatus()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Leads.Add(TestData.MakeLead(1, "new"));
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        await svc.UpdateStatusAsync(1, LeadStatus.Contacted);
        var result = await svc.GetByIdAsync(1);

        // Assert
        result!.Status.Should().Be(LeadStatus.Contacted);
    }

    [Fact]
    public async Task UpdateStatusAsync_RecordsStatusHistory()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Leads.Add(TestData.MakeLead(1, "new"));
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        await svc.UpdateStatusAsync(1, LeadStatus.Contacted);
        var result = await svc.GetByIdAsync(1);

        // Assert
        result!.StatusHistory.Should().HaveCount(1);
        result.StatusHistory[0].FromStatus.Should().Be("new");
        result.StatusHistory[0].ToStatus.Should().Be("contacted");
    }

    [Fact]
    public async Task GetPipelineSummaryAsync_GroupsLeadsByStatus()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Leads.AddRange(
            TestData.MakeLead(1, "new"),
            TestData.MakeLead(2, "new"),
            TestData.MakeLead(3, "contacted"),
            TestData.MakeLead(4, "closed")
        );
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        var result = await svc.GetPipelineSummaryAsync();

        // Assert
        result.TotalLeads.Should().Be(4);
        result.CountByStatus["new"].Should().Be(2);
        result.CountByStatus["contacted"].Should().Be(1);
        result.CountByStatus["closed"].Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenLeadNotFound()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var svc = MakeService(ctx);

        // Act
        var result = await svc.GetByIdAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllLeads_OrderedByCreatedAt()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var lead1 = TestData.MakeLead(1); lead1.CreatedAt = DateTime.UtcNow.AddDays(-2);
        var lead2 = TestData.MakeLead(2); lead2.CreatedAt = DateTime.UtcNow.AddDays(-1);
        ctx.Leads.AddRange(lead1, lead2);
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        var result = (await svc.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        // Most recent first
        result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt);
    }
}

// ══════════════════════════════════════════════════════
//  BLOG SERVICE TESTS
// ══════════════════════════════════════════════════════
public class BlogServiceTests
{
    private static BlogServiceImpl MakeService(AppDbContext ctx)
        => new(new BlogRepository(ctx), MockCache.Real());

    [Fact]
    public async Task GetPublishedAsync_ReturnsOnlyPublishedPosts()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.BlogPosts.AddRange(
            TestData.MakeBlogPost(1, published: true),
            TestData.MakeBlogPost(2, published: false),
            TestData.MakeBlogPost(3, published: true)
        );
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        var result = (await svc.GetPublishedAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(p => !string.IsNullOrEmpty(p.Title)).Should().BeTrue();
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsPost_WhenFound()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.BlogPosts.Add(TestData.MakeBlogPost(1, published: true));
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        var result = await svc.GetBySlugAsync("test-post-1");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("test-post-1");
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsNull_ForUnpublishedPost()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.BlogPosts.Add(TestData.MakeBlogPost(1, published: false));
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        var result = await svc.GetBySlugAsync("test-post-1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_IncrementsViewCount()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var post = TestData.MakeBlogPost(1, published: true);
        post.ViewCount = 100;
        ctx.BlogPosts.Add(post);
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        await svc.GetBySlugAsync("test-post-1");
        var updated = await ctx.BlogPosts.FindAsync(1);

        // Assert
        updated!.ViewCount.Should().Be(101);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPaginatedResults()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        for (int i = 1; i <= 10; i++)
            ctx.BlogPosts.Add(TestData.MakeBlogPost(i, published: true));
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        var result = await svc.GetPagedAsync(page: 1, pageSize: 4);

        // Assert
        result.Items.Should().HaveCount(4);
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_Page2_ReturnsCorrectItems()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        for (int i = 1; i <= 6; i++)
            ctx.BlogPosts.Add(TestData.MakeBlogPost(i, published: true));
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        var result = await svc.GetPagedAsync(page: 2, pageSize: 4);

        // Assert
        result.Items.Should().HaveCount(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPublishedAsync_FormatsViewCount_Over1000()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var post = TestData.MakeBlogPost(1, published: true);
        post.ViewCount = 5100;
        ctx.BlogPosts.Add(post);
        await ctx.SaveChangesAsync();
        var svc = MakeService(ctx);

        // Act
        var result = (await svc.GetPublishedAsync()).First();

        // Assert
        result.Views.Should().Contain("K");
    }
}

// ══════════════════════════════════════════════════════
//  TESTIMONIAL SERVICE TESTS
// ══════════════════════════════════════════════════════
public class TestimonialServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllTestimonials_OrderedBySortOrder()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        ctx.Testimonials.AddRange(
            TestData.MakeTestimonial(1),
            TestData.MakeTestimonial(2),
            TestData.MakeTestimonial(3)
        );
        await ctx.SaveChangesAsync();
        var repo = new Repository<Testimonial>(ctx);
        var svc  = new TestimonialServiceImpl(repo, MockCache.Real());

        // Act
        var result = (await svc.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Stars.Should().Be(5);
        result.All(t => !string.IsNullOrEmpty(t.Name)).Should().BeTrue();
    }
}

// ══════════════════════════════════════════════════════
//  NEWSLETTER SERVICE TESTS
// ══════════════════════════════════════════════════════
public class NewsletterServiceTests
{
    [Fact]
    public async Task SubscribeAsync_ReturnTrue_ForNewEmail()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var repo = new SubscriberRepository(ctx);
        var svc  = new NewsletterServiceImpl(repo);

        // Act
        var result = await svc.SubscribeAsync("new@test.com");

        // Assert
        result.Should().BeTrue();
        ctx.Subscribers.Should().HaveCount(1);
    }

    [Fact]
    public async Task SubscribeAsync_ReturnsFalse_ForDuplicateEmail()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var repo = new SubscriberRepository(ctx);
        var svc  = new NewsletterServiceImpl(repo);
        await svc.SubscribeAsync("dup@test.com");

        // Act — subscribe again with same email
        var result = await svc.SubscribeAsync("dup@test.com");

        // Assert
        result.Should().BeFalse();
        ctx.Subscribers.Should().HaveCount(1); // not duplicated
    }

    [Fact]
    public async Task SubscribeAsync_IsCaseInsensitive()
    {
        // Arrange
        using var ctx = TestDbFactory.CreateEmpty();
        var repo = new SubscriberRepository(ctx);
        var svc  = new NewsletterServiceImpl(repo);
        await svc.SubscribeAsync("Test@EXAMPLE.com");

        // Act
        var result = await svc.SubscribeAsync("test@example.com");

        // Assert
        result.Should().BeFalse(); // treated as duplicate
    }
}
