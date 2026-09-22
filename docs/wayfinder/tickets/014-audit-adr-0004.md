---
id: 14
title: "Audit implementation: ADR-0004 Outbox + background worker for AI Categorization, using Gemini"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/14
---

## Question

Does the current codebase implement the decision recorded in `docs/adr/0004-ai-categorization-background-job.md`? Search for an outbox table/entity, a BackgroundService/IHostedService implementation, a Gemini/AI provider client or interface, any Notes or AI Categorization module code at all.

## Finding (not implemented)

Repo-wide search for `Outbox`, `Gemini`, `BackgroundService`, `IHostedService`, `Categoriz`, `Suggestion`, `Note`, `NoteCreated` matched only one file: `backend/Web.Jotwai/Controllers/AuthController.cs`, and only as a forward-looking code comment naming future modules — not any actual implementation. Only migration present is the Identity-only `InitialCreate`.

**Missing**: Notes module (`Note` entity, `NoteCreated` domain event), outbox table + transactional write on note creation, `BackgroundService`/`IHostedService` poller, AI provider interface + Gemini (2.5 Flash-Lite) client, Category/Suggestion domain models and persistence, hosted-service wiring in `Program.cs`.

Left open — implementation debt, not a re-litigated decision.
