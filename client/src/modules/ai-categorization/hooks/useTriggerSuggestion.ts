import { useMutation } from "@tanstack/react-query";
import { triggerSuggestion } from "../services/suggestionsService";

export function useTriggerSuggestion() {
    return useMutation({
        mutationFn: (noteId: string) => triggerSuggestion(noteId),
    });
}
