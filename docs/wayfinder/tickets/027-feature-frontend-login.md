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

## Finding (implemented, not yet verified against a live backend)

Built:

- `modules/identity/` — `types/`, `services/authService.ts` (login/register/logout/getCurrentUser), `hooks/` (`useLogin`, `useRegister`, `useLogout`, `useCurrentUser`), `components/` (`LoginPage`, `RegisterPage`)
- `core/http/session.ts` — plain (non-React) token store, localStorage-backed, with a subscribe/notify hook so React state and the axios interceptor stay in sync without a state-management library
- `core/http/httpClient.ts` — request interceptor attaches `Authorization: Bearer`; response interceptor transparently refreshes an expired access token on 401 and retries the original request once, clearing the session if the refresh itself fails
- `shared/context/AuthContext.tsx` — thin React wrapper over `core/http/session.ts` via `useSyncExternalStore`, exposing `isAuthenticated`
- `core/router/ProtectedRoute.tsx` / `PublicOnlyRoute.tsx` — unauthenticated access to `/` renders `core/pages/Unauthorized.tsx`; an authenticated user hitting `/login` or `/register` is redirected to `/`
- Backend: added a `Cors` policy (`Program.cs` + `Cors:AllowedOrigins` in `appsettings.Development.json`, currently `http://localhost:5173`) — none existed before (the gap ticket 016 flagged), so the SPA can call the API cross-origin at all

Verified in-browser (Vite dev server, live click-through): unauthenticated `/` → Unauthorized page → link to `/login`; `/login` and `/register` render and validate; submitting degrades correctly through the error-message priority order (a Vite-served 404 body first, then a real `Network Error` once `client/.env`'s `VITE_API_BASE_URL` was fixed to point at the API) — confirms the request path and error handling are wired correctly end-to-end short of a live API response.

**Not verified**: an actual register/login/refresh/logout round trip against a running API + Postgres — Docker Desktop isn't available in this environment, so the API's `identity` DB dependency couldn't be started. `dotnet build` succeeds and the CORS/route wiring was reviewed, but nobody has exercised a real 201/200 response through the new client code yet. Whoever picks this up next should run `docker-compose up` + the API + `npm run dev` and click through register → logout → login → refresh (wait out `AccessTokenMinutes` or shorten it locally) before closing this ticket.
