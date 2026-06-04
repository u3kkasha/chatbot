# ADR 0003: Functional Result Patterns using OneOf

## Status
Accepted

## Context
Standard C# error handling often relies on returning `null` or throwing exceptions for flow control (e.g., `UserNotFound`). This is implicit, error-prone, and ambiguous for AI agents.

## Decision
We will use the **OneOf** library to implement **Functional Result Patterns** (Discriminated Unions) for all service methods.

### Implementation:
- Methods will return a `OneOf<TSuccess, TError1, TError2>` type.
- Example: `public Task<OneOf<User, UserNotFound, ValidationError>> GetUserByIdAsync(UserId id)`.
- Use **Pattern Matching** to handle results in the calling layer.

### Benefits:
- **Explicitness:** The signature tells you exactly what can happen.
- **LLM-Friendly:** AI agents can't "forget" to handle an error case because the type system requires it.
- **Safety:** Eliminates "magic" exceptions and null-reference risks.

## Consequences
- Requires the `OneOf` library dependency.
- Developers must learn the pattern matching syntax for results.
