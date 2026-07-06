# SiteManagerKenya — Refactored Paytruck (Production-ready)

## Project Overview

SiteManagerKenya is a refactored, production-focused implementation of the Paytruck application. The codebase provides a layered architecture (Infrastructure, Data, Application, Presentation) with an emphasis on performance, maintainability, and testability.

This repository includes the application source, automated tests, database migration scripts, and implementation guidance in the Documentation folder.

## Key Capabilities

- Layered architecture following SOLID principles
- Dependency injection and centralized configuration
- Generic repository pattern and unit-of-work style data access
- Optimized payroll calculation and time-tracking model
- Asynchronous database and I/O operations for responsive UI
- Comprehensive unit and integration test coverage

## Repository Structure

Top-level layout:

```
RefactoredPaytruck/
├── SiteManagerKenya/                # Application source (WPF / .NET)
├── SiteManagerKenya.Tests/          # Unit and integration tests
├── Documentation/                   # Architecture, setup, migration guides
├── SiteManagerKenya.sln             # Visual Studio solution
├── build.bat / build.ps1            # Build helpers for Windows
├── run.bat / run.ps1                # Run helpers for Windows
└── README.md
```

See the Documentation folder for detailed design decisions and migration steps.

## Prerequisites

- .NET SDK 8.0 or compatible (install via dotnet.microsoft.com)
- Visual Studio 2022/2023 or `dotnet` CLI for build and test
- SQLite tools (if applying the included migration script)

On Windows, you can verify the SDK with:

```powershell
dotnet --info
```

## Quick Start — Build and Run (Windows)

1. Open a Developer PowerShell or command prompt in the repository root.
2. Restore and build the solution:

```powershell
dotnet restore SiteManagerKenya.sln
dotnet build SiteManagerKenya.sln -c Release
```

3. Run the application (WPF) from Visual Studio or using `dotnet` if an entry project is available:

```powershell
dotnet run --project SiteManagerKenya\SiteManagerKenya.csproj
```

Alternatively, use the provided scripts on Windows:

```powershell
.\build.bat
.\run.bat
```

## Tests

Run unit and integration tests with the `dotnet` CLI:

```powershell
dotnet test SiteManagerKenya.Tests\SiteManagerKenya.Tests.csproj -c Release
```

## Database Migration

The repository contains `Documentation/DATABASE_MIGRATION.sql` for migrating legacy attendance/clock tables into the unified TimeRecords model. Review the migration plan in `Documentation/MIGRATION_ROADMAP.md` before applying scripts. Execute migrations in a controlled environment and create a backup before running any migration.

## Architecture Summary

- Infrastructure: Configuration, logging, connection pooling, and database initialization.
- Data: Entities, repositories, and data access helpers.
- Application: Business services (payroll, time tracking, workers).
- Presentation: WPF views and ViewModels implementing MVVM patterns.

Design goals: performance, clear separation of concerns, testability, and minimal external side effects.

## Contributing

Contributions are welcome. Please follow these guidelines:

1. Open an issue describing the change or bug.
2. Create a feature branch from `main` or `master`.
3. Include unit tests for new behavior.
4. Submit a pull request with a clear description and rationale.

## Documentation

Primary documentation lives in the `Documentation/` folder. Key files:

- `Documentation/SETUP_INSTRUCTIONS.md` — Step-by-step setup and environment details
- `Documentation/ARCHITECTURE_REVIEW.md` — Rationale and architectural trade-offs
- `Documentation/MIGRATION_ROADMAP.md` — Data migration plan and phases

## License and Use

Check the repository owner for licensing details. If no explicit license file is present, treat the code as subject to the owner's terms and request permission for production use.

## Contact and Support

For questions about the implementation or migration process, consult the Documentation or open an issue in this repository.

---

Updated README for clarity and alignment with the repository contents.

