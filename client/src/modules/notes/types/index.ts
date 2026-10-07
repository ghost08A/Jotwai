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
  name: string;
  content: string;
  parentId?: string;
}

export interface RenameNoteRequest {
  name: string;
}

export interface MoveNoteRequest {
  parentId: string | null;
}

export interface UpdateNoteRequest {
  content: string;
}
