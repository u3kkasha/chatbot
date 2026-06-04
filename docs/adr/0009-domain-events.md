# ADR 0009: Decoupled Logic using Domain Events

## Status

Accepted

## Context

As the platform grows, Orchestration services can become bloated if they have to handle every side-effect of an action (e.g., saving a message, updating SignalR, triggering AI, sending an email).

## Decision

We will implement an internal **Reactive Domain Event Broker**.

### Implementation:

1. **Events:** Services will publish lightweight C# records representing domain changes (e.g., `MessageReceivedEvent`).
2. **Handlers:** Specialized, decoupled handlers will subscribe to these events to perform side-effects.
3. **In-Process:** Initially, events will be processed in-process (Mediator-style or simple internal observer).
4. **Reliability:** For cross-module side-effects, we will promote events to **Hangfire** jobs for "Outbox Pattern" style reliability.

### Benefits:

- **Decoupling:** Orchestrators only care about the primary action.
- **Maintainability:** Side-effects are isolated in their own handlers.
- **Scalability:** Easy to transition to a distributed message bus (RabbitMQ/ServiceBus) if the system grows.

## Consequences

- Requires careful tracing of event flows (mitigated by OpenTelemetry).
- Can make the execution path less "obvious" (mitigated by Mermaid diagrams).
