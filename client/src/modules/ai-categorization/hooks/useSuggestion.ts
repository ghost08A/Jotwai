import { useQuery } from "@tanstack/react-query";
import { getSuggestion } from "../services/suggestionsService";
import { SUGGESTION_QUERY_KEY } from "./query-keys";

export function useSuggestion(noteId: string, refetchInterval: number | false = false) {
    return useQuery({
        queryKey: SUGGESTION_QUERY_KEY(noteId),
        queryFn: () => getSuggestion(noteId),
        refetchInterval,
    });
}
