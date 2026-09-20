# ADR 0002: In-process event/mediator communication between modules

## Status
Accepted

## Context
Given the modular monolith (ADR 0001), Modules still need to react to each other — e.g. AI Categorization needs to know when a Note is created; Search needs to know when Note content or Topic/Category names change. Direct in-process method calls between Modules would work today but silently couples them, defeating the point of drawing boundaries.

## Decision
Modules communicate exclusively through in-process Domain Events, published via a mediator (MediatR-style) when a Module's own state changes: `NoteCreated`, `NoteUpdated`, `NoteDeleted`, `TopicRenamed`, `CategoryCreated`, `CategoryRenamed`, `CategoryDeleted`, etc. No Module holds a direct reference to another Module's services or repositories.

## Consequences
- A Module can be developed, tested, and reasoned about without knowing which other Modules subscribe to its events.
- This event boundary is exactly where a future microservice extraction would cut — in-process pub/sub becomes a message broker, with minimal change to the publishing/subscribing code shape.
- Events are in-process and synchronous-by-default (same request/transaction) unless a handler explicitly defers work (e.g. AI Categorization writing to its outbox — see ADR 0004) — there is no distributed eventual-consistency story to design for yet.
