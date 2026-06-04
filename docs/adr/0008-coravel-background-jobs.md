# ADR 0008: Reliable Background Processing with Coravel

## Status

Accepted

## Context

Critical tasks like document ingestion (Docling) and AI indexing are long-running and must be reliable. We need a way to offload these tasks from the main request thread to improve user experience and ensure system stability.

## Decision

We will use **Coravel** for all asynchronous background jobs, queuing, and task scheduling.

### Implementation:

1. **Queuing:** Coravel's in-memory queue will be used for offloading ingestion tasks.
2. **Scheduling:** Coravel's scheduler will handle recurring maintenance and cleanup tasks.
3. **Invokables:** Features will be encapsulated into `IInvokable` classes to keep the API controllers thin.
4. **Events:** Coravel's event dispatcher will be used for decoupled communication between modules.

### Benefits:

- **Near-Zero Config:** Integrates seamlessly with .NET 10 without requiring a separate database schema for storage by default.
- **Fluent Syntax:** Highly readable and expressive scheduling and queuing logic.
- **Lightweight:** Minimal overhead compared to Hangfire, aligning with our Native AOT goals.
- **Native Integration:** Built specifically for .NET Core, following modern DI and middleware patterns.

## Consequences

- Replaces the dependency on Hangfire.
- Background jobs are in-memory by default; if persistence is required in the future, we will evaluate Coravel's Pro version or a custom database store, but for the current MVP, in-memory is sufficient given our Modular Monolith focus.
