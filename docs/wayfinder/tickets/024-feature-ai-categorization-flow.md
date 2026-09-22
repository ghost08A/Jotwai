---
id: 24
title: "Build: AI Categorization suggestion flow (background trigger, confidence threshold, accept/edit/ignore, atomic bulk-accept)"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §6–§8; GLOSSARY.md "AI Categorization"/"Suggestion"; BR-007 through BR-012) plus this map's own decisions are all settled:

- Runs in background on first save with real content only; never blocks Note creation; never re-triggers automatically on edit.
- Produces a ranked Suggestion (possibly including a brand-new Category name).
- Below the confidence threshold, nothing is shown (ticket 009).
- User accepts any subset / edits / ignores; accepting is one atomic transaction (ticket 010).
- Once resolved (accepted/ignored), never auto-moves the Note again (User Ownership, ticket 025).

Nothing exists in code yet — confirmed by ADR-0004 audit (ticket 014): no outbox, no BackgroundService, no Gemini client. Blocked on the Notes module (ticket 020, for `NoteCreated`) and the Domain Event mechanism (ticket 012).

## Finding

Not started.
