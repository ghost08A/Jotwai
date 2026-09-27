import { useQuery } from "@tanstack/react-query";
import { search } from "../services/searchService";
import { searchQueryKey } from "./query-keys";

// Skips the request entirely when there's no filter active - the home topic
// browser shows the full, unfiltered tree in that case (no note search needed).
export function useSearch(q: string, categoryIds: string[]) {
    return useQuery({
        queryKey: searchQueryKey(q, categoryIds),
        queryFn: () => search({ q, categoryIds }),
        enabled: q.trim() !== "" || categoryIds.length > 0,
    });
}
