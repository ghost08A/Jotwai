# Jotwai Architecture

See `GLOSSARY.md` for domain terms used below, and `docs/adr/` for the reasoning behind each decision.

## Overview

Jotwai is a "capture first, organize later" note-taking app: React SPA client, ASP.NET Core backend, PostgreSQL storage. The backend is a **Modular Monolith** — one deployable, five internally-bounded Modules — not a true microservices system yet. Each Module is designed to be extractable into its own service later without a rewrite.

## Repository layout

Single monorepo:
- `client/` — React SPA
- `backend/` — ASP.NET Core modular monolith (`Jotwai.Domain`, `Jotwai.Service`, `Web.Jotwai`, `Jotwai.sln`)
- `docs/adr/` — architecture decision records
- `GLOSSARY.md`, `ARCHITECTURE.md`, `CLAUDE.md` — project-level docs

## Backend modules

| Module | Owns | Responsibilities |
|---|---|---|
| **Notes** | `notes` schema | Note CRUD, Markdown content, soft delete |
| **Organization** | `organization` schema | Topics (hierarchy), Categories (flat), naming rules |
| **AI Categorization** | `ai_categorization` schema | Background suggestion generation, suggestion state, outbox |
| **Search** | `search` schema | Cross-cutting search over content/Topic/Category, via its own read model |
| **Identity** | `identity` schema | Accounts, login, sessions |

### Backend project structure (global 3-tier)

Per company standard (matching the existing Loyalty-Merchant convention), the backend is **one global** Domain → Service → Web layering, not one set per Module (ADR-0010, superseding ADR-0009):

- **`Jotwai.Domain`** — no project dependencies. Entities/models, enums, request models, view models, validation models, and interfaces for *all* Modules, organized by technical-layer folder (`Models/`, `Enums/`, `Interfaces/`, `RequestModels/`, `ValidateModel/`, `ViewModels/`) — not per-Module subfolders.
- **`Jotwai.Service`** — references `Jotwai.Domain` only. Business logic (`ImplementServices/`), helpers (`Helper/`), and each Module's own `DbContext` + EF Core configurations.
- **`Web.Jotwai`** — references both `Jotwai.Service` and `Jotwai.Domain`. Controllers, `Program.cs`, middleware/configuration, `ExceptionHandler.cs`.

Modules (Identity, and later Notes/Organization/AI Categorization/Search) live inside these three projects as internal features, not as separate project pairs. The module boundary is enforced by folder discipline, code review, and the event-communication convention below — not by the compiler.

### Module communication

Modules never call each other's code or query each other's schema directly, by convention. They communicate through **Domain Events**, published in-process via a mediator (MediatR-style) when a Module's own state changes — e.g. `NoteCreated`, `NoteUpdated`, `NoteDeleted`, `TopicRenamed`, `CategoryCreated`, `CategoryRenamed`, `CategoryDeleted`. Event types and handler wiring both live in the shared `Jotwai.Domain`/`Jotwai.Service` projects (ADR-0010) — there's no cross-project reference problem to solve, since it's one project, but nothing prevents a direct cross-Module call either beyond code review.

### Cross-module reads: Read Models, not joins

If a Module needs data owned by another Module (Search needs Note content and Topic/Category names; AI Categorization needs the current Category list), it keeps its **own denormalized Read Model**, kept current by subscribing to the relevant Domain Events. No Module issues a SQL join across another Module's schema, even though physically they share one PostgreSQL database. This is what keeps the boundary real and extraction-ready.

### Database

One PostgreSQL database, **schema-per-module** (`notes`, `organization`, `ai_categorization`, `search`, `identity`). Each Module has its own EF Core `DbContext` scoped to its own schema (e.g. `IdentityDbContext`); all these `DbContext`s live inside the shared `Jotwai.Service` project (ADR-0010) — no shared `AppDbContext`.

### AI Categorization flow

1. Notes Module saves a new Note, publishes `NoteCreated`.
2. AI Categorization Module writes an **outbox** row transactionally in response (durable — survives a restart).
3. A `BackgroundService` polls the outbox, calls the AI provider, and produces a ranked Suggestion (candidate Categories, possibly including a brand-new name).
4. Suggestion state is exposed to the client as a non-real-time in-app indicator — the user sees it next time they view the Note or open the app, no push/websocket infrastructure.
5. User acts (accepts any subset / edits / ignores) — this resolves the suggestion set; unaccepted candidates are discarded, not left pending.
6. Per **User Ownership**: this flow runs exactly once automatically, on first save with content. Editing a Note never re-triggers it. The user can manually re-invoke a "Suggest Category" action at any time.

**AI provider**: Google Gemini (2.5 Flash-Lite tier) — a lightweight classification task doesn't need a large/expensive model. The provider sits behind an interface inside the AI Categorization Module so it's swappable via config.

### Search implementation

Postgres `ILIKE` substring queries against the Search Module's own read model, backed by a `pg_trgm` trigram GIN index for performance. No external search engine, no semantic/vector search — matches the explicit MVP scope decision. Filtering by a parent Topic includes all descendant Topics.

### Auth

ASP.NET Core Identity (email/password for MVP, OAuth as a fast-follow) issuing JWT access + refresh tokens. The SPA attaches the access token to API calls.

### Background jobs

In-process `BackgroundService` consuming the outbox table described above. No external message broker (RabbitMQ/Azure Service Bus) for MVP — this is the seam where one would be introduced if AI Categorization is ever extracted into its own service.

## Frontend

- **React + TypeScript + Vite**
- **react-router-dom** for routing
- **TanStack Query** for server state (fetching/caching Notes, Topics, Categories)
- **axios** for HTTP calls to the API
- No dedicated client-state library — `useState`/Context handles UI-only state (active filters, editor draft, sidebar state)
- **Tailwind CSS** + shadcn/ui-style headless components for styling
- **Tiptap** (ProseMirror) as the Note editor, serializing to/from Markdown — gives WYSIWYG bullet/numbered lists and a code-block language picker while the canonical storage stays plain Markdown

Navigation model: a default "All Notes" feed (sorted by most-recently-updated, infinite scroll) with a Topic tree and Category filter as persistent side navigation; search is layered on top, not the only way to find a Note.

## Deployment topology

The ASP.NET Core backend serves **only the JSON API**. The React SPA is built and deployed as a **separate static artifact** (static hosting/CDN), calling the API over HTTP. They are independently deployable and scalable.

## Local development

`docker-compose.yml` at the repo root runs **PostgreSQL only**. The API runs natively via `dotnet watch`; the client runs natively via `vite dev`. App processes are not containerized for local dev — only the database, to keep the inner loop fast.

## Explicit non-goals for MVP

- No LLM semantic/vector search
- No real-time push notifications (websockets) for AI suggestions
- No note sharing/collaboration between accounts
- No note version history
- No image/file attachments in Notes
- No true separate microservices deployment (that's a possible future extraction, not the MVP)
