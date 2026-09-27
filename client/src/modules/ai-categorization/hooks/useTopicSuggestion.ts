import { useQuery } from "@tanstack/react-query";
import { getTopicSuggestion } from "../services/topicSuggestionsService";
import { TOPIC_SUGGESTION_QUERY_KEY } from "./query-keys";

export function useTopicSuggestion(topicId: string, refetchInterval: number | false = false) {
    return useQuery({
        queryKey: TOPIC_SUGGESTION_QUERY_KEY(topicId),
        queryFn: () => getTopicSuggestion(topicId),
        refetchInterval,
    });
}
