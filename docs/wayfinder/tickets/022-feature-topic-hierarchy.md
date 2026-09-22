---
id: 22
title: "Build: Topic hierarchy (create/nest, sibling uniqueness, single-Topic-per-Note, non-empty delete guard)"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §4; GLOSSARY.md "Topic"; BR-003, BR-004) is decided: Topics are hierarchical with unlimited depth, sibling names unique case-insensitively per parent, a Note belongs to at most one Topic, a Topic with Notes or child Topics can't be deleted until emptied, and moving a Note to another Topic is always an explicit user action. Nothing exists in code (Organization module is entirely unbuilt — see ADR-0001 audit, ticket 011).

Note: the "Not yet specified" fog item on the map (whether moving to a not-yet-existing Topic can inline-create it) should be resolved as its own grilling ticket before or during this build — not assumed either way here.

## Finding

Not started. No Topic entity, DbContext, or UI exists anywhere in the repo.
