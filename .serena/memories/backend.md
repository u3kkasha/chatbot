# Backend Development Standards

## "The Standard" (Hasan Habib) Patterns

- **Brokers:** Local interface (e.g. `IStorageBroker`), extremely thin wrapper, no business logic, no custom exceptions.
- **Services:** Local business logic, validation (structural + logical), catches broker exceptions and wraps them into localized exceptions (e.g. `ValidationException`, `DependencyException`). Developed strictly using TDD.
- **Exposers:** API controllers wrapping service calls; no validation or direct data retrieval logic here.

## C# 14 Syntax Conventions

- Emphasize performance (Native AOT readiness) and low allocation via DI Source Generators.
- Use records, primary constructors, expression-bodied members, collection expressions `[...]`.
- Use `ValueTask` or `ValueTask<T>` for async operations.
- Map Vogen value objects to standard DB primitives in `OnModelCreating`.
- Dependency management: ALWAYS use CLI `dotnet add package <Package>` instead of editing project files manually.
