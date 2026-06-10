# Task Completion Checklist

A coding task is considered "Done" only when:

1. **Implementation:**
   - Feature/fix is fully implemented following `mem:conventions`.
   - Backend logic adheres to "The Standard" (Brokers/Services separation).
   - Frontend components use Nuxt UI and are type-safe.

2. **Verification:**
   - **Backend:** All tests pass via `snip -- dotnet test`.
   - **Frontend:**
     - Linting passes via `cd client && bun run lint`.
     - Type checking passes via `cd client && bun run typecheck`.
     - Tests pass via `cd client && bun test`.
   - **Schema:** If API changed, `bun run openapi:generate` was executed and frontend updated.

3. **Code Quality:**
   - `snip -- treefmt` executed to format all changed files.
   - No sensitive data (secrets, keys) committed.
   - Conventional commit message drafted.

4. **Environment:**
   - Changes verified in a running Tilt environment (`tilt up`).
