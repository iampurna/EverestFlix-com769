# EverestFlix

A scalable, cloud-native short-form video-sharing web application built for **COM769 — Scalable Advanced Software Solutions** (MSc Computer Science, Ulster University).

> Status: 🚧 In active development. Currently at end of Phase 1 (solution scaffold).

## Tech stack

| Layer          | Technology                                    |
| -------------- | --------------------------------------------- |
| Frontend       | Blazor WebAssembly (.NET 8)                   |
| Backend API    | ASP.NET Core 8 Web API                        |
| ORM            | Entity Framework Core 8                       |
| Identity       | ASP.NET Core Identity + JWT                   |
| Database       | SQLite (local dev) · Azure SQL (production)   |
| Media storage  | Local filesystem (dev) · Azure Blob (prod)    |
| CI/CD          | GitHub Actions                                |
| Cloud          | Microsoft Azure                               |

## Solution structure

src/
├── EverestFlix.Domain/ Entities, enums, domain constants
├── EverestFlix.Application/ Interfaces, DTOs, service contracts
├── EverestFlix.Infrastructure/ EF Core, Identity, storage implementations
├── EverestFlix.Api/ REST controllers, Program.cs
└── EverestFlix.Client/ Blazor WebAssembly SPA

tests/
├── EverestFlix.UnitTests/
└── EverestFlix.IntegrationTests/


Dependency direction: `Domain <- Application <- Infrastructure <- Api`. `Client` talks to `Api` over HTTP only.

## Local development

Prerequisites: **.NET 8 SDK** (this repo pins `8.0.424` via `global.json`).

```bash
dotnet restore
dotnet build
dotnet test
```

Detailed setup, run instructions, and API documentation will be added as features are implemented.

## Documentation

Additional docs live in `docs/`:

- `docs/architecture/` — architecture diagrams and decision records _(planned)_
- `docs/testing/` — test plan and results _(planned)_
- `docs/screenshots/` — evidence for coursework submission _(planned)_

## Roadmap

| Phase | Description                              | Status         |
| ----- | ---------------------------------------- | -------------- |
| 1     | Solution scaffold                        | ✅ Implemented |
| 2     | Domain model & persistence               | 🔜 Planned     |
| 3     | Authentication & role authorization      | 🔜 Planned     |
| 4     | Video CRUD & storage abstraction         | 🔜 Planned     |
| 5     | Reels UI                                 | 🔜 Planned     |
| 6     | Comments, ratings, creator dashboard     | 🔜 Planned     |
| 7     | Automated testing                        | 🔜 Planned     |
| 8     | Azure deployment                         | 🔜 Planned     |
| 9     | GitHub Actions CI/CD                     | 🔜 Planned     |
| 10    | Advanced feature: Azure AI sentiment     | 🔜 Planned     |

## License

Academic coursework submission — not licensed for redistribution.
