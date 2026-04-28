# 🔷 PersonalBrand — Full-Stack .NET Solution
## ASP.NET Core Web API + MVC + EF Core + SQLite/SQL Server

---

## 📐 Solution Architecture

```
PersonalBrand.sln
├── PersonalBrand.API          ← REST API (port 5000)
│   ├── Controllers/           ← 12 versioned API controllers
│   ├── Data/                  ← EF Core DbContext + Seed data
│   ├── Models/Entities/       ← 12 domain entities
│   ├── Models/DTOs/           ← Response DTOs
│   ├── Repositories/          ← Repository pattern (Interface + Impl)
│   ├── Services/              ← Business logic (Interface + Impl)
│   ├── Middleware/            ← Global exception + request logging
│   ├── Extensions/            ← Clean DI registration
│   └── appsettings.json       ← SQLite (dev) / SQL Server (prod)
│
├── PersonalBrand.MVC          ← MVC Website (port 5001)
│   ├── Controllers/           ← HomeController with all actions
│   ├── Services/              ← PersonalBrandApiClient (typed HttpClient)
│   ├── ViewModels/            ← Strongly-typed view models
│   ├── Views/                 ← Razor views (server-rendered)
│   ├── wwwroot/               ← CSS + JS
│   └── appsettings.json       ← API base URL config
│
└── PersonalBrand.Shared       ← Shared DTOs + constants
    ├── Models/SharedModels.cs ← ApiResponse<T>, ContactFormDto, etc.
    └── Constants/             ← LeadStatus, CacheKeys, ServiceTypes
```

---

## 🚀 Quick Start

### Step 1 — Run the API
```bash
cd PersonalBrand.API
dotnet restore
dotnet run
# → API runs at http://localhost:5000
# → Swagger UI at http://localhost:5000/swagger
# → SQLite DB auto-created & seeded on first run
```

### Step 2 — Run the MVC app
```bash
# In a new terminal
cd PersonalBrand.MVC
dotnet restore
dotnet run
# → Website at http://localhost:5001
```

---

## 🗄️ Database

| Environment | Provider | Config |
|---|---|---|
| Development | **SQLite** | `Data Source=personalbrand.db` (auto-created) |
| Production | **SQL Server** | `Server=...;Database=PersonalBrand;` |

**Switching to SQL Server:**
```json
// PersonalBrand.API/appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=myserver;Database=PersonalBrand;User Id=sa;Password=xxx;TrustServerCertificate=True"
  }
}
```

**EF Core Migrations:**
```bash
cd PersonalBrand.API
dotnet ef migrations add InitialCreate --output-dir Migrations
dotnet ef database update
```

---

## 📡 API Endpoints

All endpoints versioned under `/api/v1/`

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/persona` | Profile info |
| GET | `/api/v1/skills` | Skill proficiency bars |
| GET | `/api/v1/roadmap` | Career timeline |
| GET | `/api/v1/courses` | Course catalog |
| GET | `/api/v1/projects?featured=true` | Portfolio projects |
| GET | `/api/v1/qa?level=advanced&search=CQRS` | Filtered Q&A |
| GET | `/api/v1/services` | Consulting services |
| GET | `/api/v1/leads/pipeline` | CRM pipeline summary |
| GET | `/api/v1/leads` | All leads |
| POST | `/api/v1/leads` | Create lead from contact form |
| POST | `/api/v1/leads/{id}/notes` | Add note to lead |
| PATCH | `/api/v1/leads/{id}/status` | Update lead status |
| GET | `/api/v1/blog?page=1&pageSize=6` | Paginated blog posts |
| GET | `/api/v1/blog/{slug}` | Single post (increments view count) |
| GET | `/api/v1/testimonials` | Client testimonials |
| POST | `/api/v1/newsletter/subscribe` | Subscribe to newsletter |
| GET | `/health` | Health check |

---

## 🔑 Best Practices Implemented

### API
- ✅ **Repository Pattern** — `IRepository<T>` base + specific interfaces
- ✅ **Service Layer** — all business logic isolated in services
- ✅ **API Versioning** — `api/v{version}/` with Asp.Versioning
- ✅ **Memory Caching** — all read-heavy endpoints cached (1hr–6hr)
- ✅ **Soft Delete** — `IsDeleted` flag + global EF query filters
- ✅ **Global Exception Middleware** — structured JSON error responses
- ✅ **Request Logging Middleware** — method/path/status/elapsed
- ✅ **Serilog** — console + rolling file logs
- ✅ **Swagger / OpenAPI** — full API documentation
- ✅ **Seed Data** — all 12 entities seeded via `HasData()`
- ✅ **Auto UpdatedAt** — set in `SaveChangesAsync` override
- ✅ **CORS** — configured for MVC origin only
- ✅ **Response Compression** — gzip enabled

### MVC
- ✅ **Typed HttpClient** — `PersonalBrandApiClient` with DI
- ✅ **Polly Retry** — 3 retries with exponential backoff
- ✅ **Polly Circuit Breaker** — opens after 5 failures, 30s reset
- ✅ **Parallel API Calls** — `Task.WhenAll()` on Index page load
- ✅ **MVC Cache** — 5-min response cache on client side
- ✅ **Anti-Forgery Tokens** — on all POST forms
- ✅ **Server-side Toast** — via TempData after POST-redirect-GET
- ✅ **Strongly-Typed Views** — `HomeViewModel` passed to Razor
- ✅ **Client-side Q&A filter** — uses server data, no extra round-trip
- ✅ **AJAX Lead Update** — status/note changes without page reload
- ✅ **SEO** — meta tags, OG tags, Schema.org JSON-LD, canonical
- ✅ **GDPR** — cookie consent banner
- ✅ **WCAG** — ARIA labels, semantic HTML
- ✅ **Static file caching** — 30-day Cache-Control headers

---

## 🔧 Configuration

### MVC → API URL
```json
// PersonalBrand.MVC/appsettings.json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5000/"
  }
}
```

### CORS Origins (API allows MVC)
```json
// PersonalBrand.API/appsettings.json
{
  "AllowedOrigins": [
    "http://localhost:5001",
    "https://yourdomain.com"
  ]
}
```

---

## 🚀 Production Deployment (Azure)

```bash
# API → Azure App Service
az webapp up --name personalbrand-api --resource-group rg-personalbrand --runtime "DOTNET|8.0"

# MVC → Azure App Service
az webapp up --name personalbrand-web --resource-group rg-personalbrand --runtime "DOTNET|8.0"

# Database → Azure SQL
az sql db create --name PersonalBrand --server myserver --resource-group rg-personalbrand --tier S0
```

---

## 📦 NuGet Packages

### API
| Package | Purpose |
|---|---|
| EF Core + SQLite + SQL Server | ORM & databases |
| Swashbuckle | Swagger UI |
| Asp.Versioning.Mvc | API versioning |
| Serilog | Structured logging |
| Microsoft.Extensions.Caching.Memory | In-memory cache |

### MVC
| Package | Purpose |
|---|---|
| Microsoft.Extensions.Http.Polly | HttpClient resilience |
| Polly.Extensions.Http | Retry + Circuit Breaker |
| Serilog.AspNetCore | Structured logging |
| Razor.RuntimeCompilation | Hot reload views |
