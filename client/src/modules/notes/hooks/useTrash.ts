import { useQuery } from "@tanstack/react-query";
import { listTrash } from "../services/notesService";
import { TRASH_QUERY_KEY } from "./query-keys";

export function useTrash() {
  return useQuery({
    queryKey: TRASH_QUERY_KEY,
    queryFn: listTrash,
  });
}
