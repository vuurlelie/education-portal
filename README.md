![.NET CI](https://github.com/vuurlelie/EducationPortal/actions/workflows/dotnet-ci.yml)

# EducationPortal

An ASP.NET Core 9 MVC education portal to help users upskill through courses composed of mixed materials (videos, ebooks, articles). Progress is tracked per material across courses, and completing a course awards skills (incrementing level if already owned).

**Note:** This is a learning project, not a production-ready application.  
I created it to practice C#/.NET backend development (ASP.NET Core MVC, EF Core, Identity, testing, etc.)  
The goal was to build a structured, layered solution and learn the fundamentals of real-world backend development.

## Tech Stack

- **.NET:** ASP.NET Core 9 MVC, C# 12  
- **Data:** Entity Framework Core 9 (Code First, Fluent API, TPT inheritance)  
- **Auth:** ASP.NET Core Identity (EF stores)  
- **DB:** SQL Server (LocalDB by default; Docker SQL supported)  
- **Tests:** xUnit, Moq  
- **Tooling:** Local `dotnet-ef` (tool manifest), SQL Server via Docker (optional)

## Solution Layout

```text
├─ src/
│  ├─ EducationPortal.Presentation        # ASP.NET Core MVC (startup)
│  ├─ EducationPortal.BusinessLogic       # Services, application logic
│  ├─ EducationPortal.DataAccess          # EF Core DbContext, entities, migrations
│  └─ tests/
│     └─ EducationPortal.BusinessLogic.UnitTests
├─ scripts/
│  ├─ Add-Migration.ps1                   # Helper: add EF migration
│  └─ Update-Database.ps1                 # Helper: apply migrations
├─ docs/                                  # (ERD, diagrams, etc.)
├─ Directory.Build.props                  # shared build settings
├─ Directory.Packages.props               # central NuGet versions
└─ .config/dotnet-tools.json              # local dotnet-ef tool manifest
```

## Features (MVP)

- Courses with materials (Video / Book / Article) and completion-awarded skills  
- TPT inheritance for materials (shared base `Material`)  
- Cross-course progress propagation when a shared material is completed  
- Identity-based authentication (register, login)  
- Profiles: list courses (Available / In Progress / Completed) and user skills w/ levels

## Prerequisites

- **.NET SDK 9.x**
- **SQL Server** (choose one):
  - LocalDB (default; Windows)
  - Docker for SQL Server (cross-platform)
- **PowerShell 7+** (recommended for helper scripts on Windows)

## First-Time Setup

### 1) Trust HTTPS dev cert (one-time)
```bash
dotnet dev-certs https --trust
```

### 2) Restore local developer tools (EF Core CLI)
```bash
dotnet tool restore
```

### 3) Restore and build
```bash
dotnet restore
dotnet build
```

### 4) Configure the database connection
#### Default (no secrets needed): LocalDB
`src/EducationPortal.Presentation/appsettings.json` contains a non-secret LocalDB connection string

#### Alternative: Dockerized SQL Server (no local install required)
Run a container:
```bash
docker run --env "ACCEPT_EULA=Y" --env "SA_PASSWORD=<your-strong-password>" \
  -p 1433:1433 --name sql_server_container -d mcr.microsoft.com/mssql/server:2022-latest
```

### 5) Create / Update the database
#### Option A – One-liner scripts (recommended)
```powershell
# apply latest migrations
pwsh ./scripts/Update-Database.ps1

# add a new migration, then apply
pwsh ./scripts/Add-Migration.ps1 -Name "AddSomething"
pwsh ./scripts/Update-Database.ps1
```

#### Option B – Raw EF CLI
```bash
dotnet ef migrations add InitialCreate \
  -c AppDbContext \
  -p ./src/EducationPortal.DataAccess/EducationPortal.DataAccess.csproj \
  -s ./src/EducationPortal.Presentation/EducationPortal.Presentation.csproj

dotnet ef database update \
  -p ./src/EducationPortal.DataAccess/EducationPortal.DataAccess.csproj \
  -s ./src/EducationPortal.Presentation/EducationPortal.Presentation.csproj
```

## Running the App
```bash
dotnet run --project src/EducationPortal.Presentation
```

## Architecture Overview
- **Presentation (MVC):** Controllers, view models, Razor views/partials
- **Business Logic:** Application services, DTOs/mappers, validation rules
- **Data Access:** EF Core AppDbContext, Fluent API configurations, TPT for materials, repositories, Unit of Work

**Identity:** `ApplicationUser : IdentityUser<Guid>`; Identity EF stores on the same DbContext

### Entity highlights
- `Material` (base) → `VideoMaterial`, `BookMaterial`, `ArticleMaterial` (TPT)
- Many-to-many junctions with composite PKs (`CourseMaterial`, `CourseSkill`, `UserCourse`, `UserMaterial`, `UserSkill`)
- Delete behaviors and constraints set in Fluent API (e.g., `UserCourse.ProgressPercent` 0–100 check)

## Development Scripts
- `scripts/Add-Migration.ps1 -Name "<Name>"`
Adds a migration using the local EF tool and correct project paths.
- `scripts/Update-Database.ps1`
Applies migrations to the configured DB. Use `-Migration "<Name>"` or `-Migration 0` to roll back.

*All scripts automatically run `dotnet tool restore`.*

## Testing
```bash
dotnet test tests/EducationPortal.BusinessLogic.UnitTests --collect:"XPlat Code Coverage"
```

### Packages:
- `xunit`, `xunit.runner.visualstudio`, `xunit.analyzers`
- `Moq`
- `coverlet.collector`

## Common Commands (cheatsheet)
```bash
# trust HTTPS cert (once)
dotnet dev-certs https --trust

# local tools
dotnet tool restore
dotnet ef --version

# EF (raw)
dotnet ef migrations add <Name> -c AppDbContext \
  -p ./src/EducationPortal.DataAccess/EducationPortal.DataAccess.csproj \
  -s ./src/EducationPortal.Presentation/EducationPortal.Presentation.csproj

dotnet ef database update -p ./src/EducationPortal.DataAccess/EducationPortal.DataAccess.csproj \
  -s ./src/EducationPortal.Presentation/EducationPortal.Presentation.csproj
```
