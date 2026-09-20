# Identity: Account Creation & Login

> Published as [ghost08A/Jotwai#1](https://github.com/ghost08A/Jotwai/issues/1) (label `ready-for-agent`). This file is kept as the local source of truth alongside the issue.

## Problem Statement

Jotwai notes need to sync across every device the user logs into (per the product requirements: capture from anywhere, organize anywhere). Right now there is no way for a person to identify themselves to the system, so there is nothing to attach a Note, Topic, or Category to, and no way for the same person's data to follow them from one device to another.

## Solution

A user can create an account with an email and password, and log in from any device to get access to their own data. Once logged in, they stay logged in across a session without re-entering credentials on every request, and can explicitly log out to end that session on a given device.

## User Stories

1. As a new user, I want to create an account with my email and a password, so that I have a place my Notes belong to.
2. As a new user, I want a clear error if I try to register with an email that's already taken, so that I understand why registration failed.
3. As a new user, I want a clear error if my chosen password doesn't meet the minimum requirements, so that I know what to fix.
4. As a new user, I want to be automatically logged in right after registering, so that I can start capturing notes immediately without a separate login step.
5. As a returning user, I want to log in with my email and password from any device, so that I can access my notes wherever I am.
6. As a returning user, I want a clear, generic error when my email or password is wrong, so that I know to retry without being told which of the two was incorrect (avoids leaking which emails are registered).
7. As a logged-in user, I want my session to stay active without re-entering my password on every action, so that using the app isn't a constant interruption.
8. As a logged-in user, I want my session to eventually refresh itself transparently in the background, so that I don't get logged out just because I left the app open for a while.
9. As a logged-in user, I want to be able to log out explicitly on a device, so that I can end my session there (e.g. on a shared or borrowed computer).
10. As a logged-in user on multiple devices, I want logging out on one device to not log me out everywhere else, so that my other active sessions aren't disrupted.
11. As any client application (web now, mobile later), I want a standard way to prove a request comes from a logged-in user, so that every other module (Notes, Organization, AI Categorization, Search) can trust the identity attached to a request.
12. As a developer of another module, I want a stable way to get "the current user's ID" from an authenticated request, so that Notes/Topics/Categories can be scoped to their owner without each module reimplementing auth.
13. As a user, I want my password to never be stored or logged in plain text anywhere, so that a data breach doesn't expose my actual password.
14. As a user, I want an expired or invalid session to be rejected with a clear "please log in again" response, so that the app can prompt me appropriately instead of failing silently or with a confusing error.
15. As a security-conscious system, I want a leaked or stolen refresh token to become useless after use (rotation) or after logout, so that token theft has a limited blast radius.

## Implementation Decisions

- **Module**: this spec is entirely within the **Identity** module (ADR-0001, ADR-0005). No other module is modified — Notes/Organization/AI Categorization/Search will consume "current user ID" via the standard ASP.NET Core auth pipeline in their own future specs, not via any direct dependency on Identity's internals.
- **Identity provider**: ASP.NET Core Identity manages user records (email as the username, password hashing, uniqueness). Email is the account identifier; no separate username field.
- **Auth mechanism**: JWT access tokens + refresh tokens (ADR-0005).
  - **Access token**: short-lived (target: 15 minutes), carries the user ID as a claim, used as a `Bearer` token on every API request to any module.
  - **Refresh token**: longer-lived (target: 30 days), opaque, stored server-side (hashed) in the Identity module's schema with an expiry and a revoked flag. Used only to obtain a new access token.
  - **Refresh token rotation**: every refresh issues a new refresh token and revokes the old one. A revoked or already-used refresh token is rejected outright, treated as a signal of possible token theft.
- **Endpoints** (Identity module's public HTTP API — the seam other modules and the client depend on):
  - `POST /api/auth/register` — `{ email, password }` → creates the account, then behaves like a successful login (returns access + refresh tokens immediately; see User Story 4).
  - `POST /api/auth/login` — `{ email, password }` → `{ accessToken, refreshToken, expiresIn }` on success; a generic "invalid credentials" error on failure (never reveals whether the email exists).
  - `POST /api/auth/refresh` — `{ refreshToken }` → new `{ accessToken, refreshToken, expiresIn }`, rotating the refresh token.
  - `POST /api/auth/logout` — `{ refreshToken }` → revokes that specific refresh token only (other devices/sessions are unaffected, per User Story 10). Idempotent: logging out an already-revoked token is not an error.
- **Schema** (owned exclusively by the Identity module, `identity` schema — ADR-0003): ASP.NET Core Identity's standard user tables, plus a `RefreshTokens` table (token hash, user ID, expiry, revoked flag, created/rotated timestamps).
- **Password policy**: ASP.NET Core Identity's default complexity rules as the starting point (adjustable later without a spec if requirements change materially).
- **OAuth (Google/GitHub, etc.)**: explicitly out of scope for this spec — noted as a fast-follow in ADR-0005, tracked as a separate future spec.

## Testing Decisions

- **Seam**: integration tests against the Identity module's public HTTP API (the four endpoints above), run through the ASP.NET Core test host (`WebApplicationFactory`) against a real PostgreSQL instance (Testcontainers) — not against mocked repositories or hand-unit-tested internal classes. This is the **first** test seam in the codebase; it establishes the pattern every other module's spec should follow: test the module through its public API, not its internals.
- **What "external behavior" means here**: given a request to one of the four endpoints, assert on the HTTP status code and response shape (tokens present/absent, error shape), and on externally-observable side effects (a second login attempt with the same email fails; a rotated refresh token becomes unusable; a logged-out refresh token becomes unusable) — never on internal method calls or how ASP.NET Core Identity stores a password hash internally.
- **Coverage for this spec** (via that one seam):
  - Register: success path, duplicate email, weak password.
  - Login: success path, wrong password, unknown email (identical error shape to wrong password).
  - Refresh: success path (old refresh token becomes invalid after rotation), expired refresh token rejected, already-used/revoked refresh token rejected.
  - Logout: success path, logging out one device doesn't affect another device's active refresh token, double-logout is not an error.
  - A protected dummy/health endpoint (or the refresh endpoint's own auth requirement, if applicable) rejects requests with no token, an expired token, and a malformed token.
- **Prior art**: none yet in this codebase — this spec's tests are the first, and should be treated as the reference pattern (Testcontainers Postgres + `WebApplicationFactory`) for the Notes, Organization, AI Categorization, and Search modules' own specs.

## Out of Scope

- OAuth / social login (Google, GitHub, etc.) — future fast-follow per ADR-0005.
- Password reset / "forgot password" flow — not covered by this spec; needed before general release but not blocking the core capture-and-sync loop.
- Email verification on registration.
- Account deletion / data export.
- Multi-factor authentication.
- Rate limiting / brute-force protection on login (worth a security-focused follow-up before production, not this spec).
- Any change to Notes, Organization, AI Categorization, or Search — they will consume "current user ID" from this module's tokens in their own future specs, via the standard ASP.NET Core auth pipeline, not via changes made here.

## Further Notes

- This is the first spec for the project and establishes two things every later module spec should reuse: the **testing seam pattern** (public API + Testcontainers, described above) and the **auth contract** (`Bearer` JWT with a user-ID claim) that Notes/Organization/AI Categorization/Search will all depend on to scope data per user.
- Per `docs/agents/issue-tracker.md`, this should be published as a GitHub issue on `ghost08A/Jotwai` with the `ready-for-agent` label once the `gh` CLI is available in this environment (not currently installed here).
