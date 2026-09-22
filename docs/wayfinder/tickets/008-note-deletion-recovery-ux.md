---
id: 8
title: Decide Note deletion & recovery UX
type: wayfinder:grilling
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/8
---

## Question

GLOSSARY.md says a Note is soft-deletable, but doesn't say whether the user sees a recovery flow. Does the product expose a Trash/Bin view where the user can list and restore soft-deleted Notes, or is soft-delete purely an internal safety net with no user-facing recovery?

## Resolution

**Decision**: Yes — expose a Trash/Bin view. Soft-deleted Notes remain listable and restorable by the user (a "Restore" action), rather than soft-delete being purely an internal safety net.

Rationale: the schema already supports it (soft delete, per GLOSSARY.md/ARCHITECTURE.md), and note-taking users expect undo on delete.
