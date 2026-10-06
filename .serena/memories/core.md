# Jotwai project map
- Root source-of-truth docs: `CLAUDE.md` (agent/repo rules), `ARCHITECTURE.md` (system architecture), `CONTEXT.md` (domain vocabulary), `docs/adr/` (architecture decisions/history), `docs/agents/` (agent workflows), `docs/wayfinder/` (current local ticket tracker).
- Backend source: `backend/`; frontend source: `client/`.
- Current architecture is a React SPA + ASP.NET Core modular monolith + PostgreSQL. Five conceptual modules: Notes, Organization, Nerd Assistant, Search, Identity.
- Module boundaries are enforced by convention: cross-module communication uses in-process Domain Events; no direct cross-schema SQL joins; consumers keep read models where needed.
- Important decision history: ADR-0009 was superseded by ADR-0010; ADR-0010 was superseded by ADR-0012 for DbContext placement. ADR-0005 was superseded by ADR-0011 for Identity implementation details.
- `docs/agents/issue-tracker.md` records the current decision that GitHub Issues are archive-only; active ticket tracking is local Markdown under `docs/wayfinder/`.
- Read `mem:tech_stack` for technology/version details, `mem:conventions` for durable coding/architecture conventions, and module-specific memories if present.