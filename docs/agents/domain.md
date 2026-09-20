# Domain Docs

How the engineering skills should consume this repo's domain documentation when exploring the codebase.

## Before exploring, read these

- **`ARCHITECTURE.md`** at the repo root — module map, data strategy, AI categorization flow, frontend stack, deployment.
- **`GLOSSARY.md`** at the repo root — exact domain vocabulary (Note, Topic, Category, Suggestion, Module, Read Model, ...). Use these terms, don't invent synonyms.
- **`docs/adr/`** — read ADRs that touch the area you're about to work in.

This repo is single-context: one ARCHITECTURE.md/GLOSSARY.md pair and one docs/adr/ at the root (no CONTEXT-MAP.md / per-context split).

## Use the glossary's vocabulary

When your output names a domain concept (in an issue title, a refactor proposal, a hypothesis, a test name), use the term as defined in GLOSSARY.md. Don't drift to synonyms the glossary explicitly avoids.

If the concept you need isn't in the glossary yet, that's a signal: either you're inventing language the project doesn't use (reconsider) or there's a real gap worth raising.

## Flag ADR conflicts

If your output contradicts an existing ADR, surface it explicitly rather than silently overriding:

> _Contradicts ADR-0002 (in-process event communication), but worth reopening because…_
