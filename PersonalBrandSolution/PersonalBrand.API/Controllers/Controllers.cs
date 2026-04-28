using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PersonalBrand.API.Services.Interfaces;
using PersonalBrand.Shared.Models;

namespace PersonalBrand.API.Controllers;

// ─── Base API Controller ──────────────────────────────
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase { }

// ─── Persona ──────────────────────────────────────────
[ApiVersion("1.0")]
public class PersonaController : BaseApiController
{
    private readonly IPersonaService _svc;
    public PersonaController(IPersonaService svc) { _svc = svc; }

    /// <summary>Get persona/profile information</summary>
    [HttpGet]
    [ProducesResponseType(200)] [ProducesResponseType(404)]
    public async Task<IActionResult> Get()
    {
        var data = await _svc.GetPersonaAsync();
        return data == null ? NotFound(ApiResponse<string>.Fail("Persona not found", 404))
                            : Ok(ApiResponse<object>.Ok(data));
    }
}

// ─── Skills ───────────────────────────────────────────
[ApiVersion("1.0")]
public class SkillsController : BaseApiController
{
    private readonly ISkillService _svc;
    public SkillsController(ISkillService svc) { _svc = svc; }

    /// <summary>Get all skills with proficiency percentages</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _svc.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }
}

// ─── Roadmap ──────────────────────────────────────────
[ApiVersion("1.0")]
public class RoadmapController : BaseApiController
{
    private readonly IRoadmapService _svc;
    public RoadmapController(IRoadmapService svc) { _svc = svc; }

    /// <summary>Get career roadmap timeline</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _svc.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }
}

// ─── Courses ──────────────────────────────────────────
[ApiVersion("1.0")]
public class CoursesController : BaseApiController
{
    private readonly ICourseService _svc;
    public CoursesController(ICourseService svc) { _svc = svc; }

    /// <summary>Get all courses</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _svc.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }

    /// <summary>Get course by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _svc.GetByIdAsync(id);
        return data == null ? NotFound(ApiResponse<string>.Fail($"Course {id} not found", 404))
                            : Ok(ApiResponse<object>.Ok(data));
    }
}

// ─── Projects ─────────────────────────────────────────
[ApiVersion("1.0")]
public class ProjectsController : BaseApiController
{
    private readonly IProjectService _svc;
    public ProjectsController(IProjectService svc) { _svc = svc; }

    /// <summary>Get all projects</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? featured = null)
    {
        var data = featured == true ? await _svc.GetFeaturedAsync() : await _svc.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }

    /// <summary>Get project by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _svc.GetByIdAsync(id);
        return data == null ? NotFound(ApiResponse<string>.Fail($"Project {id} not found", 404))
                            : Ok(ApiResponse<object>.Ok(data));
    }
}

// ─── Q&A ──────────────────────────────────────────────
[ApiVersion("1.0")]
public class QAController : BaseApiController
{
    private readonly IQAService _svc;
    public QAController(IQAService svc) { _svc = svc; }

    /// <summary>Get Q&A items, optionally filtered by level</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? level = null, [FromQuery] string? search = null)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var results = await _svc.SearchAsync(search);
            return Ok(ApiResponse<object>.Ok(results));
        }
        var data = !string.IsNullOrWhiteSpace(level)
            ? await _svc.GetByLevelAsync(level)
            : await _svc.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }
}

// ─── Consulting Services ──────────────────────────────
[ApiVersion("1.0")]
public class ServicesController : BaseApiController
{
    private readonly IConsultingService _svc;
    public ServicesController(IConsultingService svc) { _svc = svc; }

    /// <summary>Get consulting services and pricing</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _svc.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }
}

// ─── Leads / CRM ──────────────────────────────────────
[ApiVersion("1.0")]
public class LeadsController : BaseApiController
{
    private readonly ILeadService _svc;
    public LeadsController(ILeadService svc) { _svc = svc; }

    /// <summary>Get full pipeline summary (all leads grouped by status)</summary>
    [HttpGet("pipeline")]
    public async Task<IActionResult> GetPipeline()
    {
        var data = await _svc.GetPipelineSummaryAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }

    /// <summary>Get all leads</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _svc.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }

    /// <summary>Get lead by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _svc.GetByIdAsync(id);
        return data == null ? NotFound(ApiResponse<string>.Fail($"Lead {id} not found", 404))
                            : Ok(ApiResponse<object>.Ok(data));
    }

    /// <summary>Create a new lead from contact form submission</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContactFormDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(ApiResponse<string>.Fail("Name and Email are required"));

        if (!IsValidEmail(dto.Email))
            return BadRequest(ApiResponse<string>.Fail("Invalid email format"));

        var lead = await _svc.CreateLeadAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, ApiResponse<object>.Ok(lead, "Lead created successfully"));
    }

    /// <summary>Add a note to a lead</summary>
    [HttpPost("{id:int}/notes")]
    public async Task<IActionResult> AddNote(int id, [FromBody] AddNoteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Note))
            return BadRequest(ApiResponse<string>.Fail("Note cannot be empty"));

        await _svc.AddNoteAsync(id, dto.Note);
        return Ok(ApiResponse<string>.Ok("Note added"));
    }

    /// <summary>Update lead status</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateLeadStatusDto dto)
    {
        var validStatuses = new[] { "new", "contacted", "proposal", "negotiation", "closed", "lost" };
        if (!validStatuses.Contains(dto.Status))
            return BadRequest(ApiResponse<string>.Fail($"Invalid status. Valid: {string.Join(", ", validStatuses)}"));

        await _svc.UpdateStatusAsync(id, dto.Status);
        return Ok(ApiResponse<string>.Ok($"Status updated to '{dto.Status}'"));
    }

    private static bool IsValidEmail(string email) =>
        System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}

// ─── Blog ─────────────────────────────────────────────
[ApiVersion("1.0")]
public class BlogController : BaseApiController
{
    private readonly IBlogService _svc;
    public BlogController(IBlogService svc) { _svc = svc; }

    /// <summary>Get all published blog posts</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 50)
            return BadRequest(ApiResponse<string>.Fail("Invalid pagination parameters"));

        var data = await _svc.GetPagedAsync(page, pageSize);
        return Ok(ApiResponse<object>.Ok(data));
    }

    /// <summary>Get blog post by slug</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var data = await _svc.GetBySlugAsync(slug);
        return data == null ? NotFound(ApiResponse<string>.Fail($"Post '{slug}' not found", 404))
                            : Ok(ApiResponse<object>.Ok(data));
    }
}

// ─── Testimonials ─────────────────────────────────────
[ApiVersion("1.0")]
public class TestimonialsController : BaseApiController
{
    private readonly ITestimonialService _svc;
    public TestimonialsController(ITestimonialService svc) { _svc = svc; }

    /// <summary>Get all testimonials</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _svc.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }
}

// ─── Newsletter ───────────────────────────────────────
[ApiVersion("1.0")]
public class NewsletterController : BaseApiController
{
    private readonly INewsletterService _svc;
    public NewsletterController(INewsletterService svc) { _svc = svc; }

    /// <summary>Subscribe to newsletter</summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] NewsletterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(ApiResponse<string>.Fail("Email is required"));

        var isNew = await _svc.SubscribeAsync(dto.Email);
        var message = isNew ? "Successfully subscribed!" : "Already subscribed";
        return Ok(ApiResponse<string>.Ok(message));
    }
}

// ─── Health Check ─────────────────────────────────────
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        version = "1.0",
        service = "PersonalBrand.API"
    });
}
