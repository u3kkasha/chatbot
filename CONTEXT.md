# Project Context: Chatbot

## Overview

A full-stack chatbot development environment featuring a .NET 10 API and a Nuxt 4 frontend.

## Tech Stack

- **Backend:** .NET 10 (C# 14) using `Microsoft.Agents` and `Microsoft.Extensions.AI`.
  - Architecture: **"The Standard" by Hasan Habib** within a **Modular Monolith** structure (Ready-to-Split).
  - Principles: **DDD**, **FP**, **Reactive Domain Events**, and strict **TDD**.
  - Syntax: **C# 14/13 Excellence** (Primary Constructors, Records, Extension Members, Pattern Matching, Collection Expressions).
  - Performance: **Native AOT** readiness with **Source Generators** (JSON, DI).
  - API: **OpenAPI** (Scalar) with **ProblemDetails**, **IExceptionHandler**, and **Correlation ID**.
  - AI-Native: **Semantic Kernel Plugin Architecture** (API endpoints as AI Tools).
  - Brokers: **RESTfulSense** for communication; **PII Masking** for compliance; **AI Usage Tracking** for cost control.
  - Models: **Manual Mapping** only to prevent domain leakage.
  - Logic: **Stateless** for session state; **StronglyTypedId** for domain safety; **Distributed Locking** for concurrency.
  - Caching: **HybridCache** (.NET 10) for optimized L1/L2 data access.
  - Resilience: **Polly** and **Testcontainers**.
  - Validation: **FluentValidation**, **GuardClauses**, and **NetArchTest**.
  - Processing: **Hangfire** for reliable background ingestion jobs.
  - Feature Management: **Microsoft.FeatureManagement** for toggling AI and experimental flows.
  - Logging: **Serilog** with structured sinks.
  - Data: **EF Core** with **SnakeCase** naming and **Audit Interceptors**.
  - Time: **NodaTime**.
  - Observability: **OpenTelemetry** and **Health Checks**.
- **Frontend:** Nuxt 4 (TypeScript, Vue 3, TailwindCSS, **Nuxt UI**).
  - State: **Pinia** & **Pinia Colada** (Data Fetching).
  - Patterns: **Idiomatic Nuxt** (Composables, standard directory structure).
  - Validation: **Automated Zod Schema Generation** from backend OpenAPI.
  - Testing: **Playwright** (E2E) and **Vitest** (Unit/Component Testing).
- **Infrastructure:**
  - **PostgreSQL:** Primary relational database (and Hangfire storage/locking).
  - **Qdrant:** Vector database for RAG/AI capabilities.
  - **Azurite:** Local Azure Storage emulator.
  - **Docling-serve:** Document processing service.

## Development Workflow

- **Environment:** Nix Flakes + Direnv for a reproducible development shell.
- **Methodology:** **TDD** following **"The Standard"** patterns.
  - Tests: **xUnit**, **FluentAssertions**, **NSubstitute**, and **Verify** (Snapshot Testing).
  - Patterns: **FIRST**, **AAA**, and **Test Pyramid Strategy**.
- **Governance:** **Architecture Decision Records (ADRs)** and **Architecture Tests**.
- **Commits:** **Conventional Commits** with **Gitmojis** enforced via **Husky.Net** hooks.
- **Orchestration:** [Tilt](https://tilt.dev/) for managing microservices locally.
- **Formatting:** Unified formatting via **treefmt** (Csharpier, Prettier with **Tailwind Plugin**, Alejandra).
- **Fakes:** **Bogus** for generating realistic test data.
- **Documentation:** **Living Documentation** using **Mermaid.js** diagrams in markdown.

## Documentation

- **[Product Requirement Document (PRD)](docs/PRD.md):** Defines the core goals, user personas, and functional requirements.
- **[Architectural Reference Document (ARD)](docs/ARD.md):** Detailed system topology, multi-tenancy strategy, and database schema.
- **[Implementation Plan (PLAN)](docs/PLAN.md):** Roadmap for transitioning from mock implementation to full omnichannel support.
