# ADR 0011: Custom Identity entities with ASP.NET Core Identity password hashing (supersedes ADR-0005)

## Status
Accepted. Supersedes ADR-0005.

## Context
ADR-0005 named "ASP.NET Core Identity" as the auth mechanism, which reads as adopting the full framework — `IdentityUser`, `IdentityDbContext<TUser>`, `UserManager`, `SignInManager`. What was actually built for the Identity Module (Issue #2, #3) is narrower: plain `User`/`RefreshToken` entities in `Jotwai.Domain`, a hand-written `IdentityDbContext : DbContext` (not `IdentityDbContext<TUser>`), and JWT issuance/validation written directly in `TokenService`/`AuthService`. The framework's password-hashing (`IPasswordHasher<User>`) and its default password-policy rules are reused, but nothing else from ASP.NET Core Identity is wired up. ADR-0005 should describe what's actually in the codebase rather than the broader framework name.

## Decision
Keep custom `User`/`RefreshToken` entities and a hand-written `IdentityDbContext`, not ASP.NET Core Identity's `IdentityUser`/`IdentityDbContext<TUser>`/`UserManager`/`SignInManager`. Reuse only two pieces of ASP.NET Core Identity: `IPasswordHasher<User>` for hashing/verifying passwords, and its default password-policy validation (via `PasswordPolicy`). Account/session state (email, password hash, refresh tokens) is modeled and persisted entirely by the Identity Module's own entities and `DbContext`, scoped to the `identity` schema (ADR-0003).

Everything else ADR-0005 decided is unchanged: JWT access tokens plus refresh tokens, the Identity Module owns credentials entirely, refresh-token rotation/expiry are implementation details, and OAuth remains a fast-follow.

## Consequences
- ADR-0005's "ASP.NET Core Identity" framing is corrected to "ASP.NET Core Identity's password hasher + password policy," which matches the shipped code (`Jotwai.Domain/Models/User.cs`, `Jotwai.Domain/Models/RefreshToken.cs`, `Jotwai.Service/IdentityDbContext.cs`).
- No `UserManager`/`SignInManager`/ASP.NET Core Identity claims-and-roles system exists or is planned; if roles/claims are ever needed, that is a new decision, not something already covered by "ASP.NET Core Identity" as named in ADR-0005.
- Keeping custom entities (rather than `IdentityUser`) keeps the `identity` schema fully owned and shaped by this Module, consistent with ADR-0003's schema-per-module isolation — no framework-owned tables outside that shape.
