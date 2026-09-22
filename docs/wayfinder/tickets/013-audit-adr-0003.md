---
id: 13
title: "Audit implementation: ADR-0003 PostgreSQL, schema-per-module, and event-synced read models"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/13
---

## Question

Does the current codebase implement the decision recorded in `docs/adr/0003-schema-per-module-and-read-models.md`? Check DbContexts, EF Core configuration, migrations, and docker-compose.yml for schema-per-module and read-model patterns.

## Finding (partially implemented)

Only `IdentityDbContext` (`backend/Jotwai.Service/IdentityDbContext.cs`) exists, correctly using `HasDefaultSchema("identity")`, with its migration schema-qualifying all tables. `docker-compose.yml` provisions a single Postgres 16 instance, consistent with the one-database decision. No `NotesDbContext`/`OrganizationDbContext`/`AiCategorizationDbContext`/`SearchDbContext` exist, and no read-model/event-sync code exists for cross-module reads.

**Missing**: DbContexts, schemas, and migrations for `notes`, `organization`, `ai_categorization`, `search`; the read-model pattern for cross-module reads is unproven beyond the one-schema case. The pattern itself (own DbContext + `HasDefaultSchema`) is correctly established for Identity and should carry over directly.

Left open — implementation debt, not a re-litigated decision.
