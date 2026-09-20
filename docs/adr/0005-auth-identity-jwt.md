# ADR 0005: ASP.NET Core Identity with JWT tokens

## Status
Accepted

## Context
Jotwai needs accounts and login to support cross-device sync (a core product requirement) — notes must be reachable from any device the user logs into. The client is a separately-deployed SPA calling a JSON API (ADR 0007), which rules out cookie/session auth tied to server-rendered pages.

## Decision
Use ASP.NET Core Identity for user/account management (email/password for MVP, OAuth providers as a fast-follow, not MVP), issuing JWT access tokens plus refresh tokens. The SPA attaches the access token to API requests.

## Consequences
- Standard, well-supported pattern for SPA-calls-API architectures; the Identity Module (ADR 0001) owns this entirely and no other Module touches credentials directly.
- Refresh token storage/rotation and token expiry policy are implementation details to settle when the Identity Module is built, not architectural decisions.
- Sync conflict handling across devices is out of scope for auth itself — see ARCHITECTURE.md: last-write-wins, no merge UI, for concurrent edits to the same Note.
