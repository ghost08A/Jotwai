# ADR 0008: Frontend stack, styling, and note editor

## Status
Accepted

## Context
`client/` already had React, TypeScript, Vite, `react-router-dom`, `@tanstack/react-query`, and `axios` installed (found in `node_modules`, no `package.json` committed to lock it in). No styling/UI library or client-state library existed yet, and the original product requirements left "exact Note editor technology" and "exact representation of rich text" explicitly open.

## Decision
- Formalize the already-installed packages as the frontend baseline: **React + TypeScript + Vite**, **react-router-dom** (routing), **TanStack Query** (server state — fetching/caching Notes, Topics, Categories), **axios** (HTTP).
- No dedicated client-state library; `useState`/Context handles UI-only state (active filters, editor draft, sidebar state) — TanStack Query already owns server state.
- **Tailwind CSS** + shadcn/ui-style headless components for styling — fast to build a custom notepad-like UI without a heavy component library's visual opinions.
- **Tiptap** (ProseMirror) as the Note editor, configured to serialize to/from **Markdown** (the canonical storage format). Gives WYSIWYG bulleted/numbered lists and a code-block language picker while the persisted content stays plain Markdown.

## Consequences
- `package.json` needs to be (re)created to match what's actually installed, with proper scripts.
- Markdown stays the single source of truth for Note content regardless of which editor renders it — swapping the editor later doesn't require a data migration.
- No CSS-in-JS or heavy component library lock-in; Tailwind utility classes are the styling layer throughout.
