import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createTopic } from "../services/topicsService";
import { TOPICS_QUERY_KEY } from "./query-keys";
import type { CreateTopicRequest } from "../types";

export function useCreateTopic() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateTopicRequest) => createTopic(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: TOPICS_QUERY_KEY }),
  });
}
