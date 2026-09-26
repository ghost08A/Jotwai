import { useMutation, useQueryClient } from "@tanstack/react-query";
import { assignNoteTopic } from "../services/notesService";
import { NOTES_QUERY_KEY } from "./query-keys";

export function useAssignNoteTopic() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, topicId }: { id: string; topicId: string | null }) => assignNoteTopic(id, topicId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY }),
  });
}
