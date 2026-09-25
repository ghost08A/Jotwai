import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateNote } from "../services/notesService";
import { NOTES_QUERY_KEY } from "./query-keys";
import type { UpdateNoteRequest } from "../types";

export function useUpdateNote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateNoteRequest }) => updateNote(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY }),
  });
}
