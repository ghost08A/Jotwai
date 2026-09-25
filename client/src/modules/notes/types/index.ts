export interface NoteResponse {
  id: string;
  content: string;
  topicId: string | null;
  categoryIds: string[];
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
}

export interface CreateNoteRequest {
  content: string;
}

export interface UpdateNoteRequest {
  content: string;
}
