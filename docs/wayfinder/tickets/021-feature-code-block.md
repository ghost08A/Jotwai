---
id: 21
title: "Build: Code Block with optional language metadata"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: [020-feature-note-capture-crud.md]
---

## Question

Product spec (Requirements v0.1 §3) is decided: Note content can include code blocks, optionally tagged with a language (C#, JavaScript, TypeScript, Python, SQL, Java, etc.); untagged blocks are generic text/code. Per ADR-0008, Tiptap provides the language picker; the block and its language are both part of the Markdown content, and both are searchable (ticket 026).

No Tiptap dependency exists yet (`client/package.json` has no `@tiptap/*` packages — confirmed by ADR-0008 audit, ticket 017), so this is blocked on the base editor existing.

## Finding

Not started. Depends on the Note editor (ticket 020) and ADR-0008's Tiptap integration (ticket 017) landing first.
