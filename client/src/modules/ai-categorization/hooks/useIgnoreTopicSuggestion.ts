import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ignoreTopicSuggestion } from "../services/topicSuggestionsService";
import { ACTIVE_TOPIC_SUGGESTIONS_QUERY_KEY, TOPIC_SUGGESTION_QUERY_KEY } from "./query-keys";

export function useIgnoreTopicSuggestion() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (topicId: string) => ignoreTopicSuggestion(topicId),
        onSuccess: (_data, topicId) => {
            queryClient.invalidateQueries({ queryKey: TOPIC_SUGGESTION_QUERY_KEY(topicId) });
            queryClient.invalidateQueries({ queryKey: ACTIVE_TOPIC_SUGGESTIONS_QUERY_KEY });
        },
    });
}
