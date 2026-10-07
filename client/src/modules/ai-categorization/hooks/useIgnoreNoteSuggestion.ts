import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ignoreNoteSuggestion } from "../services/noteSuggestionsService";
import { ACTIVE_NOTE_SUGGESTIONS_QUERY_KEY, NOTE_SUGGESTION_QUERY_KEY } from "./query-keys";

export function useIgnoreNoteSuggestion() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (noteId: string) => ignoreNoteSuggestion(noteId),
        onSuccess: (_data, noteId) => {
            queryClient.invalidateQueries({ queryKey: NOTE_SUGGESTION_QUERY_KEY(noteId) });
            queryClient.invalidateQueries({ queryKey: ACTIVE_NOTE_SUGGESTIONS_QUERY_KEY });
        },
    });
}
