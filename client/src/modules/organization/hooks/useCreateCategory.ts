import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createCategory } from "../services/categoriesService";
import { CATEGORIES_QUERY_KEY } from "./query-keys";
import type { CreateCategoryRequest } from "../types";

export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateCategoryRequest) => createCategory(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CATEGORIES_QUERY_KEY }),
  });
}
