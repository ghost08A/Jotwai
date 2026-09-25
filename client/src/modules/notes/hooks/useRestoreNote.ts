import { useMutation, useQueryClient } from "@tanstack/react-query";
import { restoreNote } from "../services/notesService";
import { NOTES_QUERY_KEY, TRASH_QUERY_KEY } from "./query-keys";

export function useRestoreNote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => restoreNote(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: TRASH_QUERY_KEY });
    },
  });
}
