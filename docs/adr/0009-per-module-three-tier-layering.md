# ADR 0009: Per-module three-tier project structure (Domain/Service per Module + shared API)

## Status
**Superseded by ADR-0010.** Kept for historical record — the per-module project split described below was implemented for part of Issue #2, then reverted before that ticket was finished, in favor of matching the company's existing global 3-tier convention (see ADR-0010 for why and what replaces it).

## Context
The company enforces a 3-tier project standard on all backend projects:

- **Domain** — no project dependencies; entities, enums, DTOs, common models, core interfaces.
- **Service** — references Domain only; `DbContext` + EF Core configurations, business logic/services, service interfaces, external integrations.
- **API** — references Service and Domain; controllers/endpoints, middleware, auth/JWT, configuration, DI registration.
- Dependency direction: API → Service → Domain.

ADR-0001 already splits the backend **vertically** into five Modules (Notes, Organization, AI Categorization, Search, Identity), each a bounded context designed to be independently extractable, with boundaries enforced by schema ownership (ADR-0003) and event-only communication (ADR-0002). Applying the 3-tier standard literally as one global `Domain`/`Service`/`API` project set for the whole backend would decompose the codebase by technical layer instead of by Module — the opposite axis from ADR-0001 — merging all five Modules' entities into one Domain assembly and all business logic into one Service assembly. That would weaken ADR-0001's central justification ("extracting a Module later is a deployment change, not a redesign") and leaves no explicit home for Domain Events, mediator dispatch, or Read Model projections (ADR-0002, ADR-0003).

## Decision
Apply the company's 3-tier layering **independently within each Module**, not once globally:

- **`<Module>.Domain`** — entities, enums, DTOs, and core interfaces for that Module only. No project dependencies.
- **`<Module>.Service`** — references only that Module's own `.Domain`. Owns that Module's EF Core `DbContext` and configurations (scoped to that Module's own Postgres schema, per ADR-0003), business logic, service interfaces, and external integrations.
- **`Jotwai.Api`** — one shared ASP.NET Core Web API host (preserving "one deployable" from ADR-0001), referencing every Module's `.Service` (and transitively its `.Domain`). Hosts controllers/endpoints, middleware, JWT authentication, configuration, and DI registration for all Modules.

**Hard rule**: a Module's `.Domain` or `.Service` must never reference another Module's `.Domain` or `.Service`. This makes the module boundary ADR-0001 described as code-review-enforced into a **compiler-enforced** one.

**No Shared Kernel project is introduced by this decision.** One is justified only by a concrete cross-Module need that can't otherwise be met — none exists yet (see Domain Events, below).

**Domain Events placement (resolves the ADR-0002 gap):** a Module's own Domain Event types are declared in that Module's `.Domain` project as plain types with no framework dependency. Publishing and handling logic (mediator wiring) lives in that Module's `.Service` project. A Module's `.Service` handles only events it defines and publishes itself — never by referencing another Module's `.Domain` to see that Module's event shape. Cross-Module event consumption (e.g. a future Search Module reacting to `NoteCreated` from the Notes Module) is deferred: it is exactly the concrete requirement that would justify a minimal shared contracts project later, scoped narrowly to event marker types, when the first real cross-Module consumer exists. For Issue #2, the Identity Module has no cross-Module event consumers — other Modules will identify the current user via the JWT bearer contract enforced at the `Jotwai.Api` HTTP layer, not via a Domain Event — so no shared project is needed now.

**ADR-0003 (schema-per-module, DbContext-per-module) is unchanged and reaffirmed.** Each Module's `DbContext` now has an explicit, unambiguous home — its own `.Service` project — which closes the ambiguity the company standard's singular "DbContext … in Service" phrasing could otherwise have introduced (e.g. a shared `DbContext` spanning modules).

## Consequences
- More projects overall (an eventual ~11 across all five Modules: 5×`.Domain` + 5×`.Service` + 1×`Jotwai.Api`), but Module boundaries are now enforced by the compiler, not just review.
- Issue #2 scaffolds only `Jotwai.Identity.Domain`, `Jotwai.Identity.Service`, and `Jotwai.Api` — the other four Modules' projects are created only when their own tickets start, avoiding speculative scaffolding.
- If a future Module genuinely needs to consume another Module's Domain Event, that is the trigger to introduce a narrowly-scoped shared contracts project (event marker types only) — not before.
- This ADR supersedes the project-structure implications of ADR-0001 (which described Modules only at the schema/communication level, not the project level) without changing ADR-0001's module boundaries themselves.
