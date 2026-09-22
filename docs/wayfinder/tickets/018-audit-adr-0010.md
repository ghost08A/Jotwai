---
id: 18
title: "Audit implementation: ADR-0010 Global three-tier project structure, matching company convention"
type: wayfinder:research
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/18
---

## Question

Does the current codebase implement the decision recorded in `docs/adr/0010-global-three-tier-layering.md`? Check the three .csproj files' ProjectReferences, and list the actual folder structure under each of backend/Jotwai.Domain, backend/Jotwai.Service, backend/Web.Jotwai.

## Finding (implemented)

Project reference graph matches exactly: `Jotwai.Domain` has zero project references; `Jotwai.Service` references `Jotwai.Domain` only; `Web.Jotwai` references both. Folder structure under each project matches the ADR's technical-layer convention (`Interfaces/`, `Models/`, `RequestModels/`, `ValidateModel/`, `ViewModels/` under Domain; `ImplementServices/`, `Helper/`, `Configurations/`, `Migrations/` under Service; `Controllers/` under Web). No per-module subfolder split exists — correct per this ADR. `Enums/` folder is simply absent since no enums exist yet, not a deviation.

No further modules exist yet to prove the pattern holds at scale, but for everything currently built (Identity), the structure is exactly as decided.
