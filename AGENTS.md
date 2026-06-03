# Agent Mandates

This file provides foundational mandates for AI agents (like Gemini CLI) operating in this repository. These instructions take precedence over general defaults.

## Engineering Standards

- **Formatting:** ALWAYS use `treefmt` (or `nix fmt`) before committing changes. Do not use individual formatters manually.
- **Architecture:** Follow the established pattern of a .NET 10 API and Nuxt 4 Client.
- **Dependencies:** New tools or SDKs should be added to `flake.nix` to ensure environment consistency.

## Source Control

- **Gitmojis:** Use [gitmojis](https://gitmoji.dev/) in commit messages to categorize changes.
- **Surgical Edits:** Prefer targeted `replace` calls over rewriting entire files unless necessary.

## Documentation

- Keep `CONTEXT.md` updated as the project's architecture or stack evolves.
- Major architectural changes should be reflected in `ARD.md`.
