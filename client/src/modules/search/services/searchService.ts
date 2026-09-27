import { httpClient } from "../../../core/http/httpClient";
import type { SearchParams, SearchResultResponse } from "../types";

export async function search(params: SearchParams): Promise<SearchResultResponse[]> {
    // Built by hand rather than passed via axios's `params` option - axios serializes
    // array values as `categoryIds[]=...`, but ASP.NET Core's `List<Guid>` query binding
    // expects the key repeated instead (`categoryIds=...&categoryIds=...`).
    const query = new URLSearchParams();
    if (params.q) query.set("q", params.q);
    for (const categoryId of params.categoryIds) query.append("categoryIds", categoryId);

    const { data } = await httpClient.get<SearchResultResponse[]>(`/api/search?${query.toString()}`);
    return data;
}
