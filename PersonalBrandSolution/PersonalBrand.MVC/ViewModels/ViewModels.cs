using PersonalBrand.Shared.DTOs;
using PersonalBrand.Shared.Models;

namespace PersonalBrand.MVC.ViewModels;

// ─── Home / Index ─────────────────────────────────────
public class HomeViewModel
{
    public PersonaDto Persona { get; set; } = new();
    public List<SkillDto> Skills { get; set; } = [];
    public List<RoadmapItemDto> Roadmap { get; set; } = [];
    public List<CourseDto> Courses { get; set; } = [];
    public List<ProjectDto> Projects { get; set; } = [];
    public List<QAItemDto> QAItems { get; set; } = [];
    public List<ConsultingServiceDto> Services { get; set; } = [];
    public PipelineSummaryDto Pipeline { get; set; } = new();
    public List<BlogPostDto> BlogPosts { get; set; } = [];
    public List<TestimonialDto> Testimonials { get; set; } = [];
}

// ─── Contact Form ─────────────────────────────────────
public class ContactFormViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Budget { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

// ─── Pipeline / CRM ──────────────────────────────────
public class PipelineViewModel
{
    public PipelineSummaryDto Summary { get; set; } = new();
    public string[] StatusOrder { get; set; } = ["new", "contacted", "proposal", "negotiation", "closed"];
    public Dictionary<string, string> StatusLabels { get; set; } = new()
    {
        ["new"] = "Lead", ["contacted"] = "Contacted",
        ["proposal"] = "Proposal", ["negotiation"] = "Negotiation", ["closed"] = "Closed ✓"
    };
    public Dictionary<string, string> StatusColors { get; set; } = new()
    {
        ["new"] = "#888", ["contacted"] = "#0066ff",
        ["proposal"] = "#ffaa00", ["negotiation"] = "#aa44ff", ["closed"] = "#00ff88"
    };
}

// ─── Q&A ─────────────────────────────────────────────
public class QAViewModel
{
    public List<QAItemDto> Items { get; set; } = [];
    public string? CurrentLevel { get; set; }
    public string? SearchTerm { get; set; }
    public string[] Levels { get; set; } = ["all", "basic", "intermediate", "advanced", "azure", "architecture"];
}

// ─── Toast/Notification for TempData ─────────────────
public class ToastViewModel
{
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "success"; // success | error | info
}
