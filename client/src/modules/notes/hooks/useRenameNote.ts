import { useMutation, useQueryClient } from "@tanstack/react-query";
import { renameNote } from "../services/notesService";
import { NOTES_QUERY_KEY } from "./query-keys";
import type { RenameNoteRequest } from "../types";

export function useRenameNote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: RenameNoteRequest }) => renameNote(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY }),
  });
}
