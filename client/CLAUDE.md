# CLAUDE.md

This file provides guidance to AI coding agents (Claude Code, Codex, etc.) when working with code in this repository

Root-wide rules (security, Definition of Done) live in
[../CLAUDE.md](../CLAUDE.md) — this file only covers what's specific to the client.

## Tech Stack

- React 19 + TypeScript, Vite
- Routing: react-router-dom
- Server state: TanStack Query (`@tanstack/react-query`)
- HTTP: axios
- Styling: Tailwind CSS v4
- Lint: oxlint (not ESLint)
- No test runner is configured yet (no Vitest/Jest in `package.json`) — see Test Runner below

## Architecture

- See [../ARCHITECTURE.md](../ARCHITECTURE.md) "Frontend" section for the full picture.
- Server state (API data) lives in TanStack Query; never duplicate server data into client-side state
- No dedicated client-state library — `useState`/React Context handles UI-only state (active filters, editor draft, sidebar state). Don't introduce Zustand/Redux/etc. without checking with the user first
- Keep components and business/data logic separate — see Project Structure rules below
- Note content is Markdown; Tiptap is a view over that content, not the source of truth (see root [CLAUDE.md](../CLAUDE.md))

## Project Structure

`src/` is currently empty aside from `assets/` — the layout below is the structure to build into, not yet a survey of what exists. Modules mirror the backend's five Modules (see [../ARCHITECTURE.md](../ARCHITECTURE.md) "Backend modules") so frontend and backend vocabulary stay aligned with [../GLOSSARY.md](../GLOSSARY.md).

```
.editorconfig
src/
    core/
        layouts/              # app layout -> public, private
        pages/                 # system pages: authorized, not-found, unauthorized
        providers/             # incl. QueryClientProvider (TanStack Query)
        query/                 # TanStack Query: queryClient config, default options, shared query keys
        router/                # route setup, Protected Route
        http/                  # axios client
    modules/                   # one folder per backend Module
        notes/
        organization/          # Topics (hierarchy) + Categories (flat)
        ai-categorization/     # Suggestion UI/state
        search/
        identity/              # login, register, session
            components/        # module component
            hooks/              # TanStack Query hooks (useQuery/useMutation) wrapping services
            services/           # API calls (axios) — no TanStack Query here
            utils/               # pure functions, for test
            types/
    shared/                    # reusable across modules
        components/            # dropdown, text-box, number-box, check-box, ...
        constants/
        routes/                 # route path constants
        context/                # shared state via React Context (light, non-global)
        hooks/
        utils/                  # pure functions, for test
        types/                  # type, class, interface
    assets/
    .env.example               # environment template only. **use a fake credential or secret
    App.tsx
    main.tsx
```

### Rules

- Never put side effects inside presentational components
- New modules added under `modules/`; a module maps 1:1 to a backend Module — don't create a frontend-only module that doesn't correspond to one
- Do not create a new abstraction for one-off usage
- Component filename MUST match its exported name
- **TanStack Query**: `services/` = raw API calls; `hooks/` = `useQuery`/`useMutation` wrapping those services. `QueryClient` config in `core/query/`, provider in `core/providers/`
- Module-scoped or cross-module UI state (that isn't server data) uses `useState`/React Context, not a global store library

## Coding Convention

### Naming

| Pattern              | Use for                                                      |
| --------------------- | ----------------------------------------------------------- |
| `PascalCase`          | Components, types, interfaces — file name matches for component files |
| `camelCase`           | Functions, variables                                          |
| `use` + `camelCase`   | Custom hooks (e.g. `useAuth`) — file name matches              |
| `kebab-case`          | Non-component file names (e.g. `date-utils.ts`)                |
| `UPPER_SNAKE_CASE`    | Module-level constant                                          |

### Coding Rules

- Keep components simple, under 200 lines **unless the complexity genuinely justifies it**
- Descriptive variable names — no abbreviations like `qty` for `quantity`
- No dead code without comment
- Comments only when intent is non-obvious
- Extract repeated logic into hooks
- Handle errors at the call site
- **State split**: server data → TanStack Query (never copy server data into local/context state); `useState`/Context only for client/UI state (filters, modals, wizard steps, session prefs)

### For Typescript

- Strict mode on, avoid `any` — prefer `unknown` + narrowing when the real type isn't known yet
- Named exports only — plays better with refactors and auto-import than default exports
- Prefer `undefined` for "no value yet" (optional props, uninitialized state — it's TS/JS's native absence); use `null` only when a value is intentionally, explicitly empty (e.g. mirrors a nullable API/DB field) — don't use the two interchangeably
- `async/await` over `.then` chains
- Avoid the non-null assertion (`!`) — narrow the type instead; `!` compiles clean but is a top cause of "compiles fine, crashes at runtime" bugs

### Editor Config

This repo enforces formatting via `.editorconfig` at the repo root. An AI agent has no
built-in mechanism to auto-load editor config files the way an IDE does — the concrete
rule is restated here so it's actually in context. Match it in every file you write or
edit; don't infer indentation by eyeballing surrounding code.

- Indent: 4 spaces (repo-wide default)
- Line endings: LF, UTF-8, final newline required
- Trailing whitespace trimmed (except Markdown, where it's meaningful for line breaks)
- Line endings are enforced by `.gitattributes` at the repo root, not by your local Git
  settings — never "fix" them by hand, and never change `core.autocrlf` to work around a
  complaint from a formatter
- `.vscode/settings.json` is committed and pins the same rules for the editor. It is not
  personal configuration — do not add themes, fonts, or machine-specific paths to it, and
  do not relax its formatting keys to match a file that is already wrong

If a formatter reports a line-ending or whitespace failure across files you did not
touch, that is a repository configuration problem, not a code problem. Report it and
leave it alone. Do not answer it with a repo-wide reformat, and never let unrelated
reformatted files ride along in a change.

### Formatting Display

- Date: `YYYY-MM-DD`
- DateTime: `YYYY-MM-DD HH:mm:ss`
- Time only: `HH:mm` / `HH:mm:ss`

### UI & Design System

- Tailwind utility classes only — no hand-written CSS rules, no inline `style`
- Design tokens are declared once (Tailwind v4: `@theme {}`) so Tailwind generates
  utilities for them, then consumed as utilities: `text-ink`, `hover:bg-ink`,
  `rounded-card`. Never reach for the raw variable — no `var(--color-ink)`, no
  `text-[var(--color-ink)]`. A token with no utility is a token in the wrong place
- Every interactive element needs `hover`, `focus`, `active`, `disabled` state
- Clickable element add class `cursor-pointer` and `cursor-not-allowed` when disabled
- Forms must be scannable and mobile-friendly
- Meet WCAG 2.1 AA for contrast and color blindness
- Support keyboard navigation
- CTA button: Solid primary only — no ghost buttons for main action
- **Transitions**: use the most specific transition class. Avoid `transition-all`. If only color is animating, omit the transition entirely — do not use `transition-colors`

#### Rules

- **Imports**: for a library with a heavy barrel export, import from the specific file, not the package root — keeps bundles tree-shakeable:
  ```ts
  // ✓
  import Button from "@mui/material/Button";
  // ✗
  import { Button } from "@mui/material";
  ```
- **Error handling**: in `catch` blocks, extract a message in priority order — API error body, then `Error.message`, then a fixed fallback string. Use `axios.isAxiosError()` to narrow, not a manual cast:

  ```ts
  function getErrorMessage(err: unknown, fallback = "An error occurred"): string {
    if (axios.isAxiosError(err)) {
      return err.response?.data?.message ?? err.message ?? fallback;
    }
    if (err instanceof Error) {
      return err.message;
    }
    return fallback;
  }

  try {
    await saveNote(note);
  } catch (err) {
    const message = getErrorMessage(err, "Failed to save note");
    toast.error(message);
  }
  ```

### Comment Code

- `// Note:` — allowed for context that aids understanding
- `// TODO:` — if encountered **in files you are editing**, flag it to the user before proceeding

### Before Creating New Code

Before creating a new component, hook, util, or type — search whether an equivalent
implementation already exists and reuse it whenever possible. Before introducing any new
type, ask the user to confirm its name unless the name is explicitly specified in the
requirement.

### Loading Indicator

Classify by what's on screen, not by how long a call might take — an agent can always
tell from the markup/template whether something is static, fetched, or a triggered
action; it can never know a call's real-world latency ahead of time.

| UI pattern                                                                                                    | Treatment                                                       |
| ------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------- |
| Static text / content that never depends on a fetch                                                                 | No indicator                                                           |
| View is populated by fetched data — lists, detail pages, dropdown options, any `[label]: [input]` field backed by an API call | Skeleton UI                                                            |
| User-triggered action — submit, save, delete, any button that mutates data                                           | Spinner — disable the control and show the spinner inline/on the button |

#### Rules

- A dropdown/select whose options come from an API counts as fetched data even though it's a small control — skeleton it, don't treat it as static just because it's compact
- Use a full-screen/backdrop spinner only when the action blocks the entire view (e.g. a multi-step wizard submit) — otherwise keep the spinner scoped to the triggering control
- If a similar case already has a loading treatment elsewhere in the repo, match it — consistency beats re-deriving the pattern from scratch

### Commands

```bash
npm run dev          # Start dev server (Vite)
npm run build         # TypeScript check + Vite build
npm run preview       # Preview production build
npm run lint          # oxlint check
```

## Testing & Quality

Before marking task complete:

1. Run `npm run lint` — fix all oxlint errors
2. Run `npm run build` — fix all TypeScript + build errors
3. Run the test suite once one is configured (see Test Runner below)

### Test Runner

Not configured yet — `package.json` has no Vitest/Jest and no `test` script. When a test
runner is needed, set one up (Vitest is the natural fit for a Vite project) and update
this section rather than leaving tests untracked.

### Unit Test Rules

Unit test required for:

- Calculation logic
- Form validation

Do not write unit tests for:

- Presentational components with no logic
- Third-party library behavior

To test logic embedded in a component, extract it into a pure function (e.g. `/utils/`).

### Rules

- **DO NOT edit a test to make a failure pass.** When a test breaks after a code change, stop and report which tests failed and why — decide whether the _code_ regressed or the expected behavior genuinely changed, summarize the impact, and wait for explicit approval before touching any test. Do not assume the test is wrong just because it is red.
