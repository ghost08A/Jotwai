# Jotwai

"Capture first, organize later" note-taking app. React SPA client + ASP.NET Core modular-monolith backend + PostgreSQL.

## Routing — read first, even for trivial edits

Before touching any file under a path below, you MUST Read the matching file
first — no exceptions for small changes.

| Path prefix | Read this file first |
| --- | --- |
| `client/**` | `client/CLAUDE.md` |
| `backend/**` | `backend/CLAUDE.md` |

Before working in this repo, read:
- **[ARCHITECTURE.md](ARCHITECTURE.md)** — module map, data strategy, AI categorization flow, frontend stack, deployment
- **[GLOSSARY.md](GLOSSARY.md)** — exact domain vocabulary (Note, Topic, Category, Suggestion, Module, Read Model, ...). Use these terms, don't invent synonyms.
- **[docs/adr/](docs/adr/)** — why each architectural decision was made; read the relevant ADR before changing that area

## Tech Stack

- Client: React + TypeScript + Vite — see [client/CLAUDE.md](client/CLAUDE.md)
- Backend: ASP.NET Core (.NET 10) modular monolith + EF Core + PostgreSQL — see [backend/CLAUDE.md](backend/CLAUDE.md)

## Repo layout

- `client/` — React + TypeScript + Vite SPA
- `backend/` — ASP.NET Core modular monolith, global 3-tier (`Jotwai.Domain`, `Jotwai.Service`, `Web.Jotwai`) per ADR-0010. Modules (Notes, Organization, AI Categorization, Search, Identity) live inside these three projects as internal features, not separate projects.
- `docs/adr/` — architecture decision records

## Ground rules

- Modules in `backend/` communicate only via in-process Domain Events, by convention — never direct calls or cross-schema queries between modules (see ADR 0001–0003, ADR-0010). Each Module still keeps its own `DbContext` scoped to its own schema inside `Jotwai.Service`, even though the project split is global.
- Note content is Markdown; the editor (Tiptap) is a view over that, not the source of truth.
- Don't add real-time/push infrastructure, semantic search, note sharing, or version history without checking with the user first — these are explicit non-goals for the current scope (see ARCHITECTURE.md).
- Never run `git commit` on the user's behalf, even when a skill/workflow (e.g. `/implement`) says to commit as one of its steps. Implement, verify (build, tests), and stop — the user commits themselves.

## Security Rules

- DO NOT hardcode API keys, tokens, passwords, or credentials in source code
- DO NOT log sensitive data (passwords, tokens, secrets, connection strings)
- DO NOT commit `.env`, `.env.local`, or any file containing secrets or credentials
- `.env.example` is the only env file allowed in version control — placeholder values only
- Never expose stack traces or internal error details in API responses
- `appsettings.Example.json` is also an acceptable config template alongside `.env.example` — placeholder values only

## Definition of Done

A task is complete only when

- Build succeeds
- Tests pass
- Formatting and any configured analyzers pass, verified in CI and not only locally
- Existing behavior is preserved
- Requested functionality is implemented
- No unnecessary files are added
- No unrelated code is modified

## Agent skills

### Issue tracker

Issues live in GitHub Issues for ghost08A/Jotwai, using the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Domain docs

Single-context layout: ARCHITECTURE.md, GLOSSARY.md, and docs/adr/ at the repo root. See `docs/agents/domain.md`.

### Backend tickets

Naming conventions, an over-engineering checklist, and a pre-done checklist for `backend/` work. See `docs/agents/backend.md`.
