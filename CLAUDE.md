# Jotwai

"Capture first, organize later" note-taking app. React SPA client + ASP.NET Core modular-monolith backend + PostgreSQL.

Before working in this repo, read:
- **[ARCHITECTURE.md](ARCHITECTURE.md)** — module map, data strategy, AI categorization flow, frontend stack, deployment
- **[GLOSSARY.md](GLOSSARY.md)** — exact domain vocabulary (Note, Topic, Category, Suggestion, Module, Read Model, ...). Use these terms, don't invent synonyms.
- **[docs/adr/](docs/adr/)** — why each architectural decision was made; read the relevant ADR before changing that area

## Repo layout

- `client/` — React + TypeScript + Vite SPA
- `server/` — ASP.NET Core modular monolith (Notes, Organization, AI Categorization, Search, Identity modules)
- `docs/adr/` — architecture decision records

## Ground rules

- Modules in `server/` communicate only via in-process Domain Events — never direct calls or cross-schema queries between modules (see ADR 0001–0003).
- Note content is Markdown; the editor (Tiptap) is a view over that, not the source of truth.
- Don't add real-time/push infrastructure, semantic search, note sharing, or version history without checking with the user first — these are explicit non-goals for the current scope (see ARCHITECTURE.md).

## Agent skills

### Issue tracker

Issues live in GitHub Issues for ghost08A/Jotwai, using the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Domain docs

Single-context layout: ARCHITECTURE.md, GLOSSARY.md, and docs/adr/ at the repo root. See `docs/agents/domain.md`.
