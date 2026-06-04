# ADR 0008: Reliable Background Processing with Hangfire

## Status
Accepted

## Context
Critical tasks like document ingestion (Docling) and AI indexing are long-running and must be reliable. Standard `BackgroundServices` are lost if the server restarts.

## Decision
We will use **Hangfire** for all asynchronous, reliable background jobs.

### Implementation:
1. **Persistence:** Hangfire will use our PostgreSQL instance for job storage.
2. **Reliability:** Automatic retries with exponential backoff for failed Docling/OpenAI calls.
3. **Observability:** Enable the Hangfire Dashboard (local/admin only) to monitor job health.
4. **Distributed Locking:** Use Hangfire's built-in locking to prevent concurrent processing of the same session/document.

### Benefits:
- **Durability:** Jobs survive application restarts.
- **Resilience:** Built-in retry logic handles transient external failures.
- **Scalability:** Workers can be scaled independently if needed.

## Consequences
- Adds a dependency on the `Hangfire.AspNetCore` and `Hangfire.PostgreSql` packages.
- Requires additional database tables for job management.
