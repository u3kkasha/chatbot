# System Architecture

## Topology

Decoupled modular monolith structure:

- **Nuxt 4 Client** (port 3000) interacting with **.NET 10 API** (port 5136).
- Datastores: PostgreSQL 18 (relational), Qdrant (vector db), Azurite (blob storage), Garnet (distributed cache/locking).
- Logging: Seq Log server (port 8081 for UI).

## Database Isolation

- Logical multi-tenant isolation via Tenant ID matching.
- PostgreSQL Row-Level Security (RLS) configured on the relational database.
- Qdrant Vector search queries must append a mandatory `tenant_id` payload match filter.

## EF Core 10 Configuration

- Enforced snake_case naming conventions.
- Integrated audit interceptors (`AuditInterceptor.cs`) for entity timestamps.
- Native JSONB mapping (`citations_json`) configured for AI citations and metadata.
