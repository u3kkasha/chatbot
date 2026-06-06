# Project Context: Chatbot

## Overview

A full-stack chatbot development environment featuring a .NET 10 API and a Nuxt 4 frontend.

## Tech Stack

- **Backend:** .NET 10 (C# 14) using `Microsoft.Agents` and `Microsoft.Extensions.AI`.
  - Architecture: **"The Standard" by Hasan Habib** within a **Modular Monolith** structure (Ready-to-Split), evolving towards **Vertical Slice Architecture** within modules.
  - Principles: **DDD**, **FP**, **Reactive Domain Events**, and strict **TDD**.
  - Syntax: **C# 14/13 Excellence** (Primary Constructors, Records, Extension Members, Pattern Matching, Collection Expressions).
  - Package Management: **Central Package Management (CPM)** via `Directory.Packages.props`.
  - Performance: **Native AOT** readiness with **Source Generators** (JSON, DI).
  - API: **OpenAPI** (Scalar) with **ProblemDetails**, **IExceptionHandler**, and **Correlation ID**.
  - Real-time: **SignalR** for state synchronization and **TypedResults.ServerSentEvents** for AI token streaming.
  - AI-Native: **Semantic Kernel Plugin Architecture** (API endpoints as AI Tools).
  - Brokers: **RESTfulSense** for communication; **PII Masking** for compliance; **AI Usage Tracking** for cost control.
  - Models: **Manual Mapping** only to prevent domain leakage.
  - Logic: **Stateless** for session state; **StronglyTypedId** for domain safety; **Distributed Locking** for concurrency.
  - Caching: **HybridCache** (.NET 10) for optimized L1/L2 data access.
  - Resilience: **Polly** and **Testcontainers**.
  - Validation: **FluentValidation**, **GuardClauses**, and **NetArchTest**.
  - Processing: **Coravel** for near-zero config background jobs, queuing, and scheduling.
  - Feature Management: **Microsoft.FeatureManagement** for toggling AI and experimental flows.
  - Logging: **Serilog** with structured sinks.
  - Data: **EF Core 10** with **Native JSON mapping**, **SnakeCase** naming, and **Audit Interceptors**.
  - Time: **NodaTime**.
  - Observability: **OpenTelemetry** and **Health Checks**.
- **Frontend:** Nuxt 4 (TypeScript, Vue 3, TailwindCSS, **Nuxt UI**).
  - State: **Pinia** & **Pinia Colada** (Data Fetching).
  - Patterns: **Idiomatic Nuxt** (Composables, standard directory structure).
  - Validation: **Automated Zod Schema Generation** from backend OpenAPI.
  - Testing: **Playwright** (E2E) and **Vitest** (Unit/Component Testing).
- **Infrastructure:**
  - **PostgreSQL:** Primary relational database (and Hangfire storage/locking).
  - **Qdrant:** Vector database for RAG/AI capabilities.
  - **Azurite:** Local Azure Storage emulator.
  - **Docling-serve:** Document processing service.
  - **Garnet:** High-performance Redis-compatible cache.
  - **Seq:** Log server for diagnostics and structured event visualization.

---

## 📁 Directory Topology

The project is structured to enforce strong logical separation between features before physically splitting them if needed:

- **[api/](file:///home/ukasha/code/chatbot/api)**: Presentation layer hosting the API web server (`Program.cs`, middleware, endpoint controllers, and OpenApi generation).
- **[client/](file:///home/ukasha/code/chatbot/client)**: Nuxt 4 frontend operator workspace.
- **[src/Modules/](file:///home/ukasha/code/chatbot/src/Modules)**: Domain-specific modules enforcing logical Monolith boundaries:
  - **[src/Modules/Identity/](file:///home/ukasha/code/chatbot/src/Modules/Identity)**: User registration, profiles, and operator authentication.
  - **[src/Modules/Chat/](file:///home/ukasha/code/chatbot/src/Modules/Chat)**: Chat session lifecycle, routing, real-time message streaming, and agent-assist suggestion endpoints.
  - **[src/Modules/Knowledge/](file:///home/ukasha/code/chatbot/src/Modules/Knowledge)**: Source file parsing, text chunking, and embedding indexing.
- **[src/Shared/](file:///home/ukasha/code/chatbot/src/Shared)**: Cross-cutting code, shared database interceptors, and reusable Brokers (`Ai`, `AiUsage`, `Blobs`, `DateTimes`, `Logging`, `Pii`, `Processing`, `Vectors`).
- **[tests/](file:///home/ukasha/code/chatbot/tests)**: Testing suites categorizing unit, architecture, and integration tests.

---

## ⚡ Infrastructure Port & URL Map

When local Docker services are running (`tilt up` or `docker compose up`), they bind to the following host endpoints:

| Service                 | Port    | Endpoint                        | Description                                           |
| :---------------------- | :------ | :------------------------------ | :---------------------------------------------------- |
| **Nuxt 4 Client**       | `3000`  | http://localhost:3000           | Operator web interface                                |
| **Seq Log Server**      | `8081`  | http://localhost:8081           | Admin diagnostic dashboard (Log ingestion on `5341`)  |
| **Scalar OpenAPI Docs** | `5136`  | http://localhost:5136/scalar/v1 | Interactive API endpoint schema sandbox               |
| **PostgreSQL**          | `5432`  | localhost:5432                  | Relational datastore (`support_platform` DB)          |
| **Qdrant DB**           | `6333`  | http://localhost:6333           | Vector storage UI/API (gRPC on `6334`)                |
| **Garnet Store**        | `6379`  | localhost:6379                  | Redis-compatible distributed cache and locker         |
| **Docling-serve**       | `5001`  | http://localhost:5001           | Document extraction microservice                      |
| **Azurite Storage**     | `10000` | localhost:10000                 | Blob storage API (Queue on `10001`, Table on `10002`) |
| **Tilt Dashboard**      | `10350` | http://localhost:10350          | Local developer orchestrator dashboard                |

---

## 🔧 Operational Workflow Commands

### Database Migrations

Always add migrations targeting a specific module since the database uses multiple schemas (e.g. `identity` schema):

```bash
snip -- dotnet ef migrations add <MigrationName> --project src/Shared/Chatbot.Shared.csproj --startup-project api/Chatbot.Api.csproj
```

### Running Tests

To run tests:

```bash
snip -- dotnet test
```

### Verification

Run `treefmt` to format the workspace:

```bash
snip -- treefmt
```
