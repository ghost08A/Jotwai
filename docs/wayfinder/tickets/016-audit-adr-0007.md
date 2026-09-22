---
id: 16
title: "Audit implementation: ADR-0007 API-only backend, separately deployed static SPA"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/16
---

## Question

Does the current codebase implement the decision recorded in `docs/adr/0007-deployment-topology.md`? Check Program.cs for static-file/SPA serving, controllers for pure-API shape, and any CI/CD/Dockerfile/docker-compose config showing separate deployability.

## Finding (partially implemented)

`Program.cs` has no `UseStaticFiles`/`UseSpaStaticFiles`/SPA-fallback wiring, and `AuthController` is a pure `[ApiController]` returning JSON — the "API-only, no SPA serving" half of the decision is honored. But no `.github/workflows/`, no `Dockerfile` anywhere, and `docker-compose.yml` only runs Postgres/pgAdmin — nothing demonstrates the two being independently deployed yet. No CORS configuration exists either, which the ADR calls out as a required consequence.

**Missing**: deployment config (Dockerfile(s)/CI) actually showing SPA and API as separate artifacts, and CORS setup for the SPA's origin.

Left open — implementation debt, not a re-litigated decision.
