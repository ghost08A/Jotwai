# ADR 0003: PostgreSQL, schema-per-module, and event-synced read models for cross-module reads

## Status
Accepted

## Context
The modules need to persist data, and some modules need data owned by another module: Search needs Note content plus Topic/Category names; AI Categorization needs the current Category list. Physically sharing one database makes a cross-schema SQL join trivial to write — but doing so would quietly erase the module boundary ADR 0001 and 0002 establish, making a future extraction much harder.

## Decision
- Use a single **PostgreSQL** database (free, cross-platform, first-class EF Core support, works well in Docker for local dev).
- Give each Module its own **schema** (`notes`, `organization`, `ai_categorization`, `search`, `identity`), accessed only through that Module's own EF Core `DbContext`.
- A Module that needs another Module's data maintains its **own denormalized read model**, kept current by subscribing to that Module's Domain Events (ADR 0002) — e.g. Search's read model is updated on `NoteCreated`/`NoteUpdated`/`TopicRenamed`/`CategoryRenamed`. No Module ever issues a SQL join across another Module's schema.

## Consequences
- Search and AI Categorization carry some data duplication (their own copies of Note/Topic/Category data) and eventual consistency (a brief lag between a write and the read model reflecting it) — acceptable for this domain, where search results being a moment stale is not a correctness problem.
- If a Module is later extracted into its own service, its read-model-via-events pattern is unchanged — only the transport (in-process → message broker) changes.
- Schema-per-module is enforced by convention/DbContext configuration, not a hard database-level barrier (all schemas are still in one physical database) — code review is the actual enforcement mechanism.
