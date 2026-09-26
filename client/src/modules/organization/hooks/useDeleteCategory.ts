import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteCategory } from "../services/categoriesService";
import { CATEGORIES_QUERY_KEY } from "./query-keys";
import { NOTES_QUERY_KEY } from "../../notes/hooks/query-keys";

export function useDeleteCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteCategory(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CATEGORIES_QUERY_KEY });
      // Deleting a Category auto-detaches it from every Note server-side
      // (NotesKnownCategoriesProjectionHandler) - refresh Notes too so any
      // category chip shown as attached disappears.
      queryClient.invalidateQueries({ queryKey: NOTES_QUERY_KEY });
    },
  });
}
