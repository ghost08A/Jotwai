import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteTopic } from "../services/topicsService";
import { TOPICS_QUERY_KEY } from "./query-keys";

export function useDeleteTopic() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteTopic(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: TOPICS_QUERY_KEY }),
  });
}
