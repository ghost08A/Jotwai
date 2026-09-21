# ADR 0010: Global three-tier project structure, matching company convention (supersedes ADR-0009)

## Status
Accepted. Supersedes ADR-0009.

## Context
ADR-0009 introduced a Domain/Service project pair *per Module* to get a compiler-enforced version of the module boundary ADR-0001 originally described as review-enforced only. After review against the company's existing engineering precedent — the Loyalty-Merchant project — that precedent uses a single **global** three-tier structure per backend (one Domain project, one Service project, one Web project), not a project pair per feature. To stay consistent with established team convention, tooling, and CI/onboarding expectations, Jotwai reverts to that global structure. Part of Issue #2 had already been scaffolded under ADR-0009's per-module shape; that work is being restructured to match this decision before the ticket is finished.

## Decision

**Repository layout:**
```
Jotwai/
├── client/
└── backend/
    ├── Jotwai.Domain/
    ├── Jotwai.Service/
    ├── Web.Jotwai/
    └── Jotwai.sln
```
(`server/` is renamed to `backend/`.)

**Projects** (one set for the whole backend, not one per feature):
- **`Jotwai.Domain`** — no project dependencies. Entities/models, enums, request models, view models, validation models, and interfaces for *all* features, organized by **technical-layer folder** (`Models/`, `Enums/`, `Interfaces/`, `RequestModels/`, `ValidateModel/`, `ViewModels/`) — matching the Loyalty-Merchant convention. Not organized by per-feature subfolder.
- **`Jotwai.Service`** — references `Jotwai.Domain` only. Business logic (`ImplementServices/`), helpers (`Helper/`), and each feature's EF Core `DbContext` + configurations (see DbContext strategy below).
- **`Web.Jotwai`** — references both `Jotwai.Service` and `Jotwai.Domain`. Controllers, `Program.cs`, middleware/configuration, `ExceptionHandler.cs`.
- Dependency direction: `Web.Jotwai` → `Jotwai.Service` → `Jotwai.Domain`; `Jotwai.Domain` → nothing.

Identity (and every future feature — Notes, Organization, AI Categorization, Search) lives inside these three projects as an internal feature, **not** as a separate project pair. No `src/Modules`, no `<Feature>.Domain`/`<Feature>.Service` projects.

**DbContext strategy** (this is what keeps ADR-0003's data isolation alive despite the project merge): each feature keeps its **own `DbContext`, scoped to its own Postgres schema** — `IdentityDbContext` → `identity` schema now, `NotesDbContext` → `notes` schema later, and so on. No shared `AppDbContext`. This is a deliberate choice to preserve schema-level isolation and a future extraction path even though the project-level split is gone.

**Cross-feature communication**: unchanged in substance from ADR-0002 — features still communicate via MediatR-style in-process Domain Events, kept as a **convention**, not a compiler-enforced rule. There is no longer a project boundary preventing one feature's code from directly calling another's; this now relies entirely on team discipline and code review. Direct concrete service-to-service calls across features are explicitly **not** the default, even though nothing technically prevents them anymore.

**Domain Event types** live in `Jotwai.Domain` (shared by all features — there's no cross-project reference problem left to solve, since it's one project); publish/handle logic lives in `Jotwai.Service`. ADR-0009's "narrow shared-kernel" question is moot: with only one Domain/Service project, no separate contracts project is ever needed for cross-feature events.

## Consequences
- Matches existing company convention (Loyalty-Merchant) — valuable for team familiarity, shared tooling/CI templates, and onboarding. This is judged to outweigh the compiler-enforced isolation ADR-0009 offered.
- Feature boundaries (Identity, and later Notes/Organization/AI Categorization/Search) are now enforced only by folder discipline within `Jotwai.Domain`/`Jotwai.Service` (a technical-layer-first folder structure, not even feature-first) plus code review and the MediatR-events convention. This is a real, accepted loss of rigor relative to ADR-0009.
- ADR-0001's claim that "extracting a Module later is a deployment change, not a redesign" no longer fully holds: extracting a feature later means carving its code out of shared `Jotwai.Domain`/`Jotwai.Service` projects, which is a genuine — if data-layer-bounded — redesign. The per-feature `DbContext`/schema split is the one hard boundary that survives, and is what keeps that future redesign from also being a full rewrite of the data layer.
- Supersedes ADR-0009 entirely (see that ADR's status). ADR-0001 and ADR-0002's module/event *design* concepts remain valid; only their physical realization (project structure) changes.
- Issue #2 is rescoped to create `Jotwai.Domain`, `Jotwai.Service`, `Web.Jotwai` (plus a test project) under `backend/`, with Identity's entities, `DbContext`, migration, and DI registration living inside them as the first feature — not as separate Identity-named projects.
