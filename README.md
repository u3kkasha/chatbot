# Omnichannel Customer Support Operator Platform

A high-performance customer support platform built with a decoupled .NET 10 API backend and a Nuxt 4 client.

## 🛠️ Tech Stack Overview

- **Backend:** .NET 10 (C# 14), Microsoft.Extensions.AI, EF Core 10, Coravel, HybridCache, Scalar OpenAPI.
- **Frontend:** Nuxt 4, Vue 3, TailwindCSS, Nuxt UI, Pinia & Pinia Colada.
- **Infrastructure:** PostgreSQL 18, Qdrant Vector DB, Azurite (Azure Storage emulator), Docling-Serve (Document parser), Garnet (Cache/Store), Seq (Structured logging).

---

## ⚙️ Quick Start

This project uses **Nix Flakes** and **direnv** to manage a fully reproducible development shell with all required CLI tools.

### 1. Prerequisites

- [Nix Package Manager](https://nixos.org/download) with Flakes enabled.
- [direnv](https://direnv.net/) (optional but recommended to auto-load the environment).
- Docker (or Podman) for managing local services.

### 2. Enter Development Shell

If you have `direnv` configured, entering the project directory will automatically load the environment:
```bash
direnv allow
```
Alternatively, enter it manually:
```bash
nix develop
```

### 3. Running the Platform

We use [Tilt](https://tilt.dev/) to orchestrate the entire development environment (databases, API, and frontend client):
```bash
tilt up
```
This will launch a local dashboard at [http://localhost:10350](http://localhost:10350) and spin up:
- **Client Workspace:** [http://localhost:3000](http://localhost:3000)
- **API Swagger / Scalar:** [http://localhost:5136/scalar/v1](http://localhost:5136/scalar/v1)
- **Seq Log Viewer:** [http://localhost:8081](http://localhost:8081)

---

## 🔧 Essential Development Commands

### Running Tests
Always run the complete test suite before committing:
```bash
dotnet test
```

### Code Formatting
This project uses `treefmt` to enforce unified formatting rules (Csharpier, Prettier, and Alejandra):
```bash
treefmt
```

---

## 📝 Project Architecture & Documentation

- [CONTEXT.md](file:///home/ukasha/code/chatbot/CONTEXT.md): Directory topology, port mapping, and tech stack details.
- [AGENTS.md](file:///home/ukasha/code/chatbot/AGENTS.md): Strict rules and instructions for AI agents.
- [docs/PRD.md](file:///home/ukasha/code/chatbot/docs/PRD.md): Product requirement documentation.
- [docs/ARD.md](file:///home/ukasha/code/chatbot/docs/docs/ARD.md): Database schema blueprints and system topologies.
- [docs/PLAN.md](file:///home/ukasha/code/chatbot/docs/PLAN.md): Modular milestone implementation schedule.
- [docs/GIT_WORKFLOW.md](file:///home/ukasha/code/chatbot/docs/GIT_WORKFLOW.md): Branching, PRs, and commit guidelines.
- [docs/adr/](file:///home/ukasha/code/chatbot/docs/adr/): Architecture Decision Records (ADRs).
