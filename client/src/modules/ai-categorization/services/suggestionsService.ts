import axios from "axios";
import { httpClient } from "../../../core/http/httpClient";
import type { NoteResponse } from "../../notes/types";
import type { AcceptSuggestionRequest, SuggestionResponse } from "../types";

export async function getSuggestion(noteId: string): Promise<SuggestionResponse | null> {
    try {
        const { data } = await httpClient.get<SuggestionResponse>(`/api/notes/${noteId}/suggestion`);
        return data;
    } catch (err) {
        if (axios.isAxiosError(err) && err.response?.status === 404) return null;
        throw err;
    }
}

export async function acceptSuggestion(noteId: string, request: AcceptSuggestionRequest): Promise<NoteResponse> {
    const { data } = await httpClient.post<NoteResponse>(`/api/notes/${noteId}/suggestion/accept`, request);
    return data;
}

export async function ignoreSuggestion(noteId: string): Promise<void> {
    await httpClient.post(`/api/notes/${noteId}/suggestion/ignore`);
}

export async function triggerSuggestion(noteId: string): Promise<void> {
    await httpClient.post(`/api/notes/${noteId}/suggestion/trigger`);
}
