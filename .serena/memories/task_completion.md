# Definition of done
- Follow root and path-specific CLAUDE.md rules before editing.
- Backend: build the whole solution; zero errors and warnings expected; when EF model changes, generate/verify migrations; persistence changes require Docker PostgreSQL; run integration tests with Testcontainers.
- Review `git diff` for accidental/obsolete files and unrelated changes.
- Verify requested behavior and existing behavior; do not close/resolve work until implementation and tests are actually complete.
- Do not create a git commit unless the user explicitly authorizes it after review.