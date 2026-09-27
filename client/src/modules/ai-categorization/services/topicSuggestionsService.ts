import axios from "axios";
import { httpClient } from "../../../core/http/httpClient";
import type { TopicSuggestionResponse } from "../types";

export async function getTopicSuggestion(topicId: string): Promise<TopicSuggestionResponse | null> {
    try {
        const { data } = await httpClient.get<TopicSuggestionResponse>(`/api/topics/${topicId}/suggestion`);
        return data;
    } catch (err) {
        if (axios.isAxiosError(err) && err.response?.status === 404) return null;
        throw err;
    }
}

export async function listActiveTopicSuggestionIds(): Promise<string[]> {
    const { data } = await httpClient.get<string[]>("/api/topics/suggestions");
    return data;
}

export async function acceptTopicSuggestion(topicId: string): Promise<void> {
    await httpClient.post(`/api/topics/${topicId}/suggestion/accept`);
}

export async function ignoreTopicSuggestion(topicId: string): Promise<void> {
    await httpClient.post(`/api/topics/${topicId}/suggestion/ignore`);
}
