import { httpClient } from "../../../core/http/httpClient";
import type { PolishContentRequest, PolishContentResponse } from "../types";

export async function polishContent(content: string): Promise<string> {
    const { data } = await httpClient.post<PolishContentResponse>(
        "/api/content-polish",
        { content } satisfies PolishContentRequest,
    );
    return data.content;
}
