---
id: 19
title: "Audit implementation: ADR-0011 Custom Identity entities with ASP.NET Core Identity password hashing"
type: wayfinder:research
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/19
---

## Question

Does the current codebase implement the decision recorded in `docs/adr/0011-custom-identity-entities-with-password-hashing.md`? Search for Identity-related entities, password hashing usage, JWT issuance code, refresh-token rotation logic, and an Auth/Identity controller.

## Finding (implemented)

Entities are plain POCOs (`User`, `RefreshToken` — not `IdentityUser`), matching the ADR's decision against using framework Identity entities directly. Password hashing uses `IPasswordHasher<User>` only (not `UserManager`/`SignInManager`), with a constant-time dummy-hash check on unknown emails to prevent timing leaks, plus `PasswordPolicy.Validate`. JWT access tokens are hand-issued (`TokenService.GenerateAccessToken`, HMAC-SHA256), and refresh-token rotation (`AuthService.RefreshAsync`) uses an atomic `ExecuteUpdateAsync` compare-and-swap to revoke-on-use, blocking replay of a rotated token. `AuthController` exposes register/login/refresh/logout/me. Covered by `Jotwai.IntegrationTests/TokenRefreshTests.cs`.

No gaps found against this ADR's actual scope.
