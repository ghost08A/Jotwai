---
id: 15
title: "Audit implementation: ADR-0006 Postgres substring search, no external search engine"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/15
---

## Question

Does the current codebase implement the decision recorded in `docs/adr/0006-search-postgres-substring.md`? Search for a Search module/controller, ILIKE usage, pg_trgm/GIN index migration, a read-model entity for search.

## Finding (not implemented)

Only migration is the Identity-only `InitialCreate` — no `pg_trgm` extension, no GIN index, no Search read-model table. No `SearchController` or `*Search*` files exist anywhere under `backend/`. Repo-wide grep for `ILIKE`, `pg_trgm`, `GIN`, `Search` found only the same forward-looking comment in `AuthController.cs` naming future modules — not actual code.

**Missing**: Search Module read-model entity + DbContext, migration enabling `pg_trgm`/GIN index, `ILIKE`-based query logic, `SearchController`, and event handlers projecting Note/Topic/Category changes into the read model.

Left open — implementation debt, not a re-litigated decision.
