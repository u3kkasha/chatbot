# Engineering Conventions

## General

- **Commits:** Follow Conventional Commits.
- **Formatting:** Enforced by `treefmt`. Always run before committing.
- **CLI Proxy:** Prefix shell commands with `snip --` to optimize token usage.

## Backend (C#)

- **"The Standard" (Hasan Habib):**
  - **Brokers:** Thin wrappers for external dependencies (Storage, Time, AI). No logic.
  - **Services:** Business logic, validation, exception mapping. Strictly TDD.
  - **Exposers:** Controllers/Endpoints. Simple delegation to services.
- **VSA:** Keep features self-contained in `Features/` folders.
- **Functional Results:** Use `OneOf<TSuccess, TError>` for service return types.
- **Type Safety:** Use Vogen/StronglyTypedId for IDs.
- **Async:** Prefer `ValueTask<T>` for high-performance paths.

## Frontend (Vue/Nuxt)

- **Nuxt 4:** Adhere to the new directory structure and conventions.
- **Components:** Prefer Nuxt UI primitives.
- **State:** Use Pinia Colada for server-state (fetching/caching) and Pinia for client-state.
- **Schema:** Use auto-generated types from the OpenAPI client.
