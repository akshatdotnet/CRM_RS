using Microsoft.EntityFrameworkCore;
using PersonalBrand.API.Models.Entities;
using System.Text.Json;

namespace PersonalBrand.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<RoadmapItem> RoadmapItems => Set<RoadmapItem>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<QAItem> QAItems => Set<QAItem>();
    public DbSet<ConsultingService> ConsultingServices => Set<ConsultingService>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadNote> LeadNotes => Set<LeadNote>();
    public DbSet<LeadStatusHistory> LeadStatusHistories => Set<LeadStatusHistory>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Global Query Filters (Soft Delete) ───────────
        modelBuilder.Entity<Persona>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Skill>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RoadmapItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Course>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<QAItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ConsultingService>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Lead>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BlogPost>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Testimonial>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Subscriber>().HasQueryFilter(e => !e.IsDeleted);

        // ─── Lead Relationships ────────────────────────────
        modelBuilder.Entity<Lead>()
            .HasMany(l => l.Notes)
            .WithOne(n => n.Lead)
            .HasForeignKey(n => n.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Lead>()
            .HasMany(l => l.StatusHistory)
            .WithOne(h => h.Lead)
            .HasForeignKey(h => h.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Indexes ───────────────────────────────────────
        modelBuilder.Entity<Lead>().HasIndex(l => l.Email);
        modelBuilder.Entity<Lead>().HasIndex(l => l.Status);
        modelBuilder.Entity<BlogPost>().HasIndex(b => b.Slug).IsUnique();
        modelBuilder.Entity<Subscriber>().HasIndex(s => s.Email).IsUnique();
        modelBuilder.Entity<Skill>().HasIndex(s => s.SortOrder);
        modelBuilder.Entity<RoadmapItem>().HasIndex(r => r.SortOrder);

        // ─── Seed Data ─────────────────────────────────────
        SeedData(modelBuilder);
    }

    // ─── Auto UpdatedAt on SaveChanges ────────────────────
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        return base.SaveChangesAsync(ct);
    }

    private static void SeedData(ModelBuilder mb)
    {
        // ── Persona ──
        mb.Entity<Persona>().HasData(new Persona
        {
            Id = 1,
            FirstName = "Arjun", LastName = "Sharma",
            Title = "Senior .NET Core Developer | Azure Cloud Architect | IT Consultant",
            ShortTitle = "Full-Stack .NET & Azure Expert",
            Experience = 15,
            Location = "Mumbai, India",
            Email = "arjun@dotnetpro.dev",
            Phone = "+91 98765 43210",
            Github = "https://github.com/arjunsharma-dev",
            LinkedIn = "https://linkedin.com/in/arjunsharma-dotnet",
            Twitter = "https://twitter.com/arjundotnet",
            YouTube = "https://youtube.com/@arjunsharmadev",
            Tagline = "Building Scalable Cloud-Native Solutions with .NET & Azure",
            Bio = "With over 15 years of hands-on experience in the .NET ecosystem, I've architected and delivered enterprise-grade solutions across BFSI, Healthcare, E-Commerce, and Government sectors.",
            StatsJson = """{"projects":"120+","clients":"45+","courses":"12+","students":"5000+"}""",
            CertificationsJson = """["AZ-900","AZ-204","AZ-305","AZ-400","MS-600","AWS-SAA","CKA","CKAD"]""",
            CreatedAt = new DateTime(2024, 1, 1), UpdatedAt = new DateTime(2024, 1, 1)
        });

        // ── Skills ──
        var skills = new[]
        {
            new Skill { Id=1, Name=".NET Core / .NET 8", Percentage=98, SortOrder=1, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Skill { Id=2, Name="C# & LINQ", Percentage=97, SortOrder=2, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Skill { Id=3, Name="Azure Cloud Services", Percentage=92, SortOrder=3, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Skill { Id=4, Name="Microservices & DDD", Percentage=90, SortOrder=4, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Skill { Id=5, Name="ASP.NET Core Web API", Percentage=96, SortOrder=5, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Skill { Id=6, Name="Entity Framework Core", Percentage=94, SortOrder=6, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Skill { Id=7, Name="Docker & Kubernetes", Percentage=85, SortOrder=7, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Skill { Id=8, Name="DevOps / CI-CD", Percentage=88, SortOrder=8, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
        };
        mb.Entity<Skill>().HasData(skills);

        // ── Roadmap ──
        mb.Entity<RoadmapItem>().HasData(
            new RoadmapItem { Id=1, Year="2008–2010", Era="Foundation", Title=".NET Framework 2.0 / 3.5", Description="Started with WinForms, ASP.NET WebForms, ADO.NET. Built enterprise desktop and web apps for BFSI clients.", TagsJson="""["C#","ADO.NET","WebForms","WCF","SQL Server"]""", Icon="🏗️", Side="left", SortOrder=1, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new RoadmapItem { Id=2, Year="2010–2013", Era="Growth", Title="MVC & Entity Framework Era", Description="Adopted ASP.NET MVC, EF 4/5, jQuery. Led team of 8 for a large insurance portal.", TagsJson="""["ASP.NET MVC","EF 4/5","jQuery","SSRS","NHibernate"]""", Icon="📐", Side="right", SortOrder=2, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new RoadmapItem { Id=3, Year="2013–2016", Era="API Era", Title="Web API & SignalR", Description="Designed RESTful APIs, real-time dashboards with SignalR. First exposure to Angular 1.x.", TagsJson="""["Web API 2","SignalR","Angular 1.x","OWIN","OAuth2"]""", Icon="🔌", Side="left", SortOrder=3, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new RoadmapItem { Id=4, Year="2016–2018", Era="Cloud Adoption", Title=".NET Core 1.x–2.x & Azure", Description="Early adopter of .NET Core. Migrated legacy apps to Azure. Architected first cloud-native solution.", TagsJson="""[".NET Core 2","Azure App Service","Azure SQL","Blob Storage","Redis"]""", Icon="☁️", Side="right", SortOrder=4, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new RoadmapItem { Id=5, Year="2018–2021", Era="Microservices", Title="Microservices & Containers", Description="Designed microservices with DDD, CQRS, Event Sourcing. Docker & Kubernetes on AKS.", TagsJson="""["Docker","Kubernetes","gRPC","RabbitMQ","Dapr","CQRS","AKS"]""", Icon="🐳", Side="left", SortOrder=5, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new RoadmapItem { Id=6, Year="2021–2023", Era="DevSecOps", Title="DevOps & Platform Engineering", Description="Built full CI/CD pipelines on Azure DevOps. Terraform IaC, Security scanning in pipelines.", TagsJson="""["Azure DevOps","Terraform","GitHub Actions","SonarQube","Helm"]""", Icon="⚙️", Side="right", SortOrder=6, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new RoadmapItem { Id=7, Year="2023–Now", Era="AI & .NET 8", Title=".NET 8 + Azure AI / OpenAI", Description="Integrating Azure OpenAI, Semantic Kernel into enterprise apps. Minimal APIs, Aspire, AI Agents.", TagsJson="""[".NET 8","Aspire","Semantic Kernel","Azure OpenAI","AI Agents","Minimal APIs"]""", Icon="🤖", Side="left", SortOrder=7, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) }
        );

        // ── Courses ──
        mb.Entity<Course>().HasData(
            new Course { Id=1, Icon="🔷", Title="Complete C# Mastery", Level="beginner", Duration="40 hrs", Students=2100, ModulesJson="""["C# Syntax & Data Types","OOP: Classes, Inheritance, Polymorphism","Interfaces & Abstract Classes","Generics & Collections","LINQ Fundamentals & Advanced","Async / Await & TPL","Pattern Matching & Records","C# 12 / .NET 8 Features"]""", SortOrder=1, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Course { Id=2, Icon="🌐", Title="ASP.NET Core Web API", Level="intermediate", Duration="35 hrs", Students=1800, ModulesJson="""["Minimal APIs & Controllers","Dependency Injection deep-dive","Middleware & Filters Pipeline","Entity Framework Core 8","Authentication: JWT & OAuth2","Swagger / OpenAPI Docs","Global Error Handling","Performance Optimization"]""", SortOrder=2, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Course { Id=3, Icon="☁️", Title="Azure Cloud for Developers", Level="intermediate", Duration="45 hrs", Students=950, ModulesJson="""["Azure App Service & Functions","Azure SQL & Cosmos DB","Azure Service Bus & Event Grid","Azure Key Vault & App Config","Blob Storage & CDN","Azure AD B2C / Entra ID","Azure API Management","Monitoring with App Insights"]""", SortOrder=3, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Course { Id=4, Icon="🏗️", Title="Microservices with .NET", Level="advanced", Duration="50 hrs", Students=620, ModulesJson="""["DDD: Bounded Contexts","CQRS & MediatR Pattern","Event Sourcing","gRPC Services","RabbitMQ & Azure Service Bus","Saga Pattern & Outbox","API Gateway with Ocelot/YARP","Distributed Tracing (OTEL)"]""", SortOrder=4, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Course { Id=5, Icon="🐳", Title="Docker & Kubernetes", Level="advanced", Duration="30 hrs", Students=480, ModulesJson="""["Docker Fundamentals","Multi-stage Dockerfile","Docker Compose","Kubernetes Core Concepts","AKS Deployment","Helm Charts","Service Mesh with Istio","K8s Security Hardening"]""", SortOrder=5, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Course { Id=6, Icon="⚡", Title="Azure DevOps & CI/CD", Level="intermediate", Duration="25 hrs", Students=760, ModulesJson="""["Git Branching Strategies","YAML Pipelines","Build & Release Automation","Infrastructure as Code (Terraform)","Container Registry & AKS Deploy","SonarQube Integration","Blue/Green & Canary Releases","Monitoring & Alerting"]""", SortOrder=6, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) }
        );

        // ── Projects ──
        mb.Entity<Project>().HasData(
            new Project { Id=1, Emoji="🏦", Title="Banking Microservices Platform", Industry="BFSI", Problem="Legacy monolith causing slow deployments & outages", Description="Re-architected a core banking system into 18 microservices using DDD & CQRS. Reduced deployment time from 4 hours to 12 minutes.", StackJson="""[".NET 7","AKS","Azure SQL","Service Bus","Dapr","CQRS"]""", GithubUrl="#", DemoUrl="#", Highlight="99.99% uptime achieved", IsFeatured=true, SortOrder=1, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Project { Id=2, Emoji="🏥", Title="Healthcare Patient Portal", Industry="Healthcare", Problem="No unified patient record management system", Description="Built FHIR-compliant patient portal with real-time notifications, secure document storage, and HL7 integration.", StackJson="""[".NET 6","Azure API Management","Cosmos DB","SignalR","FHIR"]""", GithubUrl="#", DemoUrl="#", Highlight="500K+ patients served", IsFeatured=true, SortOrder=2, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Project { Id=3, Emoji="🛒", Title="E-Commerce Recommendation Engine", Industry="Retail", Problem="Low conversion rates due to poor product discovery", Description="Integrated Azure Machine Learning with a .NET 8 microservice to serve personalized product recommendations in <200ms.", StackJson="""[".NET 8","Azure ML","Redis","Event Grid","Minimal APIs"]""", GithubUrl="#", DemoUrl="#", Highlight="32% sales uplift", IsFeatured=false, SortOrder=3, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Project { Id=4, Emoji="🏛️", Title="Government Document Workflow", Industry="Government", Problem="Paper-based approvals taking weeks", Description="Digital workflow system with digital signatures, audit trail, role-based access, and Azure AD integration.", StackJson="""[".NET 6","Azure AD","Blob Storage","SignalR","EF Core 7"]""", GithubUrl="#", DemoUrl="#", Highlight="85% time reduction", IsFeatured=false, SortOrder=4, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Project { Id=5, Emoji="📊", Title="Real-Time Analytics Dashboard", Industry="SaaS", Problem="Business intelligence delayed by hours", Description="Built real-time analytics with Azure Stream Analytics, Event Hub, and a SignalR-powered live dashboard.", StackJson="""["Azure Stream Analytics","Event Hub","SignalR",".NET 7","Power BI Embedded"]""", GithubUrl="#", DemoUrl="#", Highlight="Real-time under 500ms", IsFeatured=false, SortOrder=5, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Project { Id=6, Emoji="🤖", Title="AI-Powered HR Chatbot", Industry="Enterprise", Problem="HR team overwhelmed with repetitive queries", Description="Built internal HR chatbot using Azure OpenAI + Semantic Kernel with RAG over company policy documents.", StackJson="""[".NET 8","Azure OpenAI","Semantic Kernel","Cosmos DB","Azure Search"]""", GithubUrl="#", DemoUrl="#", Highlight="70% query deflection", IsFeatured=true, SortOrder=6, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) }
        );

        // ── Q&A ──
        mb.Entity<QAItem>().HasData(
            new QAItem { Id=1, Level="basic", Category="C#", Question="What is the difference between value types and reference types in C#?", Answer="Value types (int, struct, bool) are stored on the stack and hold data directly. Reference types (class, string, arrays) are stored on the heap, and variables hold a reference. Value types get copied on assignment; reference types share the same object unless explicitly cloned.", SortOrder=1, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new QAItem { Id=2, Level="basic", Category=".NET", Question="What is the difference between .NET Framework and .NET Core / .NET 5+?", Answer=".NET Framework is Windows-only, tied to system installation. .NET Core (now .NET 5+) is cross-platform, open-source, modular, self-contained deployable, and performs significantly better. New development should target .NET 6+ (LTS) or .NET 8.", SortOrder=2, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new QAItem { Id=3, Level="intermediate", Category="ASP.NET Core", Question="How does Dependency Injection work in ASP.NET Core?", Answer="ASP.NET Core has built-in DI via IServiceCollection. You register services as AddTransient (new per request), AddScoped (one per HTTP request), or AddSingleton (one for app lifetime). The DI container resolves dependencies via constructor injection promoting loose coupling and testability.", SortOrder=3, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new QAItem { Id=4, Level="intermediate", Category="ASP.NET Core", Question="What is Middleware in ASP.NET Core and how does the pipeline work?", Answer="Middleware are components in the HTTP request/response pipeline. Each middleware can process the request, pass to next, or short-circuit. Order matters — UseAuthentication must come before UseAuthorization. Added in Program.cs with Use*, Map*, or Run* methods.", SortOrder=4, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new QAItem { Id=5, Level="advanced", Category="Architecture", Question="Explain CQRS pattern with MediatR in a .NET application.", Answer="CQRS separates read (Query) and write (Command) models. MediatR implements the mediator pattern — you send a Command or Query, and a corresponding Handler processes it. This decouples request senders from handlers, makes unit testing trivial, and supports cross-cutting concerns via IPipelineBehavior.", SortOrder=5, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new QAItem { Id=6, Level="azure", Category="Azure", Question="What is the difference between Azure Service Bus and Azure Event Grid?", Answer="Azure Service Bus is a message broker for reliable, ordered delivery (command-style messaging). Azure Event Grid is event routing based on pub/sub — ideal for reactive fan-out scenarios. Use Service Bus for long-running workflows; Event Grid for state-change notifications to multiple subscribers.", SortOrder=6, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new QAItem { Id=7, Level="architecture", Category="System Design", Question="How would you design a URL shortener at scale?", Answer="Core: (1) ASP.NET Core Minimal API; (2) Base62 short code from Snowflake ID; (3) Cosmos DB for storage; (4) Redis for hot URL cache; (5) HTTP 301 redirects; (6) Event Hub → Stream Analytics for click analytics; (7) Azure CDN for edge caching. Handles ~100K redirects/sec.", SortOrder=7, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) }
        );

        // ── Consulting Services ──
        mb.Entity<ConsultingService>().HasData(
            new ConsultingService { Id=1, Icon="💻", Title="Contract Development", Price="₹8,000", Period="/ day", Description="Full-stack .NET development", IsFeatured=false, FeaturesJson="""["ASP.NET Core / Web API","Azure Cloud Implementation","Code Review & Architecture","Technical Documentation","Up to 8 hrs/day commitment"]""", SortOrder=1, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new ConsultingService { Id=2, Icon="🏗️", Title="Architecture Consulting", Price="₹15,000", Period="/ day", Description="Cloud-native solution architecture", IsFeatured=true, FeaturesJson="""["System Design & Architecture Review","Microservices Strategy","Azure Landing Zone Design","DevOps Pipeline Setup","Team Mentoring","Proof of Concept Delivery","Post-implementation Support"]""", SortOrder=2, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new ConsultingService { Id=3, Icon="🎓", Title="Corporate Training", Price="₹50,000", Period="/ batch", Description="Team upskilling programs", IsFeatured=false, FeaturesJson="""["Customized curriculum",".NET / Azure / DevOps topics","Hands-on lab exercises","Q&A + recorded sessions","Certificate of completion","Up to 25 participants"]""", SortOrder=3, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) }
        );

        // ── Sample Leads ──
        mb.Entity<Lead>().HasData(
            new Lead { Id=1, Name="Priya Mehta", Email="priya@fintech.io", Role="CTO, FinTech Startup", Service="Architecture Consulting", Budget="₹5L – ₹15L", Message="Microservices migration project", Status="new", Value="₹12L", Source="contact-form", CreatedAt=new DateTime(2024,1,10), UpdatedAt=new DateTime(2024,1,10) },
            new Lead { Id=2, Name="Rahul Kapoor", Email="rahul@bank.com", Role="VP Eng, MNC Bank", Service="Architecture Consulting", Budget="₹5L – ₹15L", Message="Azure cloud migration consulting", Status="contacted", Value="₹8L", Source="linkedin", CreatedAt=new DateTime(2024,1,8), UpdatedAt=new DateTime(2024,1,8) },
            new Lead { Id=3, Name="Sunita Rao", Email="sunita@itcorp.com", Role="L&D Head, IT Corp", Service="Corporate Training", Budget="₹1L – ₹5L", Message=".NET Core training for 20 devs", Status="proposal", Value="₹2.5L", Source="referral", CreatedAt=new DateTime(2024,1,5), UpdatedAt=new DateTime(2024,1,5) },
            new Lead { Id=4, Name="Amir Sheikh", Email="amir@saas.io", Role="Founder, SaaS Co", Service="Contract Development", Budget="₹15L+", Message="Full-stack development contract 6 months", Status="negotiation", Value="₹18L", Source="contact-form", CreatedAt=new DateTime(2024,1,2), UpdatedAt=new DateTime(2024,1,2) },
            new Lead { Id=5, Name="Deepika Joshi", Email="deepika@hospital.in", Role="IT Director, Hospital", Service="Contract Development", Budget="₹1L – ₹5L", Message="Healthcare portal development", Status="closed", Value="₹5L", Source="referral", CreatedAt=new DateTime(2023,12,20), UpdatedAt=new DateTime(2023,12,20) }
        );

        mb.Entity<LeadNote>().HasData(
            new LeadNote { Id=1, LeadId=2, Note="Sent intro email on Jan 8", AddedBy="Owner", CreatedAt=new DateTime(2024,1,8), UpdatedAt=new DateTime(2024,1,8) },
            new LeadNote { Id=2, LeadId=2, Note="Scheduled call for Monday", AddedBy="Owner", CreatedAt=new DateTime(2024,1,9), UpdatedAt=new DateTime(2024,1,9) },
            new LeadNote { Id=3, LeadId=3, Note="Proposal sent Jan 6", AddedBy="Owner", CreatedAt=new DateTime(2024,1,6), UpdatedAt=new DateTime(2024,1,6) },
            new LeadNote { Id=4, LeadId=5, Note="Project signed. Kickoff on Jan 15", AddedBy="Owner", CreatedAt=new DateTime(2023,12,22), UpdatedAt=new DateTime(2023,12,22) }
        );

        // ── Blog Posts ──
        mb.Entity<BlogPost>().HasData(
            new BlogPost { Id=1, Emoji="⚡", Category="Performance", Title="10 EF Core Tricks That Will 10x Your Query Performance", Excerpt="From compiled queries to split queries — practical patterns from production banking systems.", Content="Full article content here...", Slug="ef-core-performance-tricks", PublishedDate=new DateTime(2024,1,8), ReadTime="8 min", ViewCount=4200, TagsJson="""["EF Core","Performance",".NET","Database"]""", CreatedAt=new DateTime(2024,1,8), UpdatedAt=new DateTime(2024,1,8) },
            new BlogPost { Id=2, Emoji="☁️", Category="Azure", Title="Azure Service Bus vs Event Grid: Decision Tree", Excerpt="Stop using the wrong messaging service. A clear decision framework with .NET code examples.", Content="Full article content here...", Slug="azure-service-bus-vs-event-grid", PublishedDate=new DateTime(2023,12,28), ReadTime="6 min", ViewCount=3800, TagsJson="""["Azure","Messaging","Architecture"]""", CreatedAt=new DateTime(2023,12,28), UpdatedAt=new DateTime(2023,12,28) },
            new BlogPost { Id=3, Emoji="🐳", Category="DevOps", Title="Zero-Downtime Deployments on AKS with .NET 8", Excerpt="Blue/green deployments on Azure Kubernetes Service with real YAML configs.", Content="Full article content here...", Slug="zero-downtime-aks-dotnet8", PublishedDate=new DateTime(2023,12,15), ReadTime="12 min", ViewCount=2900, TagsJson="""["AKS","DevOps","Kubernetes",".NET 8"]""", CreatedAt=new DateTime(2023,12,15), UpdatedAt=new DateTime(2023,12,15) },
            new BlogPost { Id=4, Emoji="🤖", Category="AI / .NET", Title="Building RAG with Semantic Kernel & Azure OpenAI", Excerpt="Build a document Q&A chatbot using .NET 8, Semantic Kernel, and Azure Cognitive Search.", Content="Full article content here...", Slug="rag-semantic-kernel-azure-openai", PublishedDate=new DateTime(2023,11,30), ReadTime="15 min", ViewCount=5100, TagsJson="""["AI","Semantic Kernel","Azure OpenAI",".NET 8"]""", CreatedAt=new DateTime(2023,11,30), UpdatedAt=new DateTime(2023,11,30) },
            new BlogPost { Id=5, Emoji="🏗️", Category="Architecture", Title="CQRS + MediatR: The Right Way", Excerpt="After implementing CQRS in 10+ enterprise projects, here's what actually works.", Content="Full article content here...", Slug="cqrs-mediatr-right-way", PublishedDate=new DateTime(2023,11,18), ReadTime="10 min", ViewCount=6700, TagsJson="""["CQRS","Architecture","MediatR",".NET"]""", CreatedAt=new DateTime(2023,11,18), UpdatedAt=new DateTime(2023,11,18) },
            new BlogPost { Id=6, Emoji="🔐", Category="Security", Title="Securing .NET Microservices End-to-End on Azure", Excerpt="JWT, API Management, Managed Identity, Key Vault — complete security architecture with code.", Content="Full article content here...", Slug="securing-dotnet-microservices-azure", PublishedDate=new DateTime(2023,11,5), ReadTime="14 min", ViewCount=3400, TagsJson="""["Security","Azure","Microservices",".NET"]""", CreatedAt=new DateTime(2023,11,5), UpdatedAt=new DateTime(2023,11,5) }
        );

        // ── Testimonials ──
        mb.Entity<Testimonial>().HasData(
            new Testimonial { Id=1, Initials="PK", Name="Pankaj Kumar", Company="CTO — FinEdge Technologies", Text="Arjun transformed our legacy monolith into a modern microservices architecture on Azure. His deep understanding of both .NET and cloud-native patterns saved us 6 months of development time.", Stars=5, SortOrder=1, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Testimonial { Id=2, Initials="SM", Name="Sunita Menon", Company="L&D Head — Infosys BPM", Text="We hired Arjun to train 40 developers on .NET Core and Azure. The sessions were incredibly practical — real production scenarios, not just theory. Team confidence shot up dramatically.", Stars=5, SortOrder=2, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Testimonial { Id=3, Initials="AV", Name="Amit Verma", Company="VP Engineering — HDFC FinTech", Text="Best Azure architecture consultant we've worked with. Arjun designed our entire cloud infrastructure from scratch, delivered on time, within budget, and it scales flawlessly at 2M+ daily transactions.", Stars=5, SortOrder=3, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Testimonial { Id=4, Initials="RJ", Name="Riya Joshi", Company="Founder — HealthTrack SaaS", Text="Arjun's contract development work was outstanding. He built our entire backend in .NET 8 with full Azure integration in 3 months. Clean code, proper documentation.", Stars=5, SortOrder=4, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) },
            new Testimonial { Id=5, Initials="DN", Name="Deepak Nair", Company="Director — TechBridge Consulting", Text="Arjun's online .NET Core course is the best investment for my engineering team. The CQRS and microservices modules are particularly brilliant — exactly what's needed for real enterprise projects.", Stars=5, SortOrder=5, CreatedAt=new DateTime(2024,1,1), UpdatedAt=new DateTime(2024,1,1) }
        );
    }
}
