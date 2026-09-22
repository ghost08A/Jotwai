---
id: 20
title: "Build: Note capture & CRUD (create without Topic/Category, edit-overwrite, soft delete + Trash)"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §2, §9; GLOSSARY.md "Note"; ticket 008's Trash decision) is fully decided for Note capture. Nothing exists in code yet (confirmed: no `Note` model/controller anywhere in `backend/`, no Note UI in `client/src`). What needs building:

- Note creation with no required Topic/Category, saved immediately (BR-001, BR-002).
- Content stored as Markdown; edits overwrite (no version history).
- Soft delete + the decided Trash/Bin view (list + Restore action, ticket 008).

This is a build task, not an open decision — tracked here as backlog per this map's implementation-debt convention. Depends on the Notes module/schema existing (see ADR-0001/0003 audits, tickets 011/013) and the Domain Event mechanism (ticket 012) for `NoteCreated` to fire AI Categorization (ticket 024).

## Finding

Not started. No `Note` entity, DbContext, controller, or client-side Note UI exists anywhere in the repo.
