import { useMutation, useQueryClient } from "@tanstack/react-query";
import { renameTopic } from "../services/topicsService";
import { TOPICS_QUERY_KEY } from "./query-keys";
import type { RenameTopicRequest } from "../types";

export function useRenameTopic() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: RenameTopicRequest }) => renameTopic(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: TOPICS_QUERY_KEY }),
  });
}
