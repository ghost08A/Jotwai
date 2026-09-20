# ADR 0001: Modular monolith with extractable module boundaries

## Status
Accepted

## Context
Jotwai is greenfield with no proven load and (presumably) a small team. "A well-structured microservice system" was the ask, but true microservices from day one means separate deployables, separate databases, and inter-service networking/messaging before there's any real pain that justifies the cost.

## Decision
Build a single deployable ASP.NET Core backend, internally split into five Modules with strict boundaries:

- **Notes** — note CRUD, content, soft delete
- **Organization** — Topics (hierarchy) + Categories (flat)
- **AI Categorization** — background suggestion generation, suggestion state
- **Search** — cross-cutting query over content/Topic/Category
- **Identity** — accounts, login, sessions

Each Module owns its own database schema and communicates with other Modules only via in-process Domain Events (see ADR 0002). Client (`client/`) and backend (`server/`) live in one monorepo.

## Consequences
- No inter-service network calls, no distributed transactions, no service mesh/orchestration to operate for MVP.
- The module boundaries (schema ownership + event-only communication) are deliberately the same shape a real microservice boundary would have, so extracting a Module later is a deployment change, not a redesign.
- Discipline is required: nothing prevents a developer from reaching across a Module boundary in code except code review and the schema-per-module DB constraint (ADR 0003).
