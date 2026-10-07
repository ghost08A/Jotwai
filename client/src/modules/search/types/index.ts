export interface SearchResultResponse {
    id: string;
    name: string;
    content: string;
    parentId: string | null;
    topicId: string | null;
    categoryIds: string[];
    createdAt: string;
    updatedAt: string;
}

export interface SearchParams {
    q: string;
    categoryIds: string[];
    // Limits results to this note and everything beneath it, at every level.
    parentId?: string;
}
