import { useMutation } from "@tanstack/react-query";
import { triggerTopicSuggestion } from "../services/topicSuggestionsService";

export function useTriggerTopicSuggestion() {
    return useMutation({
        mutationFn: ({ topicId, content }: { topicId: string; content: string }) => triggerTopicSuggestion(topicId, content),
    });
}
