import { useQuery } from "@tanstack/react-query";
import { listCategories } from "../services/categoriesService";
import { CATEGORIES_QUERY_KEY } from "./query-keys";

export function useCategories() {
  return useQuery({
    queryKey: CATEGORIES_QUERY_KEY,
    queryFn: listCategories,
  });
}
