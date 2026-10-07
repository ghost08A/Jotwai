import { useQuery } from "@tanstack/react-query";
import { listActiveNoteSuggestionIds } from "../services/noteSuggestionsService";
import { ACTIVE_NOTE_SUGGESTIONS_QUERY_KEY } from "./query-keys";

const POLL_INTERVAL_MS = 5000;

// One call for the whole Notes page instead of one per row. A suggestion is produced by a
// background worker a few seconds after a Note is created, so this keeps checking while the
// page is open (react-query pauses the polling while the tab is hidden).
export function useNotesWithSuggestions() {
    return useQuery({
        queryKey: ACTIVE_NOTE_SUGGESTIONS_QUERY_KEY,
        queryFn: listActiveNoteSuggestionIds,
        refetchInterval: POLL_INTERVAL_MS,
    });
}
