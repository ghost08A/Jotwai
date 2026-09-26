export interface TopicResponse {
  id: string;
  name: string;
  parentId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTopicRequest {
  name: string;
  parentId?: string;
}

export interface RenameTopicRequest {
  name: string;
}

export interface CategoryResponse {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCategoryRequest {
  name: string;
}

export interface RenameCategoryRequest {
  name: string;
}
