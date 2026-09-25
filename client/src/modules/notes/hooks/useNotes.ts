import { useQuery } from "@tanstack/react-query";
import { listNotes } from "../services/notesService";
import { NOTES_QUERY_KEY } from "./query-keys";

export function useNotes() {
  return useQuery({
    queryKey: NOTES_QUERY_KEY,
    queryFn: listNotes,
  });
}
