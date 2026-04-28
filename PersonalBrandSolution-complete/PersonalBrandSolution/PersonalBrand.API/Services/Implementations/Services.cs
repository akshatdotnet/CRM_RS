using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PersonalBrand.API.Data;
using PersonalBrand.API.Models.Entities;
using PersonalBrand.API.Repositories.Interfaces;
using PersonalBrand.API.Services.Interfaces;
using PersonalBrand.Shared.Constants;
using PersonalBrand.Shared.DTOs;
using PersonalBrand.Shared.Models;
using System.Text.Json;
using ApiConsultingService = PersonalBrand.API.Models.Entities.ConsultingService;

namespace PersonalBrand.API.Services.Implementations;

// ─── Helper: JSON Deserialize ─────────────────────────
file static class JsonHelper
{
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };
    public static List<string> ToStringList(string json) =>
        JsonSerializer.Deserialize<List<string>>(json, _opts) ?? [];
    public static Dictionary<string, string> ToDict(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(json, _opts) ?? [];
}

// ─── Persona Service ──────────────────────────────────
public class PersonaService : IPersonaService
{
    private readonly IPersonaRepository _repo;
    private readonly IMemoryCache _cache;
    public PersonaService(IPersonaRepository repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<PersonaDto?> GetPersonaAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Persona, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var p = await _repo.GetFirstAsync();
            if (p == null) return null;
            return new PersonaDto
            {
                Name = $"{p.FirstName} {p.LastName}",
                FirstName = p.FirstName, LastName = p.LastName,
                Title = p.Title, ShortTitle = p.ShortTitle,
                Experience = p.Experience, Location = p.Location,
                Email = p.Email, Phone = p.Phone,
                Github = p.Github, LinkedIn = p.LinkedIn,
                Twitter = p.Twitter, YouTube = p.YouTube,
                Tagline = p.Tagline, Bio = p.Bio,
                Stats = JsonHelper.ToDict(p.StatsJson),
                Certifications = JsonHelper.ToStringList(p.CertificationsJson)
            };
        });
    }
}

// ─── Skill Service ────────────────────────────────────
public class SkillService : ISkillService
{
    private readonly IRepository<Skill> _repo;
    private readonly IMemoryCache _cache;
    public SkillService(IRepository<Skill> repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<IEnumerable<SkillDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Skills, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var skills = await _repo.GetAllAsync();
            return skills.OrderBy(s => s.SortOrder)
                .Select(s => new SkillDto { Id = s.Id, Name = s.Name, Percentage = s.Percentage });
        }) ?? [];
    }
}

// ─── Roadmap Service ──────────────────────────────────
public class RoadmapService : IRoadmapService
{
    private readonly IRepository<RoadmapItem> _repo;
    private readonly IMemoryCache _cache;
    public RoadmapService(IRepository<RoadmapItem> repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<IEnumerable<RoadmapItemDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Roadmap, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
            var items = await _repo.GetAllAsync();
            return items.OrderBy(r => r.SortOrder).Select(r => new RoadmapItemDto
            {
                Id = r.Id, Year = r.Year, Era = r.Era,
                Title = r.Title, Description = r.Description,
                Tags = JsonHelper.ToStringList(r.TagsJson),
                Icon = r.Icon, Side = r.Side
            });
        }) ?? [];
    }
}

// ─── Course Service ───────────────────────────────────
public class CourseService : ICourseService
{
    private readonly IRepository<Course> _repo;
    private readonly IMemoryCache _cache;
    public CourseService(IRepository<Course> repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<IEnumerable<CourseDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Courses, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var courses = await _repo.GetAllAsync();
            return courses.OrderBy(c => c.SortOrder).Select(MapToDto);
        }) ?? [];
    }

    public async Task<CourseDto?> GetByIdAsync(int id)
    {
        var c = await _repo.GetByIdAsync(id);
        return c == null ? null : MapToDto(c);
    }

    private static CourseDto MapToDto(Course c) => new()
    {
        Id = c.Id, Icon = c.Icon, Title = c.Title,
        Level = c.Level, Duration = c.Duration,
        Students = c.Students >= 1000 ? $"{c.Students / 100 * 100 / 1000.0:0.#}K+" : $"{c.Students}+",
        Modules = JsonHelper.ToStringList(c.ModulesJson)
    };
}

// ─── Project Service ──────────────────────────────────
public class ProjectService : IProjectService
{
    private readonly IRepository<Project> _repo;
    private readonly IMemoryCache _cache;
    public ProjectService(IRepository<Project> repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<IEnumerable<ProjectDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Projects, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var items = await _repo.GetAllAsync();
            return items.OrderBy(p => p.SortOrder).Select(MapToDto);
        }) ?? [];
    }

    public async Task<IEnumerable<ProjectDto>> GetFeaturedAsync()
    {
        var all = await GetAllAsync();
        return all.Where(p => p.IsFeatured);
    }

    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p == null ? null : MapToDto(p);
    }

    private static ProjectDto MapToDto(Project p) => new()
    {
        Id = p.Id, Emoji = p.Emoji, Title = p.Title, Industry = p.Industry,
        Problem = p.Problem, Description = p.Description,
        Stack = JsonHelper.ToStringList(p.StackJson),
        GithubUrl = p.GithubUrl, DemoUrl = p.DemoUrl,
        Highlight = p.Highlight, IsFeatured = p.IsFeatured
    };
}

// ─── Q&A Service ─────────────────────────────────────
public class QAServiceImpl : IQAService
{
    private readonly IQARepository _repo;
    private readonly IMemoryCache _cache;
    public QAServiceImpl(IQARepository repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<IEnumerable<QAItemDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.QA, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var items = await _repo.GetAllAsync();
            return items.Select(MapToDto);
        }) ?? [];
    }

    public async Task<IEnumerable<QAItemDto>> GetByLevelAsync(string level)
    {
        var items = await _repo.GetByLevelAsync(level);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<QAItemDto>> SearchAsync(string query)
    {
        var items = await _repo.SearchAsync(query);
        return items.Select(MapToDto);
    }

    private static QAItemDto MapToDto(QAItem q) => new()
    {
        Id = q.Id, Level = q.Level, Category = q.Category,
        Question = q.Question, Answer = q.Answer
    };
}

// ─── Consulting Service ───────────────────────────────
public class ConsultingServiceImpl : IConsultingService
{
    private readonly IRepository<ApiConsultingService> _repo;
    private readonly IMemoryCache _cache;
    public ConsultingServiceImpl(IRepository<ApiConsultingService> repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<IEnumerable<ConsultingServiceDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Services, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var services = await _repo.GetAllAsync();
            return services.OrderBy(s => s.SortOrder).Select(s => new ConsultingServiceDto
            {
                Id = s.Id, Icon = s.Icon, Title = s.Title, Price = s.Price,
                Period = s.Period, Description = s.Description, IsFeatured = s.IsFeatured,
                Features = JsonHelper.ToStringList(s.FeaturesJson)
            });
        }) ?? [];
    }
}

// ─── Lead Service ─────────────────────────────────────
public class LeadServiceImpl : ILeadService
{
    private readonly ILeadRepository _repo;
    public LeadServiceImpl(ILeadRepository repo) { _repo = repo; }

    public async Task<PipelineSummaryDto> GetPipelineSummaryAsync()
    {
        var leads = await _repo.GetAllWithNotesAsync();
        var countByStatus = await _repo.GetCountByStatusAsync();

        // Ensure all statuses present
        foreach (var s in LeadStatus.All)
            countByStatus.TryAdd(s, 0);

        return new PipelineSummaryDto
        {
            Leads = leads.Select(MapToDto).ToList(),
            CountByStatus = countByStatus,
            TotalLeads = leads.Count(),
            TotalPipelineValue = "₹" + leads.Where(l => l.Status != LeadStatus.Lost)
                .Sum(l => ParseValue(l.Value)) / 100000m + "L"
        };
    }

    public async Task<IEnumerable<LeadResponseDto>> GetAllAsync()
    {
        var leads = await _repo.GetAllWithNotesAsync();
        return leads.Select(MapToDto);
    }

    public async Task<LeadResponseDto?> GetByIdAsync(int id)
    {
        var lead = await _repo.GetWithNotesAsync(id);
        return lead == null ? null : MapToDto(lead);
    }

    public async Task<LeadResponseDto> CreateLeadAsync(ContactFormDto dto)
    {
        var lead = new Lead
        {
            Name = dto.Name, Email = dto.Email,
            Service = dto.Service, Budget = dto.Budget,
            Message = dto.Message, Status = LeadStatus.New,
            Value = dto.Budget, Source = "contact-form",
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        var created = await _repo.AddAsync(lead);
        await _repo.AddNoteAsync(created.Id, "Lead created via contact form");
        var result = await _repo.GetWithNotesAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task AddNoteAsync(int leadId, string note)
        => await _repo.AddNoteAsync(leadId, note);

    public async Task UpdateStatusAsync(int leadId, string newStatus)
        => await _repo.UpdateStatusAsync(leadId, newStatus);

    private static LeadResponseDto MapToDto(Lead l) => new()
    {
        Id = l.Id, Name = l.Name, Email = l.Email, Role = l.Role,
        Service = l.Service, Budget = l.Budget, Value = l.Value,
        Status = l.Status, Source = l.Source, CreatedAt = l.CreatedAt,
        Notes = l.Notes.Select(n => new LeadNoteDto
        {
            Id = n.Id, Note = n.Note, AddedBy = n.AddedBy, CreatedAt = n.CreatedAt
        }).ToList(),
        StatusHistory = l.StatusHistory.Select(h => new LeadStatusHistoryDto
        {
            FromStatus = h.FromStatus, ToStatus = h.ToStatus,
            ChangedBy = h.ChangedBy, CreatedAt = h.CreatedAt
        }).ToList()
    };

    private static decimal ParseValue(string value)
    {
        var num = new string(value.Where(c => char.IsDigit(c) || c == '.').ToArray());
        return decimal.TryParse(num, out var result) ? result * 100000 : 0;
    }
}

// ─── Blog Service ─────────────────────────────────────
public class BlogServiceImpl : IBlogService
{
    private readonly IBlogRepository _repo;
    private readonly IMemoryCache _cache;
    public BlogServiceImpl(IBlogRepository repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<IEnumerable<BlogPostDto>> GetPublishedAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Blogs, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            var posts = await _repo.GetPublishedAsync();
            return posts.Select(MapToDto);
        }) ?? [];
    }

    public async Task<BlogPostDto?> GetBySlugAsync(string slug)
    {
        var post = await _repo.GetBySlugAsync(slug);
        if (post != null) await _repo.IncrementViewCountAsync(post.Id);
        return post == null ? null : MapToDto(post);
    }

    public async Task<PagedResponse<BlogPostDto>> GetPagedAsync(int page, int pageSize)
    {
        var paged = await _repo.GetPagedAsync(page, pageSize);
        return new PagedResponse<BlogPostDto>
        {
            Items = paged.Items.Select(MapToDto).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    private static BlogPostDto MapToDto(BlogPost b) => new()
    {
        Id = b.Id, Emoji = b.Emoji, Category = b.Category, Title = b.Title,
        Excerpt = b.Excerpt, Slug = b.Slug,
        PublishedDate = b.PublishedDate.ToString("MMM dd, yyyy"),
        ReadTime = b.ReadTime,
        Views = b.ViewCount >= 1000 ? $"{b.ViewCount / 1000.0:0.#}K" : b.ViewCount.ToString(),
        Tags = JsonHelper.ToStringList(b.TagsJson)
    };
}

// ─── Testimonial Service ──────────────────────────────
public class TestimonialServiceImpl : ITestimonialService
{
    private readonly IRepository<Testimonial> _repo;
    private readonly IMemoryCache _cache;
    public TestimonialServiceImpl(IRepository<Testimonial> repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<IEnumerable<TestimonialDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Testimonials, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var items = await _repo.GetAllAsync();
            return items.OrderBy(t => t.SortOrder).Select(t => new TestimonialDto
            {
                Id = t.Id, Initials = t.Initials, Name = t.Name,
                Company = t.Company, Text = t.Text, Stars = t.Stars
            });
        }) ?? [];
    }
}

// ─── Newsletter Service ───────────────────────────────
public class NewsletterServiceImpl : INewsletterService
{
    private readonly ISubscriberRepository _repo;
    public NewsletterServiceImpl(ISubscriberRepository repo) { _repo = repo; }

    public async Task<bool> SubscribeAsync(string email)
    {
        if (await _repo.ExistsAsync(email)) return false; // already subscribed
        await _repo.AddAsync(email);
        return true;
    }
}
