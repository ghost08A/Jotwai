import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createNote } from "../services/notesService";
import { NOTES_QUERY_KEY } from "./query-keys";
import type { CreateNoteRequest } from "../types";

export function useCreateNote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateNoteRequest) => createNote(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY }),
  });
}
