# ADR 0005: Safe Time Handling using NodaTime

## Status
Accepted

## Context
The standard .NET `DateTime` type is ambiguous regarding time zones and offsets, which frequently leads to "off-by-one-hour" bugs in multi-tenant/omnichannel systems.

## Decision
We will use **NodaTime** as the standard for all time-related operations.

### Rules:
1. **Domain Models:** Use `Instant` for timestamps and `ZonedDateTime` or `OffsetDateTime` for user-facing times.
2. **Persistence:** EF Core will be configured to map NodaTime types to PostgreSQL `timestamp with time zone`.
3. **Input/Output:** API DTOs will use ISO 8601 strings, parsed into NodaTime types at the Broker/Controller boundary.

### Benefits:
- **Clarity:** The type system forces you to think about time zones.
- **Bug Prevention:** Eliminates the ambiguity of `DateTimeKind`.
- **Standardization:** Follows industry best practices for enterprise systems.

## Consequences
- Requires adding the `NodaTime` and `NodaTime.Serialization.SystemTextJson` packages.
- Requires EF Core NodaTime provider configuration.
