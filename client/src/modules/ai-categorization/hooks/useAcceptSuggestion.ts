import { useMutation, useQueryClient } from "@tanstack/react-query";
import { acceptSuggestion } from "../services/suggestionsService";
import { SUGGESTION_QUERY_KEY } from "./query-keys";
import { NOTES_QUERY_KEY } from "../../notes/hooks/query-keys";
import { CATEGORIES_QUERY_KEY } from "../../organization/hooks/query-keys";
import type { AcceptSuggestionRequest } from "../types";

export function useAcceptSuggestion() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ noteId, request }: { noteId: string; request: AcceptSuggestionRequest }) =>
            acceptSuggestion(noteId, request),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY });
            queryClient.invalidateQueries({ queryKey: CATEGORIES_QUERY_KEY });
        },
        // Invalidate on failure too: a rejected accept (e.g. stale candidate ids from a
        // suggestion that got replaced by a concurrent on-demand trigger) means the
        // backend's suggestion state already moved on - refetch so the panel shows what's
        // actually active now instead of leaving the user stuck on stale candidates.
        onSettled: (_data, _error, { noteId }) => {
            queryClient.invalidateQueries({ queryKey: SUGGESTION_QUERY_KEY(noteId) });
        },
    });
}
