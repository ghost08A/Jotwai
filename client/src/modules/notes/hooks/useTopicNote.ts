import { useQuery } from "@tanstack/react-query";
import { getNoteByTopic } from "../services/notesService";
import { topicNoteQueryKey } from "./query-keys";

export function useTopicNote(topicId: string) {
  return useQuery({
    queryKey: topicNoteQueryKey(topicId),
    queryFn: () => getNoteByTopic(topicId),
  });
}
