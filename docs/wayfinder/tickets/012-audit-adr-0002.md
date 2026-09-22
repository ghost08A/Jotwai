---
id: 12
title: "Audit implementation: ADR-0002 In-process event/mediator communication between modules"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/12
---

## Question

Does the current codebase implement the decision recorded in `docs/adr/0002-in-process-event-communication.md`? Search for a mediator/MediatR dependency or hand-rolled equivalent, any domain event types/classes, any event handlers, any pub/sub wiring in Program.cs or DI setup.

## Finding (not implemented)

No mediator package (MediatR or otherwise) is referenced in any `.csproj` — `Jotwai.Domain.csproj` has zero package references, `Jotwai.Service.csproj` only references EF Core/Npgsql/Identity/JWT packages. Grepping `backend/` for `MediatR`, `IEventHandler`, `IDomainEvent`, `INotification`, `DomainEvent`, and the specific event names from ARCHITECTURE.md (`NoteCreated`, `TopicRenamed`, `CategoryCreated`/`Renamed`/`Deleted`) returns nothing.

**Missing**: everything — a mediator dependency, the Domain Event types themselves, event handlers, and DI/pub-sub registration in `Program.cs`. No scaffolding exists yet; this needs to be introduced from scratch when the Notes module lands.

Left open — implementation debt, not a re-litigated decision.
