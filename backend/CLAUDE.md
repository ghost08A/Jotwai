# CLAUDE.md

This file provides guidance to AI coding agents (Claude Code, Codex, etc.) when working with code in this repository

**Frontend App:** see [client/CLAUDE.md](../client/CLAUDE.md)

Root-wide rules (security, Definition of Done) live in
[../CLAUDE.md](../CLAUDE.md) — this file only covers what's specific to the backend.

## Tech Stack

- .NET 10, ASP.NET Core Web API (C#)
- Entity Framework Core + Npgsql.EntityFrameworkCore.PostgreSQL
- ASP.NET Core Identity (`Microsoft.Extensions.Identity.Core`) + JWT bearer auth (`Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`)
- Swashbuckle (Swagger/OpenAPI)

## Architecture

- See [../ARCHITECTURE.md](../ARCHITECTURE.md) and [ADR-0010](../docs/adr/0010-global-three-tier-layering.md) for the full picture.
- Business logic belongs only in `Jotwai.Service`.
- Controllers (`Web.Jotwai`) should only orchestrate requests and responses.
- Do not access a `DbContext` directly from a Controller.
- Reuse existing services before creating new ones.
- Keep the `Domain` → `Service` → `Web` dependency direction unchanged.
- Modules (Notes, Organization, AI Categorization, Search, Identity) communicate only via in-process Domain Events, by convention — never direct calls or cross-schema queries between modules (ADR 0001–0003, ADR-0010).
- Cross-module reads go through a Module's own denormalized Read Model, kept current by subscribing to Domain Events — never a SQL join across another Module's schema, even though all schemas live in one PostgreSQL database.

## Project Structure

One **global** Domain → Service → Web layering (ADR-0010, superseding ADR-0009's per-Module layering) — folders are organized by technical layer, not per-Module:

```
.editorconfig
Jotwai.Domain/              # no project dependencies
    Interfaces/              # I-prefixed interfaces for all Modules (e.g. IAuthService)
    Models/                  # entities/domain models
    RequestModels/           # request DTOs
    ValidateModel/           # validation/result models
    ViewModels/               # response/view models
Jotwai.Service/              # references Jotwai.Domain only
    ImplementServices/        # business logic (one *Service per interface)
    Helper/
    Configurations/           # EF Core IEntityTypeConfiguration<T> per entity
    Migrations/               # EF Core migrations
    <Module>DbContext.cs      # one DbContext per Module, scoped to its own schema
    <Module>DbContextFactory.cs
    <Module>ServiceRegistration.cs
Web.Jotwai/                  # references Jotwai.Service + Jotwai.Domain
    Controllers/
    Properties/
    Program.cs
Jotwai.IntegrationTests/     # XUnit + Testcontainers.PostgreSql + WebApplicationFactory
```

### Rules

- Keep the `Domain` / `Service` / `Web` dependency direction unchanged — `Web.Jotwai`
  depends on `Jotwai.Service` and `Jotwai.Domain`; `Jotwai.Service` depends on
  `Jotwai.Domain` only; `Jotwai.Domain` depends on nothing project-specific.
- New code goes into the matching technical-layer folder (`Interfaces/`, `Models/`,
  `RequestModels/`, `ValidateModel/`, `ViewModels/` in `Jotwai.Domain`;
  `ImplementServices/`, `Configurations/` in `Jotwai.Service`) — the Module boundary is
  enforced by the Domain Events convention and code review, not by folder structure, so
  don't invent per-Module subfolders.
- Each Module gets its own `DbContext` (e.g. `IdentityDbContext`) scoped to its own
  PostgreSQL schema — never a shared `AppDbContext`.

## Coding Convention

### Naming

| Pattern            | Use for                                                 |
| ------------------- | -------------------------------------------------------- |
| `PascalCase`         | Classes, methods, properties, public fields, constants     |
| `camelCase`          | Local variables, method parameters                          |
| `_camelCase`         | Private fields                                               |
| `IPascalCase`        | Interfaces (e.g. `IAuthService`)                             |
| `Async` suffix        | Async methods (e.g. `RegisterAsync`, `LoginAsync`)             |
| `Service` suffix      | Service implementations (e.g. `AuthService`)                  |
| `Is`/`Has`/`Can` prefix | Boolean properties/variables (e.g. `IsActive`, `HasRefreshToken`) |

Prefer existing Jotwai terminology from [../GLOSSARY.md](../GLOSSARY.md) over any other naming source.

### Coding Rules

- Handle null values safely
- Do not swallow exceptions silently
- Keep methods focused and small
- Avoid duplicated logic. Extract helper methods only when reuse or clarity is improved
- Prefer async/await for I/O operations
- Do not use `.Result` or `.Wait()`
- Log via the injected `ILogger<T>` — never `Console.WriteLine`
- Register services with the correct DI lifetime (`Scoped` for request-bound state, `Singleton` for stateless/shared, `Transient` sparingly) — never resolve a `Scoped` service from a `Singleton`

### Don't over-engineer

Build only what the current ticket requires. Do not create:

- A shared kernel, generic repositories, or repositories at all, without a concrete need
- Unnecessary base classes or abstractions
- Interfaces with only one implementation and no reason to expect a second
- Event handlers or subscriptions for events nothing publishes yet
- A feature's `DbContext`/schema before that feature exists
- Authentication/authorization abstractions beyond what the current feature needs

### API Rules

- Keep API response format consistent with existing endpoints
- DO NOT change route names, request models, or response models unless required
- Return meaningful error messages without exposing sensitive internal details

### Database / EF Core Rules

- DO NOT modify database schema unless explicitly requested
- DO NOT rename EF-generated entity classes or properties
- Use existing DbContext and entity configuration patterns (`IEntityTypeConfiguration<T>` in `Configurations/`)
- Avoid N+1 queries
- Use `AsNoTracking()` for read-only queries
- Use `Include()` only when necessary
- Avoid loading unused navigation properties
- Generate/verify an EF Core migration for any model change

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
- `.csproj`/`.props`/`.targets` use 2 spaces instead of the 4-space default

If a formatter reports a line-ending or whitespace failure across files you did not
touch, that is a repository configuration problem, not a code problem. Report it and
leave it alone. Do not answer it with a repo-wide reformat, and never let unrelated
reformatted files ride along in a change.

### Formatting Display

- Date: `YYYY-MM-DD`
- DateTime: `YYYY-MM-DD HH:mm:ss`
- Time only: `HH:mm` / `HH:mm:ss`
- Money / amounts stored as `decimal` — never `float`/`double` (not currently used by any Module, but the rule stands if one is ever added)

### Comment Code

- `// Note:` — allowed for context that aids understanding
- `// TODO:` — if encountered **in files you are editing**, flag it to the user before proceeding

### Before Creating New Code

Before creating a new helper, extension, service, DTO, ViewModel, or interface — search
whether an equivalent implementation already exists and reuse it whenever possible.
Before introducing any new type, ask the user to confirm its name unless the name is
explicitly specified in the requirement.

### Commands

```bash
dotnet restore
dotnet build
dotnet test
dotnet format --verify-no-changes   # enforces .editorconfig; must pass in CI too
```

## Testing & Quality

Before marking a task complete:

1. Run build command
2. Fix all compile errors
3. Check affected flows manually
4. Add or update tests when business logic changes
5. Start the Postgres dependency (`docker-compose.yml`) if the change touches persistence

### Test Runner

- XUnit, via `Jotwai.IntegrationTests`
- Tests run against **Testcontainers.PostgreSql** — never a native/local Postgres instance
- `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) drives the API in-process — see `JotwaiApiFactory.cs`
- No unit-test project or mocking library exists yet; the current suite is integration-only. If pure business logic grows complex enough to warrant isolated unit tests, set up a separate unit-test project rather than mixing unit tests into `Jotwai.IntegrationTests`

### Unit Test Rules

Test required for:

- Business logic
- Validation
- Permission / auth flows
- Bug fixes

#### Test Style

- Use Arrange / Act / Assert
- Use clear test names
- Test both success and failure cases
- DO NOT test private methods directly
- Prefer testing public behavior

### Rules

- **DO NOT edit a test to make a failure pass.** When a test breaks after a code change, stop and report which tests failed and why — decide whether the _code_ regressed or the expected behavior genuinely changed, summarize the impact, and wait for explicit approval before touching any test. Do not assume the test is wrong just because it is red.
