---
id: 9
title: Decide AI behavior when no Category is confident enough
type: wayfinder:grilling
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/9
---

## Question

ARCHITECTURE.md's AI Categorization flow always "produces a ranked Suggestion", but doesn't cover the case where a Note is too short/ambiguous for any candidate Category to be worth showing. Should the system (a) always show a best-effort suggestion regardless of confidence, (b) show an explicit empty suggestion set, or (c) use a confidence threshold below which nothing is shown at all?

## Resolution

**Decision**: (c) — use a confidence threshold. If AI Categorization's best candidate confidence is below a configurable threshold, no Suggestion is surfaced to the user at all (no empty-set UI state to build).

Rationale: avoids low-value suggestions cluttering the UI without needing new UI for "no suggestion".
