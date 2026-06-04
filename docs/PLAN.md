# Implementation Plan: Omnichannel Customer Support Operator Platform

## Phase 0: Project Governance & Scaffolding

- [x] **ADR Initialization:**
  - Establish records for "The Standard", OneOf, NodaTime, Verify, Coravel, Native AOT, Domain Events, and PII Masking.
  - **Modular Monolith ADR:** Document the module boundaries and internal communication strategy.
  - [x] **Vertical Slice Architecture (VSA) ADR:** Document the strategy for grouping features (Logic, DTOs, Endpoints) by folder within modules.
- [x] **Infrastructure Scaffolding:**
  - Set up **Scalar**, **Vitest**, **Testcontainers**, **Verify**, **Husky.Net**, and **Tailwind CSS Sorting**.
  - Initialize the **Modular Monolith** project structure (e.g., `Modules/Identity`, `Modules/Chat`).
- [x] **Central Package Management (CPM):**
  - [x] Initialize `Directory.Packages.props` and migrate all solution projects to central versioning.
- [x] **Core Cross-Cutting Concerns:**
  - Implement **IExceptionHandler**, **Correlation ID** middleware, and **Coravel**.
  - Initialize **Living Documentation** standards using **Mermaid.js**.

## Phase 1: Foundation, Infrastructure & Brokers

- [x] **"The Standard" Brokers:**
  - Implement Brokers (Storage, Vector, Blob, AI, Processing).
  - **RESTfulSense:** For all REST-based Brokers.
  - **PII Masking Broker:** Implement automated PII scrubbing.
  - **AI Usage Broker:** Implement token tracking and logging.
  - **Resilience:** Integrate **Polly** policies.
- [ ] **Native AOT & Performance Scaffolding:**
  - Configure **System.Text.Json** and **DI Source Generators**.
  - Initialize **HybridCache** for high-performance L1/L2 data access.
- [ ] **DDD & Data Layer:**
  - Define Entities and Value Objects (using **Records**, **Primary Constructors**, **Extension Members**, **StronglyTypedId**, and **Vogen**).
  - Configure EF Core 10 with **SnakeCase** naming, **Audit Interceptors**, and **Native JSON Mapping** for citations and AI metadata.
- [ ] **Real-time Streaming Foundation:**
  - [ ] Scaffold **TypedResults.ServerSentEvents** support for lightweight, unidirectional AI token streaming.
- [ ] **Secret Management:**
  - Implement **VarLock** for secure local secret management.
- [ ] **Startup Configuration Validation:**
  - Implement validated Options patterns for environment safety.
- [ ] **OpenAPI, Observability & Error Handling:**
  - Define initial **DTOs** and **OpenAPI** configuration.
  - Implement **ProblemDetails**, **OpenTelemetry**, and **Health Checks**.
- [ ] **Architecture Testing:**
  - Implement **NetArchTest** suite (enforcing "Manual Mapping Only" and "Module Isolation" rules).
- [ ] **Automated Type & Schema Generation:**
  - Auto-generate **TypeScript Types** AND **Zod Schemas** from OpenAPI JSON.

## Phase 2: Foundation & Processing Services

- [ ] **Foundation Services (TDD):**
  - Use **NSubstitute**, **OneOf**, and **Verify**.
  - Apply **Modern C# Pattern Matching** for result handling.
- [ ] **Session State Machine:**
  - Implement **Stateless** configuration for status transitions.

## Phase 3: Orchestration & Domain Events

- [ ] **Reactive Domain Event Broker:**
  - Implement decoupled system behavior using events.
- [ ] **Concurrency & Safety:**
  - Implement **Distributed Locking** in orchestration services.
- [ ] **Orchestration Services:**
  - Coordinate multi-broker/multi-service flows.
- [ ] **SignalR Hubs:**
  - Real-time communication grouped by Tenant ID for state synchronization.
- [ ] **Background Processing (Coravel):**
  - Refactor ingestion flows to use **Coravel** jobs.

## Phase 4: AI & Knowledge Base Orchestration

- [ ] **AI Orchestration (Semantic Kernel):**
  - RAG pipeline (Embed -> Search -> Prompt -> Complete).
  - **AI Plugins:** Design API endpoints as **AI Tools** for Semantic Kernel.
  - **Feature Toggles:** Wrap AI flows in **Microsoft.FeatureManagement**.
- [ ] **AI Streaming Implementation:**
  - [ ] Use **TypedResults.ServerSentEvents** to stream LLM completion tokens to the client.
- [ ] **Ingestion Orchestration:**
  - Full ingestion flow (Upload -> Parse -> Chunk -> Embed -> Index).

## Phase 5: Frontend Refactoring (Nuxt 4 Operator Workspace)

- [ ] **Idiomatic Nuxt Refactoring:**
  - Implement **Composables** and **Pinia Colada** using generated types/schemas.
- [ ] **Unified Inbox UI (Nuxt UI):**
  - 3-pane layout using **Nuxt UI** components.
  - Integrate `@microsoft/signalr` into the Nuxt client for state updates.
  - Implement SSE listeners for AI response streaming.

## Phase 6: End-to-End Testing & Polish

- [ ] **Playwright E2E:**
  - Verify critical flows.
- [ ] **Security Validation:**
  - Integration tests for Tenant isolation and PII masking verification.
