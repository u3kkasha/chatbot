# Tech Stack

## Backend (.NET 10)

- **Language/Runtime:** C# 14, .NET 10 (Native AOT ready).
- **Architecture:** Modular Monolith, Vertical Slice Architecture (VSA).
- **Core Libraries:**
  - `EF Core` (Npgsql) with `NodaTime` support and snake_case naming.
  - `OneOf` for functional result patterns.
  - `RESTFulSense` for RESTful API conventions.
  - `Coravel` for background jobs and scheduling.
  - `MassTransit` for Event-Driven Architecture (EDA).
  - `Microsoft.Extensions.AI` and `Microsoft.Agents.AI` for AI orchestration.
  - `Qdrant.Client` for vector search.
  - `Azure.Storage.Blobs` for object storage.

## Frontend (Nuxt 4)

- **Framework:** Nuxt 4 (Vue 3, TypeScript).
- **Styling:** Tailwind CSS 4, Nuxt UI 4.
- **State Management:** Pinia, Pinia Colada (caching/fetching).
- **AI Integration:** Vercel AI SDK (`ai` package).
- **Validation:** Zod, Valibot, Arktype.
- **Client Generation:** `@hey-api/openapi-ts` for auto-generating the SDK from `openapi.json`.

## Infrastructure & Tooling

- **Environment:** Nix Flakes, Direnv.
- **Orchestration:** Tilt (`Tiltfile`), Docker Compose.
- **Automation:** Just (`Justfile`), Lefthook (git hooks).
- **Formatting:** Treefmt.
- **Package Managers:** Bun (frontend), Dotnet CLI (backend).
