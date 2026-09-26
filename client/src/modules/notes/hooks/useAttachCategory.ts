import { useMutation, useQueryClient } from "@tanstack/react-query";
import { attachCategory } from "../services/notesService";
import { NOTES_QUERY_KEY } from "./query-keys";

export function useAttachCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ noteId, categoryId }: { noteId: string; categoryId: string }) => attachCategory(noteId, categoryId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY }),
  });
}
