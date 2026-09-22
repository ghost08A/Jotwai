---
id: 26
title: "Build: Search (content incl. code blocks, Topic, Category — case-insensitive substring)"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §10–§11; GLOSSARY.md "Search"; BR-014 through BR-016) plus ADR-0006 are decided: case-insensitive substring match over Note content (including code block bodies), a code block's declared language, Topic, and Category, via Postgres `ILIKE` + `pg_trgm` GIN index against the Search Module's own read model. Filtering by parent Topic includes descendants. No semantic/vector search for MVP (explicit non-goal). Nothing exists in code — confirmed by ADR-0006 audit (ticket 015). Blocked on the Notes/Topic/Category modules existing (read model needs something to project from) and the Domain Event mechanism (ticket 012).

## Finding

Not started.
