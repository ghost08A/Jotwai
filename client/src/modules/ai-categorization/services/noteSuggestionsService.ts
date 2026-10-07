import axios from "axios";
import { httpClient } from "../../../core/http/httpClient";
import type { NoteSuggestionResponse } from "../types";

// A 404 means the Note has no unresolved suggestion, not an error.
export async function getNoteSuggestion(noteId: string): Promise<NoteSuggestionResponse | null> {
    try {
        const { data } = await httpClient.get<NoteSuggestionResponse>(`/api/notes/${noteId}/note-suggestion`);
        return data;
    } catch (err) {
        if (axios.isAxiosError(err) && err.response?.status === 404) return null;
        throw err;
    }
}

export async function listActiveNoteSuggestionIds(): Promise<string[]> {
    const { data } = await httpClient.get<string[]>("/api/notes/note-suggestions");
    return data;
}

export async function acceptNoteSuggestion(noteId: string): Promise<void> {
    await httpClient.post(`/api/notes/${noteId}/note-suggestion/accept`);
}

export async function ignoreNoteSuggestion(noteId: string): Promise<void> {
    await httpClient.post(`/api/notes/${noteId}/note-suggestion/ignore`);
}
