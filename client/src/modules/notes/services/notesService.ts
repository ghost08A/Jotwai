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

export async function deleteNote(id: string): Promise<void> {
  await httpClient.delete(`/api/notes/${id}`);
}

export async function restoreNote(id: string): Promise<void> {
  await httpClient.post(`/api/notes/${id}/restore`);
}
