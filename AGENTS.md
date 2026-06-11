# Agent Mandates

This file provides foundational mandates for AI agents (like Gemini CLI) operating in this repository. These instructions take precedence over general defaults.

## ⚙️ Environment & CLI Execution

- **Sandboxing Constraints:** If standard sandbox commands fail due to host/Nix/bubblewrap dependencies, configure tools with `BypassSandbox: true` and execute the command.
- **Snip Token Optimizer:** Prefix all terminal commands with `snip --` (e.g., `snip -- dotnet test`, `snip -- just format`) to minimize token consumption.
- **Formatting:** ALWAYS run `just format` to format files before committing. Do not run individual formatters manually.

---

## 🔧 Engineering & Coding Standards

- **Architecture:** Follow the Modular Monolith pattern (backend .NET 10 API, frontend Nuxt 4 Client) evolving towards Vertical Slice Architecture (VSA) inside feature folders.
- **"The Standard" Architecture Rules (Hasan Habib):**
  - **Brokers:** Act as lightweight, thin wrappers around external SDKs, DBs, or APIs. They must implement a local interface (e.g., `IStorageBroker`), carry no business logic, and throw no custom exceptions.
  - **Services:** Implement business logic. They must perform two validation levels (Structural and Logical), catch all broker exceptions, and wrap them in localized exceptions (e.g., `ValidationException`, `DependencyValidationException`, `DependencyException`, `ServiceException`).
  - **TDD Discipline:** Services must be developed via TDD. Write failing tests (Validation, Logic, Exceptions) before writing the service implementation.
  - **Asynchrony:** Use `ValueTask` or `ValueTask<T>` for async operations to optimize allocations.
- **Modern C# 14 / C# 13 Conventions:**
  - Use records, primary constructors, expression-bodied members, collection expressions (`[...]`), and Vogen-generated value objects.
  - Strongly-typed IDs (`Vogen`) must be mapped in EF Core configurations using `.HasConversion(...)` in `OnModelCreating`.
- **Dependencies:**
  - Do not edit `.csproj` or `package.json` manually to manage package dependencies.
  - Backend: Use `snip -- dotnet add package <PackageName>`
  - Frontend: Use `snip -- bun add <PackageName>`
- **Configuration & Options (AOT & Test Compatibility):**
  - Do not read from `IConfiguration` eagerly to validate or throw exceptions during service registration (startup).
  - Use the Options pattern (`IOptions<T>`) combined with source-generated validators (`[OptionsValidator]`).
  - Keep configuration processing and instance instantiation lazy by using DI factory lambdas (e.g., `services.AddSingleton<TService>(sp => ...)`) or implementing `IConfigureOptions<TOptions>`, preventing startup validation/connection crashes during OpenAPI generation or integration tests where services are registered but never resolved.

---

## 📦 Source Control & Git Rules

- **Commit Format:** ALWAYS use the `type(scope): message` format (Conventional Commits).
  - **Types:** `feat`, `fix`, `perf`, `refactor`, `docs`, `chore`, `test`, `style`.
  - **Scope:** The module or component name (e.g., `chat`, `identity`, `api`, `client`).
- **Gitmojis:** Use abstract or inanimate gitmojis in commit messages (e.g., 🔧, ⚙️, ⚡, 📝, 📦, 🔒).
  - **CRITICAL:** **NEVER** use animate gitmojis (e.g., 🐛, 🚀, 🐳, 👤, 🤖) in commits, code, or documentation.
- **Workflow:** Adhere to the [Git Workflow Strategy](docs/GIT_WORKFLOW.md).
- **Surgical Edits:** Prefer targeted replacing over rewriting entire files unless necessary.

---

## 📝 Documentation & Links

- Keep [CONTEXT.md](file:///home/ukasha/code/chatbot/CONTEXT.md) updated as ports, topologies, or technologies change.
- Major architectural changes must be reflected in [ARD.md](file:///home/ukasha/code/chatbot/docs/ARD.md).
- When linking files in markdown, use the absolute file link syntax without backticks (e.g., `[README.md](file:///home/ukasha/code/chatbot/README.md)`).

---

## 🔒 Content Guidelines & Safety

- **Animate Beings Restriction:** **NEVER** use images, icons, or emojis of animate beings (people, animals, bugs, robots, etc.) in the codebase, documentation, or commit messages.
- **Compliant Alternatives:** Use abstract symbols, geometric shapes, or plain text.
  - **Prohibited:** 🐛 (bug), 🚀 (rocket), 🐳 (docker), 👤 (user), 🤖 (robot).
  - **Compliant:** 🔧 (wrench), ⚙️ (gear), ⚡ (bolt), 📝 (memo), 📦 (package), 🔒 (lock).
