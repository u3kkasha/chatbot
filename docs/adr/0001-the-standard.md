# ADR 0001: Adoption of "The Standard" by Hasan Habib

## Status
Accepted

## Context
We need a highly predictable, testable, and scalable architecture for our Omnichannel Support Platform. The team and AI agents need a clear, deterministic structure to ensure consistency across the codebase.

## Decision
We will adopt **"The Standard"** architecture by Hasan Habib for all backend development. 

### Layering Rules:
1. **Brokers:** The only layer allowed to interact with external dependencies (Databases, APIs, Local Filesystem, etc.).
2. **Foundation Services:** Contain the primitive business logic for a single entity. They perform validation and handle exceptions from Brokers.
3. **Processing Services:** Orchestrate multiple foundation services but do not interact with Brokers directly.
4. **Orchestration Services:** Coordinate complex multi-entity workflows.
5. **Controllers/Exposers:** The entry point for the system (REST, SignalR, gRPC).

### Benefits:
- **Testability:** Every layer is isolated and easily mocked.
- **Predictability:** AI agents can generate code with high accuracy due to the strict patterns.
- **Maintainability:** Clear separation of concerns reduces cognitive load.

## Consequences
- Requires more initial boilerplate (Brokers, Foundation Services).
- Strict adherence is mandatory; any deviations must be documented in a new ADR.
