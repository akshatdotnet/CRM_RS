# RealEstate CRM — Enterprise System
### .NET 8 · MVC · Web API · EF Core · Bootstrap 5 · jQuery · SQL Server

---

## 📁 Project Structure

```
RealEstateCRM/
├── CRM.Core/               # Entities + Interfaces (zero dependencies)
├── CRM.Infrastructure/     # EF Core DbContext, Repositories, Migrations
├── CRM.Web/                # ASP.NET Core MVC — all pages and controllers
└── CRM.API/                # REST Web API with Swagger
```

---

## ✅ Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| SQL Server | LocalDB (included with VS 2022) / SQL Express |
| Visual Studio | 2022 v17+ |

---

## 🚀 Quick Start (3 Steps)

### Step 1 — Open the solution
Open `RealEstateCRM.sln` in Visual Studio 2022

### Step 2 — Run CRM.Web
Press **F5** or `dotnet run` from the CRM.Web folder.  
The database is **auto-created and seeded** on first launch.

### Step 3 — Open browser
- **Admin CRM:** https://localhost:5001  
- **API Swagger:** https://localhost:7001/swagger (run CRM.API separately)

---

## 🌐 All Pages

| Page | URL |
|------|-----|
| Dashboard | / |
| Pipeline (Kanban) | /Lead/Pipeline |
| All Leads | /Lead |
| Add Lead | /Lead/Create |
| **Properties (Admin)** | **/Property** |
| **Add Property** | **/Property/Create** |
| Customers | /Customer |
| Analytics | /Analytics |
| **Public Property Page** | **/p/{slug}** |
| **Client Deal Confirmation** | **/p/{slug}/confirm** |

---

## 🏠 Property Module

### Admin flow:
1. Go to **Properties → Add Property**
2. Fill in details, upload photos
3. Click **Save** — system generates a unique public URL
4. Copy the shareable URL from the **Details page**
5. Click **WhatsApp** to share directly with client

### Client flow:
1. Client opens the URL on their phone — no login needed
2. Views photos, property specs, and agent contact
3. Clicks **Enquire** (quick contact) or **Confirm Deal Interest**
4. Submits confirmation form — receives a reference token
5. Agent reviews in CRM and Approves/Rejects

---

## 🔌 REST API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/leads | All leads (filter: ?stage= or ?q=) |
| POST | /api/leads | Create lead |
| PATCH | /api/leads/{id}/stage | Update stage |
| GET | /api/leads/overdue | Overdue leads |
| GET | /api/customers | All customers |
| GET | /api/dashboard/summary | Dashboard metrics |

---

## 🔧 Connection String

**LocalDB (default — included with Visual Studio):**
```
Server=(localdb)\mssqllocaldb;Database=RealEstateCRM;Trusted_Connection=True
```

**SQL Express:**
```
Server=.\SQLEXPRESS;Database=RealEstateCRM;Trusted_Connection=True
```

Edit in: `CRM.Web/appsettings.json` and `CRM.API/appsettings.json`

---

## 🌱 Seed Data Included
- 3 Agents · 5 Leads across all stages · 1 Customer · 2 Properties

---

## 🛠️ Tech Stack
.NET 8 MVC + Web API · EF Core 8 · LINQ · SQL Server · Bootstrap 5.3 · jQuery 3.7 · Bootstrap Icons · Swagger
