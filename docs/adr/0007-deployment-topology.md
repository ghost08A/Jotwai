# ADR 0007: API-only backend, separately deployed static SPA

## Status
Accepted

## Context
The repo already scaffolds `client/` and `server/` as separate top-level folders. The product needs to work "everywhere the user logs in" (web now, mobile later per the requirements grilling), which means the client is a distinct artifact from the backend regardless.

## Decision
The ASP.NET Core backend serves only the JSON API. The React SPA is built and deployed as a separate static artifact (static hosting/CDN), calling the API over HTTP. The backend never serves the SPA's static files.

## Consequences
- Frontend and backend scale, cache, and deploy independently.
- CORS must be configured on the API for the SPA's origin(s).
- This also sets up cleanly for the "mobile later" direction — a future mobile client is just another consumer of the same API, no backend change needed.
