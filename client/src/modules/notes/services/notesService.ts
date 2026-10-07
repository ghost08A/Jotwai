import axios from "axios";
import { httpClient } from "../../../core/http/httpClient";
import type { CreateNoteRequest, NoteResponse, UpdateNoteRequest } from "../types";

export async function createNote(request: CreateNoteRequest): Promise<NoteResponse> {
  const { data } = await httpClient.post<NoteResponse>("/api/notes", request);
  return data;
}

export async function listNotes(): Promise<NoteResponse[]> {
  const { data } = await httpClient.get<NoteResponse[]>("/api/notes");
  return data;
}

export async function listTrash(): Promise<NoteResponse[]> {
  const { data } = await httpClient.get<NoteResponse[]>("/api/notes/trash");
  return data;
}

export async function updateNote(id: string, request: UpdateNoteRequest): Promise<NoteResponse> {
  const { data } = await httpClient.put<NoteResponse>(`/api/notes/${id}`, request);
  return data;
}

// A Topic has at most one active Note (its continuous document) - addressed by topicId
// instead of noteId. getNoteByTopic's 404 means the Topic has no content yet, not an error.
export async function getNoteByTopic(topicId: string): Promise<NoteResponse | null> {
  try {
    const { data } = await httpClient.get<NoteResponse>(`/api/notes/by-topic/${topicId}`);
    return data;
  } catch (err) {
    if (axios.isAxiosError(err) && err.response?.status === 404) return null;
    throw err;
  }
}

export async function upsertNoteByTopic(topicId: string, request: UpdateNoteRequest): Promise<NoteResponse> {
  const { data } = await httpClient.put<NoteResponse>(`/api/notes/by-topic/${topicId}`, request);
  return data;
}

export async function deleteNote(id: string): Promise<void> {
  await httpClient.delete(`/api/notes/${id}`);
}

export async function restoreNote(id: string): Promise<NoteResponse> {
  const { data } = await httpClient.post<NoteResponse>(`/api/notes/${id}/restore`);
  return data;
}

export async function assignNoteTopic(id: string, topicId: string | null): Promise<NoteResponse> {
  const { data } = await httpClient.put<NoteResponse>(`/api/notes/${id}/topic`, { topicId });
  return data;
}

export async function attachCategory(noteId: string, categoryId: string): Promise<NoteResponse> {
  const { data } = await httpClient.post<NoteResponse>(`/api/notes/${noteId}/categories/${categoryId}`);
  return data;
}

export async function detachCategory(noteId: string, categoryId: string): Promise<NoteResponse> {
  const { data } = await httpClient.delete<NoteResponse>(`/api/notes/${noteId}/categories/${categoryId}`);
  return data;
}
