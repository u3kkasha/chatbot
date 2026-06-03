# Project Context: Chatbot

## Overview

A full-stack chatbot development environment featuring a .NET 10 API and a Nuxt 4 frontend.

## Tech Stack

- **Backend:** .NET 10 (C#) using `Microsoft.Agents` and `Microsoft.Extensions.AI`.
- **Frontend:** Nuxt 4 (TypeScript, Vue 3, TailwindCSS).
- **Infrastructure:**
  - **PostgreSQL:** Primary relational database.
  - **Qdrant:** Vector database for RAG/AI capabilities.
  - **Azurite:** Local Azure Storage emulator.
  - **Docling-serve:** Document processing service.

## Development Workflow

- **Environment:** Nix Flakes + Direnv for a reproducible development shell.
- **Orchestration:** [Tilt](https://tilt.dev/) for managing microservices locally.
- **Formatting:** Unified formatting via `treefmt`.
  - `csharpier` for C#.
  - `prettier` for TypeScript, Vue, JSON, Markdown, and YAML.
  - `alejandra` for Nix.
