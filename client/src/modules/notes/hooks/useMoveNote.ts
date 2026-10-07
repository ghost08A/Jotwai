import { useMutation, useQueryClient } from "@tanstack/react-query";
import { moveNote } from "../services/notesService";
import { NOTES_QUERY_KEY } from "./query-keys";
import type { MoveNoteRequest } from "../types";

export function useMoveNote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: MoveNoteRequest }) => moveNote(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY }),
  });
}
