# Agent Mandates

This file provides foundational mandates for AI agents (like Gemini CLI) operating in this repository. These instructions take precedence over general defaults.

## Engineering Standards

- **Formatting:** ALWAYS use `treefmt` (or `nix fmt`) before committing changes. Do not use individual formatters manually.
- **Architecture:** Follow the established pattern of a .NET 10 API and Nuxt 4 Client.
- **Dependencies:**
  - New tools or SDKs should be added to `flake.nix` to ensure environment consistency.
  - **Package Management:** AI agents MUST use terminal commands (e.g., `dotnet add package`, `bun add`) to manage project-level dependencies. DO NOT manually edit `csproj`, `package.json`, or lock files for adding/removing packages.

## Source Control

- **Commit Format:** ALWAYS use the `type(scope): message` format (Conventional Commits).
  - **Types:** `feat`, `fix`, `perf`, `refactor`, `docs`, `chore`, `test`, `style`.
  - **Scope:** The module or component name (e.g., `chat`, `identity`, `api`, `client`).
- **Gitmojis:** Use [gitmojis](https://gitmoji.dev/) in commit messages. **CRITICAL:** Only use abstract or inanimate gitmojis (e.g., 🔧, ⚙️, ⚡, 📝, 📦). NEVER use animate gitmojis (e.g., 🐛, 🚀, 🐳).
- **Workflow:** Adhere to the [Git Workflow Strategy](docs/GIT_WORKFLOW.md).
- **Surgical Edits:** Prefer targeted `replace` calls over rewriting entire files unless necessary.

## Documentation

- Keep `CONTEXT.md` updated as the project's architecture or stack evolves.
- Major architectural changes should be reflected in `ARD.md`.

## Content Guidelines

- **Animate Beings:** NEVER use images, icons, or emojis of animate beings (people, animals, etc., including "bug" emojis) in the codebase, documentation, or commit messages. This is a strict project-wide constraint.
- **Compliant Alternatives:** Use abstract symbols, geometric shapes, or text.
  - **Prohibited:** 🐛 (bug), 🚀 (rocket/animate), 🐳 (docker whale), 👤 (user), 🤖 (robot).
  - **Compliant:** 🔧 (wrench), ⚙️ (gear), ⚡ (bolt), 📝 (memo), 📦 (package), 🔒 (lock).
