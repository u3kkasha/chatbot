# ADR 0002: Modular Monolith Architecture

## Status
Accepted

## Context
While "The Standard" defines the internal layering of a service, we also need a way to organize our different business domains (Identity, Chat, Knowledge Base) to prevent the application from becoming a "Big Ball of Mud."

## Decision
We will organize the application as a **Modular Monolith**.

### Rules:
1. **Internal Boundaries:** Each module will live in its own directory/namespace (e.g., `Modules.Identity`).
2. **Encapsulation:** Modules will expose an `IModuleContract` or use internal Domain Events for communication.
3. **Data Isolation:** Each module will ideally own its own tables/schema within the PostgreSQL database.
4. **Ready-to-Split:** The structure must be clean enough that any module can be moved to a separate microservice with minimal refactoring.

### Benefits:
- **Scalability:** We can scale the development team across modules.
- **Future-Proofing:** Simplifies the transition to microservices if/when needed.
- **Clarity:** Clear boundaries make the system easier for humans and AI to reason about.

## Consequences
- Requires careful design of cross-module communication (avoiding direct dependencies).
- Enforced via **NetArchTest** in Phase 1.
