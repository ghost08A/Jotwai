---
id: 10
title: Decide bulk-accept semantics for Category suggestions
type: wayfinder:grilling
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/10
---

## Question

GLOSSARY.md says the user "may accept any subset" of a Suggestion's candidate Categories. When the user accepts several at once — some already-existing Categories, some brand-new AI-proposed names — is this one atomic transaction (create + attach all, succeed or fail together), or can each Category be accepted/attached independently?

## Resolution

**Decision**: One atomic transaction. Accepting a subset of a Suggestion's candidate Categories — whether pre-existing or brand-new AI-proposed names — creates any new Categories and attaches all accepted Categories to the Note as a single transaction: succeed or fail together, no partial/intermediate state.

Rationale: consistent with the "Save Note immediately" pattern of treating a user action as one atomic step.
