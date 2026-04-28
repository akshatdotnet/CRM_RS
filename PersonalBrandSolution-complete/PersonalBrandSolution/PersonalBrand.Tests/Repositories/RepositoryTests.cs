using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.API.Models.Entities;
using PersonalBrand.API.Repositories.Implementations;
using PersonalBrand.Shared.Constants;
using Xunit;

namespace PersonalBrand.Tests.Repositories;

// ══════════════════════════════════════════════════════
//  GENERIC REPOSITORY TESTS
// ══════════════════════════════════════════════════════
public class GenericRepositoryTests
{
    [Fact]
    public async Task AddAsync_AddsEntityAndReturnsWithId()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        var repo = new Repository<Skill>(ctx);
        var skill = TestData.MakeSkill(0); // id=0 → EF assigns

        // Act
        var result = await repo.AddAsync(skill);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        ctx.Skills.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntity_WhenExists()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.Skills.Add(TestData.MakeSkill(7));
        await ctx.SaveChangesAsync();
        var repo = new Repository<Skill>(ctx);

        // Act
        var result = await repo.GetByIdAsync(7);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(7);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        var repo = new Repository<Skill>(ctx);

        // Act
        var result = await repo.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletes_NotHardDeletes()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.Skills.Add(TestData.MakeSkill(5));
        await ctx.SaveChangesAsync();
        var repo = new Repository<Skill>(ctx);

        // Act
        await repo.DeleteAsync(5);

        // Assert — global filter hides it from normal queries
        var fromRepo = await repo.GetByIdAsync(5);
        fromRepo.Should().BeNull(); // hidden by IsDeleted filter

        // But still exists in DB (soft delete)
        var raw = ctx.Skills.IgnoreQueryFilters().FirstOrDefault(s => s.Id == 5);
        raw.Should().NotBeNull();
        raw!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.Skills.Add(TestData.MakeSkill(3, ".NET Core", 90));
        await ctx.SaveChangesAsync();
        var repo = new Repository<Skill>(ctx);

        // Act
        var skill = await repo.GetByIdAsync(3);
        skill!.Percentage = 98;
        await repo.UpdateAsync(skill);

        // Assert
        var updated = await repo.GetByIdAsync(3);
        updated!.Percentage.Should().Be(98);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenEntityExists()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.Skills.Add(TestData.MakeSkill(10));
        await ctx.SaveChangesAsync();
        var repo = new Repository<Skill>(ctx);

        // Act
        var exists = await repo.ExistsAsync(10);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        var repo = new Repository<Skill>(ctx);

        // Act
        var exists = await repo.ExistsAsync(999);

        // Assert
        exists.Should().BeFalse();
    }
}

// ══════════════════════════════════════════════════════
//  LEAD REPOSITORY TESTS
// ══════════════════════════════════════════════════════
public class LeadRepositoryTests
{
    [Fact]
    public async Task GetByStatusAsync_ReturnsOnlyMatchingStatus()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.Leads.AddRange(
            TestData.MakeLead(1, "new"),
            TestData.MakeLead(2, "new"),
            TestData.MakeLead(3, "contacted"),
            TestData.MakeLead(4, "closed")
        );
        await ctx.SaveChangesAsync();
        var repo = new LeadRepository(ctx);

        // Act
        var result = (await repo.GetByStatusAsync("new")).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(l => l.Status == "new").Should().BeTrue();
    }

    [Fact]
    public async Task AddNoteAsync_AddsNoteToLead()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.Leads.Add(TestData.MakeLead(1));
        await ctx.SaveChangesAsync();
        var repo = new LeadRepository(ctx);

        // Act
        await repo.AddNoteAsync(1, "Called client", "Admin");
        var lead = await repo.GetWithNotesAsync(1);

        // Assert
        lead!.Notes.Should().HaveCount(1);
        lead.Notes.First().Note.Should().Be("Called client");
        lead.Notes.First().AddedBy.Should().Be("Admin");
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesStatusAndAddsHistory()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.Leads.Add(TestData.MakeLead(1, "new"));
        await ctx.SaveChangesAsync();
        var repo = new LeadRepository(ctx);

        // Act
        await repo.UpdateStatusAsync(1, "contacted", "Owner");
        var lead = await repo.GetWithNotesAsync(1);

        // Assert
        lead!.Status.Should().Be("contacted");
        lead.StatusHistory.Should().HaveCount(1);
        lead.StatusHistory.First().FromStatus.Should().Be("new");
        lead.StatusHistory.First().ToStatus.Should().Be("contacted");
    }

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsCorrectCounts()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.Leads.AddRange(
            TestData.MakeLead(1, "new"),
            TestData.MakeLead(2, "new"),
            TestData.MakeLead(3, "new"),
            TestData.MakeLead(4, "contacted"),
            TestData.MakeLead(5, "closed")
        );
        await ctx.SaveChangesAsync();
        var repo = new LeadRepository(ctx);

        // Act
        var counts = await repo.GetCountByStatusAsync();

        // Assert
        counts["new"].Should().Be(3);
        counts["contacted"].Should().Be(1);
        counts["closed"].Should().Be(1);
    }

    [Fact]
    public async Task GetAllWithNotesAsync_IncludesNotesAndHistory()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        var lead = TestData.MakeLead(1);
        ctx.Leads.Add(lead);
        await ctx.SaveChangesAsync();
        var repo = new LeadRepository(ctx);
        await repo.AddNoteAsync(1, "Test note");
        await repo.UpdateStatusAsync(1, "contacted");

        // Act
        var results = (await repo.GetAllWithNotesAsync()).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Notes.Should().HaveCount(1);
        results[0].StatusHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsLead_WhenEmailMatches()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        var lead = TestData.MakeLead(1);
        lead.Email = "test@company.com";
        ctx.Leads.Add(lead);
        await ctx.SaveChangesAsync();
        var repo = new LeadRepository(ctx);

        // Act
        var result = await repo.GetByEmailAsync("TEST@COMPANY.COM"); // case insensitive

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@company.com");
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.Leads.AddRange(
            TestData.MakeLead(1), TestData.MakeLead(2), TestData.MakeLead(3)
        );
        await ctx.SaveChangesAsync();
        var repo = new LeadRepository(ctx);

        // Act
        var count = await repo.GetTotalCountAsync();

        // Assert
        count.Should().Be(3);
    }
}

// ══════════════════════════════════════════════════════
//  BLOG REPOSITORY TESTS
// ══════════════════════════════════════════════════════
public class BlogRepositoryTests
{
    [Fact]
    public async Task GetBySlugAsync_ReturnsPost()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.BlogPosts.Add(TestData.MakeBlogPost(1, published: true));
        await ctx.SaveChangesAsync();
        var repo = new BlogRepository(ctx);

        // Act
        var result = await repo.GetBySlugAsync("test-post-1");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("test-post-1");
    }

    [Fact]
    public async Task GetPublishedAsync_ExcludesUnpublished()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.BlogPosts.AddRange(
            TestData.MakeBlogPost(1, published: true),
            TestData.MakeBlogPost(2, published: false)
        );
        await ctx.SaveChangesAsync();
        var repo = new BlogRepository(ctx);

        // Act
        var result = await repo.GetPublishedAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task IncrementViewCountAsync_IncrementsCount()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        var post = TestData.MakeBlogPost(1);
        post.ViewCount = 50;
        ctx.BlogPosts.Add(post);
        await ctx.SaveChangesAsync();
        var repo = new BlogRepository(ctx);

        // Act
        await repo.IncrementViewCountAsync(1);
        await repo.IncrementViewCountAsync(1);

        // Assert
        var updated = await ctx.BlogPosts.FindAsync(1);
        updated!.ViewCount.Should().Be(52);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPage()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        for (int i = 1; i <= 8; i++)
            ctx.BlogPosts.Add(TestData.MakeBlogPost(i, published: true));
        await ctx.SaveChangesAsync();
        var repo = new BlogRepository(ctx);

        // Act
        var result = await repo.GetPagedAsync(2, 3);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(8);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);
    }
}

// ══════════════════════════════════════════════════════
//  QA REPOSITORY TESTS
// ══════════════════════════════════════════════════════
public class QARepositoryTests
{
    [Fact]
    public async Task GetByLevelAsync_FiltersCorrectly()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        ctx.QAItems.AddRange(
            TestData.MakeQA(1, "basic"),
            TestData.MakeQA(2, "advanced"),
            TestData.MakeQA(3, "basic")
        );
        await ctx.SaveChangesAsync();
        var repo = new QARepository(ctx);

        // Act
        var result = (await repo.GetByLevelAsync("basic")).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(q => q.Level == "basic").Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_MatchesInAnswer()
    {
        // Arrange
        using var ctx  = TestDbFactory.CreateEmpty();
        var qa = TestData.MakeQA(1, "basic");
        qa.Question = "What is EF Core?";
        qa.Answer   = "Entity Framework Core is an ORM for .NET";
        ctx.QAItems.Add(qa);
        await ctx.SaveChangesAsync();
        var repo = new QARepository(ctx);

        // Act
        var result = (await repo.SearchAsync("ORM")).ToList();

        // Assert
        result.Should().HaveCount(1);
    }
}
