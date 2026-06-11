---
name: Chatbot Backend Architecture
description: Enforces the hybrid architecture combining Hasan Habib's Brokers/Services separation, Vertical Slice Architecture (VSA), Functional Result Patterns (using OneOf), and DDD with modern C# 14 conventions.
---

# Chatbot Backend Architecture Guidelines

This skill governs the backend architectural decisions of the Chatbot codebase. It integrates elements of Hasan Habib's "The Standard" with Functional Result Patterns, Domain-Driven Design (DDD), and Vertical Slice Architecture (VSA).

## ⚙️ Core Architecture: The Standard + VSA Hybrid

We follow a **Modular Monolith** structure where modules are logically decoupled and "Ready-to-Split". Within each module (e.g., `Modules.Chat`), we enforce a **Vertical Slice Architecture (VSA)**.

### 1. Vertical Slice Architecture (VSA) Folder Rules

- Code must be grouped by feature/domain area, not by technical type.
- A vertical slice folder (e.g., `Features/Conversations/SendMessage`) must co-locate:
  - **DTOs / Models:** Request and response schemas specific to the feature.
  - **Logic:** Feature-specific service logic (Foundation, Processing, or Orchestration).
  - **Exposer:** The Minimal API endpoint or Controller definition.
- Common infrastructure (e.g., shared Database context, cross-cutting Brokers, base classes) belongs in `Brokers` or `Base` folders of the module or the `Shared` project.
- Slices can call Foundation Services of other slices within the same module, but cross-module communication must use MassTransit events or contracts.

### 2. Layering & Responsibility Boundaries

- **Brokers:** Act as lightweight, thin wrappers around external SDKs, databases, or APIs.
  - Must implement a local interface (e.g., `IStorageBroker`).
  - Must carry no business or mapping logic.
  - Must not catch or wrap exceptions, nor return `OneOf`. Let native exceptions bubble up to the Service layer.
- **Services (Foundation, Processing, Orchestration):**
  - Implement business logic and validations.
  - Catch native broker exceptions and wrap them in localized exceptions (e.g., `DependencyException`, `ServiceException`).
  - Return functional results for business errors.

---

## ⚡ Functional Result Patterns & Error Handling (OneOf)

We do **not** throw exceptions for business rules, validation failures, or expected domain conditions (e.g., Entity Not Found). Instead, we use functional discriminated unions via the **OneOf** library.

### 1. Service Return Types

- Methods must return `Task<OneOf<TSuccess, TError1, ...>>` or `ValueTask<OneOf<TSuccess, ...>>`.
- Common domain/business errors:
  - `ValidationError` (contains structured validation failures).
  - `NotFoundError` (resource does not exist).
  - `ConflictError` (business state conflict, concurrency issue).
  - `UnauthorizedError` (operation disallowed).

### 2. Example Service Method Signature

```csharp
public async ValueTask<OneOf<ChatSession, ValidationError, NotFoundError>> GetSessionAsync(
    ChatSessionId sessionId,
    TenantId tenantId);
```

### 3. Exposer/Endpoint Result Handling

- Map the `OneOf` result to HTTP `IResult` using C# pattern matching (`Match` or `Switch`):

```csharp
return result.Match(
    session => TypedResults.Ok(session.ToDto()),
    validation => TypedResults.BadRequest(validation.ToProblemDetails()),
    notFound => TypedResults.NotFound()
);
```

### 4. Categorized Exceptions (For Technical Failures Only)

We restrict exceptions to truly unexpected technical or transport failures (e.g., DB connection loss, API timeout).

- Catch native broker exceptions in the Service layer and wrap them in Xeption-derived categories:
  - `DependencyException` (transient external database or HTTP failures).
  - `ServiceException` (unhandled exceptions, system errors).
- Do **not** expose these exceptions to the client. Let them bubble up to the `GlobalExceptionHandler` to return a `ProblemDetails` response.

---

## 🔒 Domain-Driven Design (DDD) & C# 14 Conventions

### 1. Strongly-Typed IDs (StronglyTypedId)

- Every entity ID must be represented as a strongly-typed value object using **StronglyTypedId** (for Native AOT compatibility):

```csharp
[StronglyTypedId(Template.Guid)]
public readonly partial struct ChatSessionId;
```

- In EF Core entity configurations, always map these value objects:

```csharp
builder.Property(x => x.Id)
    .HasConversion(id => id.Value, value => new ChatSessionId(value));
```

### 2. Manual Mapping

- Do not use AutoMapper or similar reflection-based libraries.
- Write explicit extension methods or static factory methods to map between DTOs, Entities, and Events to preserve readability.

### 3. Modern C# Syntax Mandates

- Use **Primary Constructors** for dependency injection.
- Use **Records** for DTOs and immutable domain events.
- Use **Collection Expressions** `[...]` instead of `new List<T>()` or `new[]`.
- Use expression-bodied members and pattern matching where it improves readability.

---

## ⚙️ Configuration & Options (AOT & Test Compatibility)

To support Native AOT and prevent eager configuration errors in integration tests and OpenAPI doc generation, follow these rules for binding and validating configurations:

1. **Avoid Eager Configuration Reads:** Never use `configuration.GetSection(...)[...]` directly during DI registration (e.g. at startup in `AddSharedInfrastructure` or module extensions) to check values or throw exceptions.
2. **Standard Options Binding:** Bind configuration sections using `AddOptions<T>()`:
   ```csharp
   services.AddOptions<QdrantOptions>()
       .Bind(configuration.GetSection(QdrantOptions.SectionName));
   ```
3. **Lazy Dependency Configuration:** If a service registration or option depends on other settings, configure them lazily via DI factory lambdas or `IConfigureOptions<T>`:
   ```csharp
   services.AddTransient<IConfigureOptions<RedisCacheOptions>, ConfigureRedisCacheOptions>();
   ```
4. **Bypassing Validation at Startup:** Register source-generated options validators (`[OptionsValidator]`) and call `ValidateOnStart()` inside a check for OpenAPI generation mode:
   ```csharp
   if (!isGeneratingOpenApi)
   {
       builder.ValidateOnStart();
   }
   ```
   This ensures that missing configurations in isolated environments (such as CI workflows generating documentation or running integration tests) do not trigger startup crashes since services are registered but never resolved.
