# ADR 0006: Postgres substring search, no external search engine

## Status
Accepted

## Context
The product requirement is explicit: case-insensitive substring match over Note content, Topic, and Category for MVP — no LLM semantic search, vector database, or embeddings (BR-014, BR-015, BR-016, and the "Search Scope — MVP" section of the requirements). Standing up Elasticsearch/Meilisearch for this would be infrastructure the requirement doesn't call for.

## Decision
Implement search as Postgres `ILIKE` substring queries against the Search Module's own read model (ADR 0003), backed by a `pg_trgm` trigram GIN index for acceptable performance at scale. Code block bodies count as searchable content; a code block's declared language is searchable metadata. Filtering by a parent Topic includes all descendant Topics.

## Consequences
- No new infrastructure to run or operate.
- Search quality is limited to substring matching — no ranking by relevance, no typo tolerance, no semantic understanding. This is an accepted MVP limitation, explicitly deferred as a future feature by the requirements.
- If search needs change later (ranking, fuzzy match, semantic search), the read-model boundary (ADR 0003) means only the Search Module's internals change, not its event-driven inputs.
