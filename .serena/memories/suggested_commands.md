# Windows commands
- Work from repo root `D:\Jotwai`.
- Backend solution/build entrypoint is under `backend\` (`Jotwai.sln`); use `dotnet build` there for full backend validation.
- Integration tests live in `backend\Jotwai.IntegrationTests`; persistence-related tests require Docker/Testcontainers PostgreSQL, not a native local PostgreSQL installation.
- Frontend commands depend on the scripts in `client\package.json`; inspect that file before running npm commands because the repository history notes package-lock/package metadata may evolve.
- For repository inspection on Windows, prefer `dir`, `type`, `findstr`, or the Jodwai file-reading/search tools rather than assuming PowerShell commands are available; Jodwai shell execution may use cmd semantics.