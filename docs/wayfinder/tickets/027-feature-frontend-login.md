---
id: 27
title: "Build: Frontend login flow (Identity/Auth)"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

The backend Auth API is fully built and tested (`AuthController`: register/login/refresh/logout/me, JWT access + refresh tokens with rotation — confirmed in ticket 019), but nothing on the client consumes it yet (confirmed in ticket 017: `axios` is installed but unused, no Identity module code, no login UI anywhere in `client/src`). What needs building, per `client/CLAUDE.md`'s module structure:

- `modules/identity/` — login page/form, register page/form, `services/` (raw axios calls to `/auth/*` via `core/http/httpClient`), `hooks/` (`useLogin`, `useRegister`, `useCurrentUser` wrapping those services with TanStack Query)
- Access-token storage and attaching it to outgoing requests (axios interceptor in `core/http`)
- Refresh-token flow: transparently refresh an expired access token and retry, per the rotation behavior `AuthService.RefreshAsync` already implements server-side
- Logout: call `/auth/logout`, clear local session state
- Protected Route wiring in `core/router` (per `client/CLAUDE.md`'s Project Structure) — redirect unauthenticated users to `/login`; redirect an already-authenticated user away from `/login`
- `core/pages/unauthorized.tsx` for a failed/expired session, per the system pages already named in `client/CLAUDE.md`

This is a build task, not an open decision — tracked here as backlog, same convention as tickets 020–026. Not blocked on anything: the backend API this depends on already exists and is covered by `Jotwai.IntegrationTests/TokenRefreshTests.cs` and friends.

## Finding

Not started. No login/register page, no `modules/identity/` code, and no auth-aware routing exists anywhere in `client/src` — `axios` remains an unused dependency (per ticket 017).
