# ADR 0004: Performance & Native AOT Readiness

## Status

Accepted

## Context

High performance and efficient resource usage are critical for our platform. We want the API to be ready for serverless or highly dense containerized deployments.

## Decision

We will design the system to be **Native AOT (Ahead-of-Time)** compatible from day one.

### Implementation:

1. **Source Generation:** Use `System.Text.Json` source generators for all DTOs and internal models.
2. **Reflection-Free:** Avoid reflection-heavy libraries or patterns in the hot path.
3. **DI Validation:** Use `Microsoft.Extensions.DependencyInjection` source generators where applicable to ensure resolution safety at compile-time.
4. **Configuration:** Use source-generated Options for environment variables.

### Benefits:

- **Startup Time:** API starts in milliseconds.
- **Memory Footprint:** Significantly lower RAM usage compared to JIT-compiled apps.
- **Deployment:** Binaries are standalone and don't require the full .NET runtime in the image.

## Consequences

- Requires strict adherence to AOT-safe coding patterns.
- Some legacy .NET libraries may be incompatible.
