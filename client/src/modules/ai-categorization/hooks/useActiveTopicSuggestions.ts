import { useQuery } from "@tanstack/react-query";
import { listActiveTopicSuggestionIds } from "../services/topicSuggestionsService";
import { ACTIVE_TOPIC_SUGGESTIONS_QUERY_KEY } from "./query-keys";

// Powers the badge indicator on the Topic Browser page (ticket 040) - one call for the
// whole tree instead of one per node.
export function useActiveTopicSuggestions() {
    return useQuery({
        queryKey: ACTIVE_TOPIC_SUGGESTIONS_QUERY_KEY,
        queryFn: listActiveTopicSuggestionIds,
    });
}
