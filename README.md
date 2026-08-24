# EverestFlix

EverestFlix is a scalable short-form video-sharing web application developed for **COM769 — Scalable Advanced Software Solutions**.

The system follows a separated client/API architecture and is designed for cloud deployment on Microsoft Azure.

## Current Status

✅ Core local application complete  
✅ Authentication and role-based authorization complete  
✅ Creator video upload and management complete  
✅ Consumer interaction features complete  
✅ Automated unit and API integration testing complete  
🔜 Azure deployment and cloud-service integration next

---

## Core Features

### Consumer

- Register and log in
- Browse latest short-form videos
- Search videos
- Play videos through the reels interface
- View video metadata
- Post comments
- Rate videos from 1–5
- View rating summaries
- View personal profile

### Creator

Creators have all standard authenticated functionality plus:

- Creator dashboard
- MP4 video upload
- Required metadata:
  - Title
  - Publisher
  - Producer
  - Genre
  - Age rating
- Edit owned videos
- Delete owned videos
- View uploaded-video statistics

Public registration creates **Consumer** accounts only.

Creator accounts are provisioned separately rather than through public registration.

### Administration

- Administrator role supported through ASP.NET Core Identity
- Role-based authorization
- Administrative identity separated from Creator functionality

---

## Technology Stack

| Layer | Technology |
| --- | --- |
| Frontend | Blazor WebAssembly (.NET 8) |
| Backend | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Authentication | ASP.NET Core Identity + JWT |
| Local database | SQLite |
| Local media storage | Filesystem storage |
| Testing | xUnit + ASP.NET Core integration testing |
| Production database | Azure SQL planned |
| Production media | Azure Blob Storage planned |
| Hosting | Microsoft Azure planned |
| CI/CD | GitHub Actions planned |

---

## Architecture

```text
Blazor WebAssembly Client
          |
          | HTTPS / REST
          v
ASP.NET Core Web API
          |
          +------------------+
          |                  |
          v                  v
 Entity Framework       Video Storage
          |                  |
          v                  v
       SQLite            Local Files
     (local dev)         (local dev)
     Internet
   |
   v
Azure-hosted Blazor Client
   |
   v
ASP.NET Core API
   |
   +--------------------+
   |                    |
   v                    v
Azure SQL         Azure Blob Storage

EverestFlix/
├── src/
│   ├── EverestFlix.Client/
│   ├── EverestFlix.Api/
│   ├── EverestFlix.Application/
│   ├── EverestFlix.Domain/
│   └── EverestFlix.Infrastructure/
│
├── tests/
│   ├── EverestFlix.UnitTests/
│   └── EverestFlix.IntegrationTests/
│
├── docs/
├── .github/
├── EverestFlix.sln
└── README.md

Project Responsibilities
EverestFlix.Domain — entities, enums and domain constants
EverestFlix.Application — DTOs, interfaces and application contracts
EverestFlix.Infrastructure — Entity Framework Core, Identity, storage and service implementations
EverestFlix.Api — REST API, authentication pipeline and controllers
EverestFlix.Client — Blazor WebAssembly user interface
The client communicates with the backend only through HTTP REST endpoints.