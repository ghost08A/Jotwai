import { httpClient } from "../../../core/http/httpClient";
import type { CategoryResponse, CreateCategoryRequest, RenameCategoryRequest } from "../types";

export async function createCategory(request: CreateCategoryRequest): Promise<CategoryResponse> {
  const { data } = await httpClient.post<CategoryResponse>("/api/categories", request);
  return data;
}

export async function listCategories(): Promise<CategoryResponse[]> {
  const { data } = await httpClient.get<CategoryResponse[]>("/api/categories");
  return data;
}

export async function renameCategory(id: string, request: RenameCategoryRequest): Promise<CategoryResponse> {
  const { data } = await httpClient.put<CategoryResponse>(`/api/categories/${id}`, request);
  return data;
}

export async function deleteCategory(id: string): Promise<void> {
  await httpClient.delete(`/api/categories/${id}`);
}
