import { useMutation, useQueryClient } from "@tanstack/react-query";
import { acceptNoteSuggestion } from "../services/noteSuggestionsService";
import { ACTIVE_NOTE_SUGGESTIONS_QUERY_KEY, NOTE_SUGGESTION_QUERY_KEY } from "./query-keys";
import { NOTES_QUERY_KEY } from "../../notes/hooks/query-keys";

export function useAcceptNoteSuggestion() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (noteId: string) => acceptNoteSuggestion(noteId),
        onSettled: (_data, _error, noteId) => {
            queryClient.invalidateQueries({ queryKey: NOTE_SUGGESTION_QUERY_KEY(noteId) });
            queryClient.invalidateQueries({ queryKey: ACTIVE_NOTE_SUGGESTIONS_QUERY_KEY });
            // Accepting moves the Note under its new parent.
            queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY });
        },
    });
}
