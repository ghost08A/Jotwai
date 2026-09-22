# Backend tickets

Checklist for agents implementing backend (`backend/`) tickets, on top of the architecture in ADR-0001–0003, ADR-0010, ADR-0011.

## Don't over-engineer

Build only what the current ticket requires. Do not create:

- A shared kernel, generic repositories, or repositories at all, without a concrete need
- Unnecessary base classes or abstractions
- Interfaces with only one implementation and no reason to expect a second
- Event handlers or subscriptions for events nothing publishes yet
- A feature's `DbContext`/schema before that feature exists
- Authentication/authorization abstractions beyond what the current feature needs

## Naming

- Classes, methods: `PascalCase`
- Variables: `camelCase`
- Interfaces: `I` prefix (`IAuthService`)
- Service implementations: `Service` suffix (`AuthService`)
- Async methods: `Async` suffix (`RegisterAsync`, `LoginAsync`)
- Boolean properties/variables: `Is`/`Has`/`Can` prefix (`IsActive`, `HasRefreshToken`)

Prefer existing Jotwai terminology from [`GLOSSARY.md`](../../GLOSSARY.md) over any other naming source.

## Before calling a change done

1. Check project references match the dependency direction in ADR-0010 (`Web.Jotwai` → `Jotwai.Service` → `Jotwai.Domain`; `Jotwai.Domain` → nothing).
2. Build the whole solution; zero warnings, zero errors.
3. Generate/verify EF Core migrations for any model change.
4. Start the Postgres dependency (`docker-compose.yml`) if the change touches persistence.
5. Run the integration test suite — it must run against **Testcontainers**, never a native/local Postgres instance.
6. Review `git diff` for stale files left over from a prior structure or approach.
7. Don't close the GitHub issue until the implementation and tests are actually complete.
