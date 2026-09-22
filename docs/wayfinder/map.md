---
title: Jotwai Requirements v0.1 — Decision Map
label: wayfinder:map
archived_from: https://github.com/ghost08A/Jotwai/issues/7
---

## Destination

A decided, engineering-ready spec resolving the behaviors from Requirements v0.1 that are not already answered by ARCHITECTURE.md / GLOSSARY.md / docs/adr/ — expressed as closed tickets on this map. Cross-checking the requirements doc against existing domain docs during charting showed most of the original 14 "open questions" already have answers on record (editor tech, rich-text representation, multi-category suggestions, category creation from suggestions, Topic mutability, search matching rules, pagination/sorting). What's left to decide: Note deletion/recovery UX, how AI Categorization handles a Note with no confident Category match, and whether accepting multiple suggested Categories is one transaction or independent actions.

This map additionally audits every currently-accepted ADR in `docs/adr/` against the actual codebase, since implementation (Identity module, some backend scaffolding) is ahead of some ADRs and behind others.

## Notes

- Domain docs to consult: ARCHITECTURE.md, GLOSSARY.md, `docs/adr/`.
- Non-goals already fixed by CLAUDE.md / ARCHITECTURE.md (don't re-litigate without checking with the user first): real-time/push infra, LLM semantic/vector search, note sharing/collaboration, note version history, image/file attachments, true microservices split.
- **Override of default wayfinder scope**: this map includes `wayfinder:research` audit tickets, one per currently-accepted ADR, whose question is "does the codebase implement this ADR's decision, and if not, what's missing?" Resolved by reading the codebase; if the decision is confirmed implemented, the ticket is closed and recorded below; if a gap is found, the ticket stays open (unassigned) with the finding recorded in it — tracked implementation debt, not a re-litigated decision.
- Superseded ADRs (0005, 0009) are not audited — their content is historical, replaced by ADR-0011 and ADR-0010 respectively.
- **Tracker**: this map uses the local-markdown tracker (`docs/wayfinder/`), not GitHub Issues, per explicit user instruction (2026-09-22) overriding this repo's default (`docs/agents/issue-tracker.md`) for wayfinder work specifically. See that doc's "Wayfinding operations" section for the local-markdown convention. The map and its tickets were originally charted as GitHub issues #7–#19 (ghost08A/Jotwai) and migrated here with full content preserved; each ticket file below records its original GitHub URL for archive purposes.

## Decisions so far

- [Decide Note deletion & recovery UX](tickets/008-note-deletion-recovery-ux.md): expose a Trash/Bin view — soft-deleted Notes are listable and user-restorable, not silently gone.
- [Decide AI behavior when no Category is confident enough](tickets/009-ai-no-confident-category.md): confidence threshold — below it, no Suggestion is surfaced at all.
- [Decide bulk-accept semantics for Category suggestions](tickets/010-bulk-accept-suggestions.md): one atomic transaction — create + attach all accepted Categories together, or none.
- [Audit implementation: ADR-0010 Global three-tier project structure](tickets/018-audit-adr-0010.md): confirmed implemented — project references and folder layout match exactly for the Identity module (only module built so far).
- [Audit implementation: ADR-0011 Custom Identity entities with password hashing](tickets/019-audit-adr-0011.md): confirmed implemented — custom `User`/`RefreshToken` POCOs, `IPasswordHasher`-only hashing with constant-time unknown-email handling, hand-issued JWTs, and atomic compare-and-swap refresh-token rotation, all present and tested.

## Open implementation-debt tickets (audited, not yet built)

Per-ADR audits found these behind the documented decision — left open as tracked backlog, not further decisions to make:

- [ADR-0001 modular monolith](tickets/011-audit-adr-0001.md) — only Identity module exists; Notes/Organization/AI Categorization/Search unbuilt.
- [ADR-0002 in-process event communication](tickets/012-audit-adr-0002.md) — no mediator/event infrastructure exists at all yet.
- [ADR-0003 schema-per-module + read models](tickets/013-audit-adr-0003.md) — only the `identity` schema/DbContext exists; pattern proven but not extended.
- [ADR-0004 outbox + AI Categorization background job](tickets/014-audit-adr-0004.md) — nothing built.
- [ADR-0006 Postgres substring search](tickets/015-audit-adr-0006.md) — nothing built.
- [ADR-0007 API-only backend, separate SPA deploy](tickets/016-audit-adr-0007.md) — API-only code shape is correct, but no CORS config and no deployment artifacts (Dockerfile/CI) exist yet to prove independent deployability.
- [ADR-0008 frontend stack and editor](tickets/017-audit-adr-0008.md) — React/Vite/router/TanStack Query/Tailwind scaffolding exists; Tiptap, shadcn/ui-style components, axios usage, and all feature code are missing.

## Not yet specified

- Whether "moving" a Note to a Topic includes inline-creating a not-yet-existing Topic in the same action, or requires the Topic to exist first (surfaced while narrowing the destination; not sharp enough to ticket yet).

## Out of scope

- Real-time/push notifications, semantic/vector search, note sharing, version history, file attachments, microservices split — per CLAUDE.md and ARCHITECTURE.md's explicit non-goals list. Not re-opened here.
