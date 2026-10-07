import { NOTES_QUERY_KEY } from "../../notes/hooks/query-keys";

// Nested under the notes list key on purpose: every note mutation already invalidates
// NOTES_QUERY_KEY, and invalidation matches by prefix, so cached search results refresh too.
// Category selection order shouldn't fragment the cache - sort before keying.
export const searchQueryKey = (q: string, categoryIds: string[], parentId?: string) =>
    [...NOTES_QUERY_KEY, "search", q, [...categoryIds].sort(), parentId ?? null] as const;
