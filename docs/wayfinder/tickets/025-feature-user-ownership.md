---
id: 25
title: "Build: User Ownership enforcement (no auto re-categorization or move after initial organization)"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: [020-feature-note-capture-crud.md, 022-feature-topic-hierarchy.md, 023-feature-category-flat.md, 024-feature-ai-categorization-flow.md]
---

## Question

Product spec (Requirements v0.1 §8–§9; GLOSSARY.md "User Ownership") is decided: once a Note's Topic/Category is set (by the user or by accepting an AI suggestion), the system never moves or re-categorizes it automatically again — editing content never re-triggers AI, and only an explicit user action changes Topic/Category. This isn't a separate subsystem to build so much as an invariant that Notes/Topic/Category/AI Categorization must all respect — tracked as its own ticket so it isn't silently dropped while those land piecemeal (e.g. don't wire "on Note update" to re-run AI Categorization by accident).

## Finding

Not started — no code exists yet for any of the pieces this invariant constrains.
