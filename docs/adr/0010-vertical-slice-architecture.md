# ADR 0010: Vertical Slice Architecture (VSA) within Modules

## Status

Accepted

## Context

While "The Standard" provides excellent layering (Brokers -> Services -> Exposers), a traditional "Clean Architecture" implementation often leads to "folder-by-type" organization (all DTOs in one folder, all Services in another). This causes feature logic to be spread across the entire module, making it harder to navigate and maintain.

## Decision

We will adopt a **Vertical Slice Architecture (VSA)** approach _within_ each module of our Modular Monolith.

### Rules:

1. **Feature Co-location:** Inside a module (e.g., `Modules.Chat`), code will be grouped by feature/domain area rather than technical type.
2. **Slice Contents:** A vertical slice folder (e.g., `Features/Conversations/SendMessage`) will contain:
   - **Models/DTOs:** Request and Response objects specific to that feature.
   - **Logic:** Foundation/Processing/Orchestration services relevant to the slice.
   - **Exposer:** The Controller or Endpoint definition.
3. **Shared Infrastructure:** Common Brokers, Base Entities, and Cross-cutting services will still reside in the `Brokers` or `Base` folders of the module or the `Shared` project.
4. **Communication:** Slices can call Foundation Services of other slices within the same module, but cross-module communication must still follow ADR 0002 (Contracts/Events).

### Benefits:

- **High Cohesion:** Everything related to a single feature is in one place.
- **Low Coupling:** Changes to one feature are less likely to impact others.
- **Developer Velocity:** Faster navigation and reduced cognitive load for both humans and AI.

## Consequences

- Folders may contain a mix of different file types (.cs, .sql, etc.).
- Requires discipline to ensure shared logic is properly extracted when it truly belongs to the "Foundation" layer of the domain.
