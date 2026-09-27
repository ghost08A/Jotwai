import { useMutation, useQueryClient } from "@tanstack/react-query";
import { acceptTopicSuggestion } from "../services/topicSuggestionsService";
import { ACTIVE_TOPIC_SUGGESTIONS_QUERY_KEY, TOPIC_SUGGESTION_QUERY_KEY } from "./query-keys";
import { TOPICS_QUERY_KEY } from "../../organization/hooks/query-keys";
import { NOTES_QUERY_KEY } from "../../notes/hooks/query-keys";

export function useAcceptTopicSuggestion() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (topicId: string) => acceptTopicSuggestion(topicId),
        onSettled: (_data, _error, topicId) => {
            queryClient.invalidateQueries({ queryKey: TOPIC_SUGGESTION_QUERY_KEY(topicId) });
            queryClient.invalidateQueries({ queryKey: ACTIVE_TOPIC_SUGGESTIONS_QUERY_KEY });
            // Merge deletes a Topic and reassigns Notes; Placement re-parents one - either
            // way the Topic tree and (for Merge) the Note list may have changed.
            queryClient.invalidateQueries({ queryKey: TOPICS_QUERY_KEY });
            queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY });
        },
    });
}
