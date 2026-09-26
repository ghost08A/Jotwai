import { useQuery } from "@tanstack/react-query";
import { search } from "../services/searchService";
import { searchQueryKey } from "./query-keys";

export function useSearch(q: string, topicId: string) {
    return useQuery({
        queryKey: searchQueryKey(q, topicId),
        queryFn: () => search({ q, topicId }),
    });
}
