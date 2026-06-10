# Suggested Commands

## Development Workflow

- `just setup` — Full first-time setup (hooks, backend/frontend dependencies).
- `tilt up` — Start the integrated development environment (API, Client, and all Infra services).
- `snip -- treefmt` — Format all files in the repository.

## Backend (dotnet)

- `snip -- dotnet restore` — Restore NuGet packages.
- `snip -- dotnet build` — Build the solution.
- `snip -- dotnet test` — Run all backend tests (Unit, Integration, Architecture).
- `snip -- dotnet add package <Package>` — Add a dependency (prefer CLI over manual editing).

## Frontend (bun)

- `cd client && bun install` — Install dependencies.
- `cd client && bun run dev` — Start Nuxt dev server (if not using Tilt).
- `cd client && bun run openapi:generate` — Update the API client from `openapi.json`.
- `cd client && bun run lint` — Run ESLint.
- `cd client && bun run typecheck` — Run TypeScript type checking.
- `cd client && bun test` — Run Vitest tests.
- `cd client && bun add <Package>` — Add a dependency (prefer CLI over manual editing).
