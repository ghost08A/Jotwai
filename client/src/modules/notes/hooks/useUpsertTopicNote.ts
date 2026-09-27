import { useMutation, useQueryClient } from "@tanstack/react-query";
import { upsertNoteByTopic } from "../services/notesService";
import { NOTES_QUERY_KEY, topicNoteQueryKey } from "./query-keys";
import type { UpdateNoteRequest } from "../types";

export function useUpsertTopicNote(topicId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateNoteRequest) => upsertNoteByTopic(topicId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: topicNoteQueryKey(topicId) });
      queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY });
    },
  });
}
