# Omnichannel Customer Support Operator Platform

A high-performance customer support platform built with a decoupled .NET 10 API backend and a Nuxt 4 client.

## 🛠️ Tech Stack Overview

- **Backend:** .NET 10 (C# 14), Microsoft.Extensions.AI, EF Core 10, MassTransit (EDA), Coravel, HybridCache, Scalar OpenAPI.
- **Frontend:** Nuxt 4, Vue 3, TailwindCSS 4, Nuxt UI 4, Pinia & Pinia Colada.
- **Infrastructure:** PostgreSQL 18, Qdrant Vector DB, Azurite (Azure Storage emulator), Docling-Serve (Document parser), Garnet (Cache/Store), Seq (Structured logging).

---

## ⚙️ Quick Start

This project uses **Nix Flakes** and **direnv** to manage a fully reproducible development shell with all required CLI tools.

### 1. Prerequisites

- [Nix Package Manager](https://nixos.org/download) with Flakes enabled.
- [direnv](https://direnv.net/) (optional but recommended to auto-load the environment).
- Docker (or Podman) for managing local services.

### 2. Environment Variables & Secret Management

This project supports secret delivery via **Infisical** (primary) and `.env.local` file (fallback/local overrides).

#### Option A: Infisical (Primary)

Ensure you have the Infisical CLI installed. First, authenticate the CLI by running:

```bash
infisical login
```

Once authenticated, secrets will be automatically exported when entering the directory via `direnv` (if configured), or you can manually export them using:

```bash
eval $(infisical export --format=dotenv-export --silent)
```

#### Option B: Local File Configuration (Fallback)

Copy the example environment file to `.env.local` (which is gitignored):

```bash
cp .env.local.example .env.local
```

Then configure your local secrets (e.g., `OpenAI_API_KEY`) inside `.env.local`.

### 3. Enter Development Shell

If you have `direnv` configured, entering the project directory will automatically load the environment:

```bash
direnv allow
```

Alternatively, enter the shell manually:

```bash
nix develop
```

### 4. Setup and Restore

Once inside the development shell, run the automated setup to install git hooks, restore backend dependencies, and install frontend packages:

```bash
just setup
```

### 5. Run the Platform

We use [Tilt](https://tilt.dev/) to orchestrate the entire development environment (API, Client, and all Infrastructure services):

```bash
tilt up
```

Alternatively, you can run the flake-configured alias:

```bash
start
```

This will launch the Tilt local dashboard at [http://localhost:10350](http://localhost:10350) and manage all services:

- **Client Workspace (Nuxt 4):** [http://localhost:3000](http://localhost:3000)
- **API Swagger / Scalar:** [http://localhost:5136/scalar/v1](http://localhost:5136/scalar/v1)
- **Seq Log Viewer:** [http://localhost:8081](http://localhost:8081)

To shut down the platform, simply exit the Tilt process (Ctrl+C).

---

## 🔧 Essential Development Commands

### Running Tests

Always run the complete test suite before committing or pushing:

```bash
dotnet test
```

### Code Formatting

This project uses `treefmt` to enforce unified formatting rules. It is mandatory to run this before committing:

```bash
treefmt
```

---

## 📝 Project Architecture & Documentation

- [CONTEXT.md](file:///home/ukasha/code/chatbot/main/CONTEXT.md): Directory topology, port mapping, and tech stack details.
- [AGENTS.md](file:///home/ukasha/code/chatbot/main/AGENTS.md): Strict rules and instructions for AI agents.
- [docs/PRD.md](file:///home/ukasha/code/chatbot/main/docs/PRD.md): Product requirement documentation.
- [docs/ARD.md](file:///home/ukasha/code/chatbot/main/docs/ARD.md): Database schema blueprints and system topologies.
- [docs/PLAN.md](file:///home/ukasha/code/chatbot/main/docs/PLAN.md): Modular milestone implementation schedule.
- [docs/GIT_WORKFLOW.md](file:///home/ukasha/code/chatbot/main/docs/GIT_WORKFLOW.md): Branching, PRs, and commit guidelines.
- [docs/adr/](file:///home/ukasha/code/chatbot/main/docs/adr/): Architecture Decision Records (ADRs).
