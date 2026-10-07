import { useQuery } from "@tanstack/react-query";
import { search } from "../services/searchService";
import { searchQueryKey } from "./query-keys";

// Skips the request entirely when there's no filter active - the callers show their full,
// unfiltered list in that case (no note search needed).
export function useSearch(q: string, categoryIds: string[], parentId?: string) {
    return useQuery({
        queryKey: searchQueryKey(q, categoryIds, parentId),
        queryFn: () => search({ q, categoryIds, parentId }),
        enabled: q.trim() !== "" || categoryIds.length > 0 || parentId !== undefined,
    });
}
