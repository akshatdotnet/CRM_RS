namespace PersonalBrand.Shared.DTOs;

// ─── Persona ─────────────────────────────────────────
public class PersonaDto
{
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortTitle { get; set; } = string.Empty;
    public int Experience { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Github { get; set; } = string.Empty;
    public string LinkedIn { get; set; } = string.Empty;
    public string Twitter { get; set; } = string.Empty;
    public string YouTube { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public Dictionary<string, string> Stats { get; set; } = [];
    public List<string> Certifications { get; set; } = [];
}

// ─── Skill ───────────────────────────────────────────
public class SkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Percentage { get; set; }
}

// ─── Roadmap ─────────────────────────────────────────
public class RoadmapItemDto
{
    public int Id { get; set; }
    public string Year { get; set; } = string.Empty;
    public string Era { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public string Icon { get; set; } = string.Empty;
    public string Side { get; set; } = "left";
}

// ─── Course ──────────────────────────────────────────
public class CourseDto
{
    public int Id { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Students { get; set; } = string.Empty;
    public List<string> Modules { get; set; } = [];
}

// ─── Project ─────────────────────────────────────────
public class ProjectDto
{
    public int Id { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Stack { get; set; } = [];
    public string GithubUrl { get; set; } = string.Empty;
    public string DemoUrl { get; set; } = string.Empty;
    public string Highlight { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
}

// ─── Q&A ─────────────────────────────────────────────
public class QAItemDto
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

// ─── Consulting Service ───────────────────────────────
public class ConsultingServiceDto
{
    public int Id { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public List<string> Features { get; set; } = [];
}

// ─── Lead / CRM ──────────────────────────────────────
public class LeadResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Budget { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<LeadNoteDto> Notes { get; set; } = [];
    public List<LeadStatusHistoryDto> StatusHistory { get; set; } = [];
}

public class LeadNoteDto
{
    public int Id { get; set; }
    public string Note { get; set; } = string.Empty;
    public string AddedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LeadStatusHistoryDto
{
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PipelineSummaryDto
{
    public Dictionary<string, int> CountByStatus { get; set; } = [];
    public int TotalLeads { get; set; }
    public string TotalPipelineValue { get; set; } = string.Empty;
    public List<LeadResponseDto> Leads { get; set; } = [];
}

// ─── Blog ────────────────────────────────────────────
public class BlogPostDto
{
    public int Id { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string PublishedDate { get; set; } = string.Empty;
    public string ReadTime { get; set; } = string.Empty;
    public string Views { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
}

// ─── Testimonial ─────────────────────────────────────
public class TestimonialDto
{
    public int Id { get; set; }
    public string Initials { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Stars { get; set; }
}
