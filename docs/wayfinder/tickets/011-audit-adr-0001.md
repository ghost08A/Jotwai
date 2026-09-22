---
id: 11
title: "Audit implementation: ADR-0001 Modular monolith with extractable module boundaries"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/11
---

## Question

Does the current codebase implement the decision recorded in `docs/adr/0001-modular-monolith.md` (ADR-0001: Modular monolith with extractable module boundaries)? Read the ADR in full, then check `backend/` (and `client/` where relevant) for whether it matches. Report: implemented / partially implemented / not implemented, with file references, and if partial/not, exactly what's missing.

## Finding (partially implemented)

Only the Identity module exists in code (single global 3-tier project split, matching ADR-0010). Domain models (`Jotwai.Domain/Models/User.cs`, `RefreshToken.cs`, `RefreshTokenIssuance.cs`), service (`AuthService.cs`, `TokenService.cs`), `IdentityDbContext` (schema-scoped to `identity`), migration, and `AuthController` all exist. No `DomainEvent`/`IEventHandler`/MediatR infrastructure exists anywhere in `backend/` — the in-process Domain Event mechanism this ADR depends on for module boundaries is unbuilt, and there's no second module yet to test the boundary against.

**Missing**: Notes, Organization, AI Categorization, and Search modules (no models/DbContext/service/controller for any of them); the Domain Event mechanism itself (tracked separately in ADR-0002's audit, ticket 012).

Left open — this is implementation debt, not a re-litigated decision. Revisit once the Notes module lands.
