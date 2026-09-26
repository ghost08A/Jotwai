import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ignoreSuggestion } from "../services/suggestionsService";
import { SUGGESTION_QUERY_KEY } from "./query-keys";

export function useIgnoreSuggestion() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (noteId: string) => ignoreSuggestion(noteId),
        onSuccess: (_, noteId) => queryClient.invalidateQueries({ queryKey: SUGGESTION_QUERY_KEY(noteId) }),
    });
}
