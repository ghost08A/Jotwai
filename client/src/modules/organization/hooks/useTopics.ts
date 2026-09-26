import { useQuery } from "@tanstack/react-query";
import { listTopics } from "../services/topicsService";
import { TOPICS_QUERY_KEY } from "./query-keys";

export function useTopics() {
  return useQuery({
    queryKey: TOPICS_QUERY_KEY,
    queryFn: listTopics,
  });
}
