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
