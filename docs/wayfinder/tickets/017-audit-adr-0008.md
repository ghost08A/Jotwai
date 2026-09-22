---
id: 17
title: "Audit implementation: ADR-0008 Frontend stack, styling, and note editor"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/17
---

## Question

Does the current codebase implement the decision recorded in `docs/adr/0008-frontend-stack-and-editor.md`? Read `client/package.json` for dependencies and check `client/src` for actual app code, routing, or editor component.

## Finding (partially implemented)

Installed and used: React 19, Vite, react-router-dom, TanStack Query, Tailwind — `main.tsx` wires up `BrowserRouter` + `QueryClientProvider`, `App.tsx` has one placeholder route. `axios` is a dependency but unused (no API client/hooks reference it yet).

**Missing entirely**: Tiptap (no `@tiptap/*` packages anywhere), any shadcn/ui-style component library (just raw Tailwind classes so far), any Note/Topic/Category feature code, any real data-fetching hooks, any editor.

Left open — implementation debt, not a re-litigated decision. This is expected: only scaffolding exists on the frontend so far.
