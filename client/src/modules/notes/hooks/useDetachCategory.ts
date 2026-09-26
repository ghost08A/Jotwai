import { useMutation, useQueryClient } from "@tanstack/react-query";
import { detachCategory } from "../services/notesService";
import { NOTES_QUERY_KEY } from "./query-keys";

export function useDetachCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ noteId, categoryId }: { noteId: string; categoryId: string }) => detachCategory(noteId, categoryId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY }),
  });
}
