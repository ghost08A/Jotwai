# ADR 0004: Outbox + in-process background worker for AI Categorization, using Gemini

## Status
Accepted

## Context
Per the product requirements, AI categorization must run in the background and must never block Note creation (BR-008, BR-009), and must run automatically exactly once per Note (BR-010, BR-011, BR-012). It needs to survive an app restart between "Note saved" and "AI suggestion produced" — a fire-and-forget in-memory task would silently drop work on a crash/redeploy. It also needs an actual LLM to call; this is a lightweight classification task (pick likely Categories for a short note), not one that needs a large/expensive/highly-capable model.

## Decision
- The AI Categorization Module writes an **outbox row** transactionally when it reacts to `NoteCreated` (first save with real content only).
- An in-process **`BackgroundService`** polls the outbox, calls the AI provider, and records the resulting ranked Suggestion.
- No external message broker (RabbitMQ/Azure Service Bus) or job framework (Hangfire) for MVP — the outbox table is the durability mechanism, and it is the seam where a real broker would be introduced if this Module is ever extracted.
- AI provider: **Google Gemini** (2.5 Flash-Lite tier), chosen for its generous free tier and because the classification task doesn't require a large model. The provider is called through an interface owned by this Module, so switching providers later is a configuration change, not a rewrite.

## Consequences
- Pending categorization work survives an app restart (it's a row in the database, not in-memory state).
- No new infrastructure (broker, job server) to run or operate for MVP.
- Gemini's free-tier rate limits are a real constraint to monitor; the provider abstraction means hitting them doesn't require re-architecting, just swapping the implementation behind the interface.
- The suggestion result is surfaced to the client as a non-real-time in-app indicator (no websocket/push infrastructure) — the user sees it next time they view the Note or open the app.
