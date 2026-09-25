import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteNote } from "../services/notesService";
import { NOTES_QUERY_KEY, TRASH_QUERY_KEY } from "./query-keys";

export function useDeleteNote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteNote(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: TRASH_QUERY_KEY });
    },
  });
}
