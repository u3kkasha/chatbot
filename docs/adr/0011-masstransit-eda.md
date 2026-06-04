# ADR 0011: Event-Driven Architecture with MassTransit

## Status

Accepted

## Context

As the platform transitions to a Modular Monolith with a "Ready-to-Split" mandate (ADR 0002), we need a robust, standardized way to handle Domain Events and inter-module communication. While Coravel handles scheduling, it lacks advanced EDA features like a Transactional Outbox, easy transport switching, and complex Pub/Sub patterns.

## Decision

We will use **MassTransit** as our primary message bus and EDA framework.

### Implementation:

1. **In-Memory Transport:** Initially, we will use the in-memory transport to minimize infrastructure complexity during the MVP phase.
2. **Transactional Outbox:** We will integrate MassTransit with EF Core 10 to implement the Outbox pattern, ensuring events are only published if the database transaction succeeds.
3. **Domain Events:** All cross-module and side-effect communication will use MassTransit `Publish/Subscribe` patterns.
4. **Resilience:** We will utilize MassTransit's built-in retry, redelivery, and dead-letter queueing features.

### Benefits:

- **Transport Portability:** Allows switching from in-memory to RabbitMQ, Azure Service Bus, or Amazon SQS with minimal configuration changes.
- **Reliability:** The Transactional Outbox prevents "ghost events" (events sent when a DB transaction fails).
- **Scalability:** Simplifies the eventual split of modules into independent microservices.
- **Industry Standard:** Provides a well-documented, feature-rich framework for modern .NET applications.

## Consequences

- Introduces an additional dependency to the project.
- Requires slight adjustments to the `Shared` infrastructure to register the bus and its consumers.
- Coexists with Coravel, which remains the tool for task scheduling and simple fire-and-forget queues.
