# Portfolio Tracker

Lightweight ASP.NET Core MVC application to track and report user assets and portfolio performance.

**Tech stack:** ASP.NET Core MVC (Razor views), Entity Framework Core (SQLite), ASP.NET Identity, Bootstrap, jQuery, C#.

**Key features:**
- User registration and authentication
- Create, edit and delete assets
- Dashboard with aggregated portfolio metrics and reports
- Server-side services for crypto pricing and report generation

**Quick start**

Prerequisites: .NET 10 SDK (or the SDK matching the project target).

Run locally:

```bash
dotnet restore
dotnet build
dotnet ef database update            # optional: apply migrations if using EF tools
dotnet run --project PortfolioTracker
```

Configuration: check `PortfolioTracker/appsettings.json` and `appsettings.Development.json` for connection strings and feature toggles.

**Project layout (high level):**
- `PortfolioTracker/Controllers` — MVC controllers
- `PortfolioTracker/Views` — Razor views and UI
- `PortfolioTracker/Models` — domain and view models
- `PortfolioTracker/Data` — EF Core DbContext and migrations
- `PortfolioTracker/Services` — background services (crypto, reporting)

