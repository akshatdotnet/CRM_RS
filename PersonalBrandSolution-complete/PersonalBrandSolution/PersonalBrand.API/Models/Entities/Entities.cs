using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.API.Models.Entities;

// ─── Base Entity ─────────────────────────────────────
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}

// ─── Persona / Profile ───────────────────────────────
public class Persona : BaseEntity
{
    [MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(200)] public string ShortTitle { get; set; } = string.Empty;
    public int Experience { get; set; }
    [MaxLength(100)] public string Location { get; set; } = string.Empty;
    [MaxLength(200)] public string Email { get; set; } = string.Empty;
    [MaxLength(50)] public string Phone { get; set; } = string.Empty;
    [MaxLength(300)] public string Github { get; set; } = string.Empty;
    [MaxLength(300)] public string LinkedIn { get; set; } = string.Empty;
    [MaxLength(300)] public string Twitter { get; set; } = string.Empty;
    [MaxLength(300)] public string YouTube { get; set; } = string.Empty;
    [MaxLength(500)] public string Tagline { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    // Stats (JSON or separate table; stored as string for simplicity)
    public string StatsJson { get; set; } = "{}";
    public string CertificationsJson { get; set; } = "[]";
}

// ─── Skill ───────────────────────────────────────────
public class Skill : BaseEntity
{
    [MaxLength(150)] public string Name { get; set; } = string.Empty;
    public int Percentage { get; set; }
    public int SortOrder { get; set; }
}

// ─── Roadmap Item ────────────────────────────────────
public class RoadmapItem : BaseEntity
{
    [MaxLength(50)]  public string Year { get; set; } = string.Empty;
    [MaxLength(100)] public string Era { get; set; } = string.Empty;
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TagsJson { get; set; } = "[]";   // JSON array
    [MaxLength(10)]  public string Icon { get; set; } = string.Empty;
    [MaxLength(10)]  public string Side { get; set; } = "left";
    public int SortOrder { get; set; }
}

// ─── Course ──────────────────────────────────────────
public class Course : BaseEntity
{
    [MaxLength(10)]  public string Icon { get; set; } = string.Empty;
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(20)]  public string Level { get; set; } = string.Empty;  // beginner/intermediate/advanced
    [MaxLength(50)]  public string Duration { get; set; } = string.Empty;
    public int Students { get; set; }
    public string ModulesJson { get; set; } = "[]";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// ─── Project ─────────────────────────────────────────
public class Project : BaseEntity
{
    [MaxLength(10)]  public string Emoji { get; set; } = string.Empty;
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(100)] public string Industry { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StackJson { get; set; } = "[]";
    [MaxLength(500)] public string GithubUrl { get; set; } = string.Empty;
    [MaxLength(500)] public string DemoUrl { get; set; } = string.Empty;
    [MaxLength(200)] public string Highlight { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; } = false;
}

// ─── Q&A ─────────────────────────────────────────────
public class QAItem : BaseEntity
{
    [MaxLength(20)]  public string Level { get; set; } = string.Empty;     // basic/intermediate/advanced/azure/architecture
    [MaxLength(100)] public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// ─── Service/Consulting ──────────────────────────────
public class ConsultingService : BaseEntity
{
    [MaxLength(10)]  public string Icon { get; set; } = string.Empty;
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(50)]  public string Price { get; set; } = string.Empty;
    [MaxLength(50)]  public string Period { get; set; } = string.Empty;
    [MaxLength(300)] public string Description { get; set; } = string.Empty;
    public bool IsFeatured { get; set; } = false;
    public string FeaturesJson { get; set; } = "[]";
    public int SortOrder { get; set; }
}

// ─── Lead / CRM ──────────────────────────────────────
public class Lead : BaseEntity
{
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(200)] public string Email { get; set; } = string.Empty;
    [MaxLength(200)] public string Role { get; set; } = string.Empty;
    [MaxLength(100)] public string Service { get; set; } = string.Empty;
    [MaxLength(100)] public string Budget { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    [MaxLength(30)]  public string Status { get; set; } = "new";
    [MaxLength(100)] public string Value { get; set; } = string.Empty;
    public string Source { get; set; } = "contact-form";
    public ICollection<LeadNote> Notes { get; set; } = [];
    public ICollection<LeadStatusHistory> StatusHistory { get; set; } = [];
}

public class LeadNote : BaseEntity
{
    public int LeadId { get; set; }
    public Lead? Lead { get; set; }
    public string Note { get; set; } = string.Empty;
    [MaxLength(200)] public string AddedBy { get; set; } = "Owner";
}

public class LeadStatusHistory : BaseEntity
{
    public int LeadId { get; set; }
    public Lead? Lead { get; set; }
    [MaxLength(30)] public string FromStatus { get; set; } = string.Empty;
    [MaxLength(30)] public string ToStatus { get; set; } = string.Empty;
    [MaxLength(200)] public string ChangedBy { get; set; } = "Owner";
}

// ─── Blog / Article ──────────────────────────────────
public class BlogPost : BaseEntity
{
    [MaxLength(10)]  public string Emoji { get; set; } = string.Empty;
    [MaxLength(100)] public string Category { get; set; } = string.Empty;
    [MaxLength(300)] public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    [MaxLength(300)] public string Slug { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
    [MaxLength(20)]  public string ReadTime { get; set; } = string.Empty;
    public int ViewCount { get; set; } = 0;
    public bool IsPublished { get; set; } = true;
    public string TagsJson { get; set; } = "[]";
}

// ─── Testimonial ─────────────────────────────────────
public class Testimonial : BaseEntity
{
    [MaxLength(10)]  public string Initials { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(300)] public string Company { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Stars { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

// ─── Newsletter Subscriber ───────────────────────────
public class Subscriber : BaseEntity
{
    [MaxLength(200)] public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Source { get; set; } = "footer";
}
