# ADR 0006: Snapshots & Hermetic Integration Tests

## Status

Accepted

## Context

Traditional integration tests are often brittle (due to external databases) or tedious to maintain (due to complex assertions on large JSON objects).

## Decision

We will adopt **Testcontainers** and **Verify** for all integration testing.

### Implementation:

1. **Testcontainers:** Every integration test suite will spin up real, ephemeral instances of PostgreSQL and Qdrant in Docker.
2. **Verify (Snapshot Testing):** Use `Verify` to validate complex outputs (AI responses, document hierarchies).
3. **Hermetic:** No tests shall depend on pre-existing shared databases.

### Benefits:

- **Reliability:** Tests run against real infrastructure, not mocks, but stay perfectly isolated.
- **Maintainability:** Snapshot testing replaces hundreds of brittle `Assert` calls.
- **Reproducibility:** If it passes on one machine, it will pass in CI and for every developer.

## Consequences

- Requires Docker to be available in the development/CI environment.
- Increased initial test execution time due to container startup (mitigated by reuse patterns).
