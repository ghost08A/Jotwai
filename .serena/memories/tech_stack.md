# Jotwai stack
- Frontend: React + TypeScript + Vite; react-router-dom; TanStack Query for server state; axios for HTTP; Tailwind CSS + headless/shadcn-style UI; Tiptap/ProseMirror editor; Markdown is persisted source of truth.
- Backend: ASP.NET Core .NET 10 modular monolith; projects currently `Jotwai.Domain`, `Jotwai.Service`, `Web.Jotwai`; integration tests in `Jotwai.IntegrationTests`.
- Database: PostgreSQL, schema-per-module (`notes`, `organization`, `ai_categorization`, `search`, `identity`); module-specific DbContexts.
- AI: Google Gemini, currently 2.5 Flash-Lite tier; provider is behind an interface owned by Nerd Assistant.
- Background processing: in-process `BackgroundService` consuming durable outbox rows; no external broker for MVP.
- Search: PostgreSQL `ILIKE` substring search with `pg_trgm`/GIN indexing over Search read model; no vector/semantic search in MVP.
- Auth: custom Identity entities/services with password hashing reused from ASP.NET Core Identity; JWT access + refresh tokens. See ADR-0011.