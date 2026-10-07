import { useQuery } from "@tanstack/react-query";
import { getNoteSuggestion } from "../services/noteSuggestionsService";
import { NOTE_SUGGESTION_QUERY_KEY } from "./query-keys";

export function useNoteSuggestion(noteId: string) {
    return useQuery({
        queryKey: NOTE_SUGGESTION_QUERY_KEY(noteId),
        queryFn: () => getNoteSuggestion(noteId),
    });
}
