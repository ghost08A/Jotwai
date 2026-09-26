import { useMutation, useQueryClient } from "@tanstack/react-query";
import { renameCategory } from "../services/categoriesService";
import { CATEGORIES_QUERY_KEY } from "./query-keys";
import type { RenameCategoryRequest } from "../types";

export function useRenameCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: RenameCategoryRequest }) => renameCategory(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CATEGORIES_QUERY_KEY }),
  });
}
