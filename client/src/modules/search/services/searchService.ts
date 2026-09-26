import { httpClient } from "../../../core/http/httpClient";
import type { SearchParams, SearchResultResponse } from "../types";

export async function search(params: SearchParams): Promise<SearchResultResponse[]> {
    const { data } = await httpClient.get<SearchResultResponse[]>("/api/search", {
        params: {
            q: params.q || undefined,
            topicId: params.topicId || undefined,
        },
    });
    return data;
}
