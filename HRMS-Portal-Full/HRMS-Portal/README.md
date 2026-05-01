# 🏢 HRMS Portal — Phase 1
### .NET 8 Clean Architecture | JWT Auth | EF Core | REST API

---

## ⚡ Quick Start (3 steps)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ← required
- Internet connection (first run downloads NuGet packages)

### Run on Windows
```cmd
setup-windows.bat
```

### Run on Linux / macOS
```bash
chmod +x setup-unix.sh && ./setup-unix.sh
```

### Or manually
```bash
dotnet restore HRMS.sln
dotnet build HRMS.sln
cd src/HRMS.Web
dotnet run
```

Then open: **http://localhost:5000/swagger**

---

## 🔐 Demo Login Credentials

| Role     | Email                       | Password      |
|----------|-----------------------------|---------------|
| **Admin**    | admin@acmecorp.in       | `Admin@123`   |
| **HR**       | priya.sharma@acmecorp.in| `Hr@12345`    |
| **Employee** | rahul.verma@acmecorp.in | `Welcome@123` |
| **Employee** | anjali.mehta@acmecorp.in| `Welcome@123` |

> **How to login in Swagger:**
> 1. POST `/api/auth/login` → copy `accessToken`
> 2. Click **Authorize** button (top right) → paste `Bearer <your_token>`

---

## 📁 Project Structure

```
HRMS/
├── HRMS.sln
├── setup-windows.bat
├── setup-unix.sh
└── src/
    ├── HRMS.Domain/              ← Entities, Enums (no dependencies)
    │   ├── Common/BaseEntity.cs
    │   ├── Entities/             Employee, User, SalarySlip, Document, SalaryStructure
    │   └── Enums/                UserRole, Department, EmploymentStatus, etc.
    │
    ├── HRMS.Application/         ← Business logic (depends on Domain only)
    │   ├── Common/               ApiResponse<T>, PagedResult<T>
    │   ├── DTOs/                 Auth, Employee, Documents, Salary
    │   ├── Interfaces/           IGenericRepository, IUnitOfWork, all service interfaces
    │   ├── Mappings/             AutoMapper profile
    │   ├── Services/             AuthService, EmployeeService, SalarySlipService, DocumentService
    │   └── Validators/           FluentValidation validators
    │
    ├── HRMS.Infrastructure/      ← Data access, external services
    │   ├── Persistence/          HrmsDbContext, EF Configurations, DataSeeder
    │   ├── Repositories/         Generic + specific repository implementations
    │   └── Services/             JWT, Email (SMTP/mock), PDF (PuppeteerSharp)
    │
    └── HRMS.Web/                 ← API layer
        ├── Controllers/          Auth, Employees, SalarySlips, Documents, Health
        ├── Extensions/           DI registration
        ├── Middleware/           Exception handling, Request logging
        └── Program.cs
```

---

## 🌐 API Endpoints

### 🔑 Authentication
| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| POST | `/api/auth/login` | Public | Login → get JWT |
| POST | `/api/auth/refresh` | Public | Refresh token |
| POST | `/api/auth/logout` | Any | Revoke token |
| POST | `/api/auth/change-password` | Any | Change password |

### 👤 Employees
| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| GET | `/api/employees` | HR/Admin | Paged list with filters |
| GET | `/api/employees/{id}` | Any* | Get employee detail |
| POST | `/api/employees` | HR/Admin | Create employee + user account |
| PUT | `/api/employees/{id}` | HR/Admin | Update employee |
| DELETE | `/api/employees/{id}` | Admin | Soft delete |
| PUT | `/api/employees/{id}/salary` | HR/Admin | Update salary structure |

### 💰 Salary Slips
| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| POST | `/api/salaryslips` | HR/Admin | Generate single slip |
| POST | `/api/salaryslips/bulk?month=&year=` | HR/Admin | Generate for all employees |
| GET | `/api/salaryslips/{id}` | Any* | Get slip details |
| GET | `/api/salaryslips/employee/{empId}` | Any* | All slips for employee |
| GET | `/api/salaryslips/{id}/pdf` | Any* | Download PDF |
| POST | `/api/salaryslips/{id}/send` | HR/Admin | Email to employee |

### 📄 Documents
| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| GET | `/api/documents/employee/{empId}` | Any* | List employee documents |
| POST | `/api/documents/employee/{empId}/offer-letter` | HR/Admin | Generate offer letter |
| POST | `/api/documents/employee/{empId}/appointment-letter` | HR/Admin | Generate appointment letter |
| POST | `/api/documents/employee/{empId}/experience-letter` | HR/Admin | Generate experience letter |
| POST | `/api/documents/employee/{empId}/form16?financialYear=` | HR/Admin | Generate Form 16 |
| GET | `/api/documents/{docId}/pdf` | Any* | Download PDF |
| POST | `/api/documents/{docId}/send` | HR/Admin | Email document |

### 🏥 Health
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/health` | Health check |

*Employees can only access their own records.

---

## ⚙️ Configuration (`src/HRMS.Web/appsettings.json`)

### Use SQL Server instead of InMemory (Phase-2)
```json
{
  "Database": {
    "UseInMemory": false
  },
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=HrmsDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Then run migrations:
```bash
cd src/HRMS.Infrastructure
dotnet ef migrations add Init --startup-project ../HRMS.Web
dotnet ef database update --startup-project ../HRMS.Web
```

### Enable real SMTP email
```json
{
  "Email": {
    "Mock": false,
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "you@gmail.com",
    "Password": "your-app-password",
    "From": "hr@yourcompany.com",
    "FromName": "HR Team"
  }
}
```

### PDF Generation Note
PDF generation uses **PuppeteerSharp** (headless Chrome). On first run it downloads Chromium (~150MB).
If you're on Linux, install Chrome dependencies:
```bash
sudo apt-get install -y libatk1.0-0 libcups2 libnspr4 libnss3 libxss1 libxtst6 xdg-utils
```

---



---

## 🖥️ Web Portal (ASP.NET Core MVC)

The Web Portal (`HRMS.WebPortal`) is a full server-side MVC UI that talks to the API.

### Start both together

**Windows:**
```cmd
run-all.bat
```

**Linux/Mac:**
```bash
chmod +x run-all.sh && ./run-all.sh
```

| URL | What |
|-----|------|
| `http://localhost:5000/swagger` | REST API + Swagger docs |
| `http://localhost:5001` | MVC Web Portal UI |

---

### Portal Pages

| Page | URL | Roles |
|------|-----|-------|
| Login | `/Auth/Login` | Public |
| Dashboard | `/Dashboard` | All |
| Employee List | `/Employee` | All (employees see limited data) |
| Add Employee | `/Employee/Create` | HR, Admin |
| Employee Detail | `/Employee/Detail/{id}` | Own record / HR / Admin |
| Salary Slips | `/Salary/Index?employeeId=...` | Own slips / HR / Admin |
| Generate Salary Slip | `/Salary/Generate` | HR, Admin |
| Documents | `/Document/Index?employeeId=...` | HR, Admin |
| Generate Offer Letter | `/Document/GenerateOffer` | HR, Admin |

### Quick Login (click-to-fill on login page)

The login page shows three quick-fill buttons — click any to auto-fill credentials:

| Button | Email | Password |
|--------|-------|----------|
| Admin  | admin@acmecorp.in | Admin@123 |
| HR     | priya.sharma@acmecorp.in | Hr@12345 |
| Employee | rahul.verma@acmecorp.in | Welcome@123 |

### Portal Architecture

```
HRMS.WebPortal
├── Controllers/       AuthController, DashboardController, EmployeeController
│                      SalaryController, DocumentController
├── Models/            ViewModels per module (not shared with API DTOs)
├── Services/          ApiClient — typed HTTP client calling HRMS.Web API
├── Views/             Razor views per controller
│   ├── Auth/          Login.cshtml, AccessDenied.cshtml
│   ├── Dashboard/     Index.cshtml
│   ├── Employee/      Index.cshtml, Create.cshtml, Detail.cshtml
│   ├── Salary/        Index.cshtml, Generate.cshtml
│   ├── Document/      Index.cshtml, GenerateOffer.cshtml
│   └── Shared/        _Layout.cshtml
└── wwwroot/
    ├── css/hrms.css   Custom design system (dark sidebar, stat cards, tables)
    └── js/site.js     Salary live preview, quick-fill, search debounce
```

### Design System

- **Font**: DM Sans (UI) + DM Mono (codes/numbers)
- **Sidebar**: Dark navy with accent highlights
- **Theme**: Corporate indigo accent (`#4f6ef7`)
- **Components**: Stat cards, data tables with filters, paginated lists,
  salary preview calculator, badge system for statuses

---

## 🔒 Security Checklist (Before Production)

- [ ] Change `Jwt:SecretKey` to a random 256-bit value (`openssl rand -base64 32`)
- [ ] Set `Email:Mock` to `false`
- [ ] Set `Database:UseInMemory` to `false`
- [ ] Force HTTPS (`RequireHttpsMetadata = true`)
- [ ] Restrict CORS to your domain
- [ ] Store secrets in environment variables or a vault

---

## 📦 NuGet Packages Used

| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore.InMemory` | Dev/test database |
| `Microsoft.EntityFrameworkCore.SqlServer` | Production database |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT auth |
| `BCrypt.Net-Next` | Password hashing |
| `AutoMapper` | Entity ↔ DTO mapping |
| `FluentValidation.AspNetCore` | Input validation |
| `Serilog.AspNetCore` | Structured logging |
| `Swashbuckle.AspNetCore` | Swagger/OpenAPI |
| `PuppeteerSharp` | PDF generation |

==============================================================================

HRMS Portal — Conversation Summary

1. Problem Statement

Client needed a production-ready Phase-1 HRMS (Human Resource Management System) built on .NET 8 Clean Architecture with JWT auth, RBAC, employee lifecycle management, and India-specific compliance modules (Form 16, salary slips)
Required 7 core modules: Authentication, Employee Onboarding, Offer Letter, Appointment Letter, Experience Letter, Salary Slip, Form 16 — plus a full MVC Web Portal UI on top of the REST API
Project had to be immediately runnable locally with seed data, zero manual DB setup, and download-ready as a ZIP


2. Solution

4-layer Clean Architecture: HRMS.Domain → HRMS.Application → HRMS.Infrastructure → HRMS.Web (API) + HRMS.WebPortal (MVC UI)
REST API (localhost:5000) with JWT auth, EF Core InMemory DB, AutoMapper, FluentValidation, Serilog, PuppeteerSharp PDF generation, mock SMTP email
MVC Web Portal (localhost:5001) with cookie-based auth, typed ApiClient (never hits DB directly), dark-themed custom CSS design system, click-to-fill seed user login
3 build errors fixed mid-session: raw string literal CSS/interpolation conflict (CS9006), NuGet CVE warnings (bumped all packages to latest patched versions), missing project references in HRMS.WebPortal.csproj
Delivered as downloadable ZIP with run-all.bat / run-all.sh to start both projects simultaneously


3. Best Practices / Key Takeaways
Architecture

Never expose domain entities directly — always map through DTOs at the boundary
Unit of Work wraps all repositories; transaction scope is explicit (BeginTransaction / Commit / Rollback)
MVC portals calling a backing API should use a typed HTTP client (ApiClient) — keeps the UI layer stateless and the auth token isolated in cookie claims

C# / .NET Specifics

Raw string literals ($"""...""") and CSS curly braces {} conflict at compile time — use StringBuilder.AppendFormat() for HTML templates with heavy CSS
Pin NuGet packages to patch-level versions (e.g., 8.0.11 not 8.0.0) — .0 releases frequently carry known CVEs that show up immediately as build warnings
InMemory EF Core does not support real transactions — design the UoW interface to be swappable; SQL Server activates it with zero code changes

Security

JWT for API endpoints + Cookie auth for MVC views is the correct hybrid — not JWT-in-cookies
BCrypt all passwords; store PAN/Aadhaar masked; Form 16 requires CA digital signature before production use — scaffold it but flag compliance risk explicitly in the template output

Delivery

Seed data with fixed GUIDs so tests and demo flows are reproducible across restarts
Ship run-all.bat + run-all.sh alongside the ZIP — reduces "how do I start this" friction to zero
Live salary calculators in forms (JS input event → AppendFormat preview) dramatically improve data-entry UX with zero backend roundtrip
2 / 2