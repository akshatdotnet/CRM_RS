namespace PersonalBrand.Shared.Constants;

public static class LeadStatus
{
    public const string New = "new";
    public const string Contacted = "contacted";
    public const string Proposal = "proposal";
    public const string Negotiation = "negotiation";
    public const string Closed = "closed";
    public const string Lost = "lost";

    public static readonly string[] All = [New, Contacted, Proposal, Negotiation, Closed, Lost];
}

public static class ServiceTypes
{
    public const string ContractDevelopment = "Contract Development";
    public const string ArchitectureConsulting = "Architecture Consulting";
    public const string CorporateTraining = "Corporate Training";
    public const string CodeReview = "Code Review";
    public const string Other = "Other";
}

public static class CacheKeys
{
    public const string Persona = "persona";
    public const string Skills = "skills";
    public const string Roadmap = "roadmap";
    public const string Courses = "courses";
    public const string Projects = "projects";
    public const string QA = "qa";
    public const string Services = "services";
    public const string Blogs = "blogs";
    public const string Testimonials = "testimonials";
    public const string LeadsAll = "leads_all";
}
