export interface NoteResponse {
  id: string;
  name: string;
  content: string;
  parentId: string | null;
  topicId: string | null;
  categoryIds: string[];
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
  hasDuplicateSiblingName: boolean;
}

export interface CreateNoteRequest {
  content: string;
}

export interface UpdateNoteRequest {
  content: string;
}
