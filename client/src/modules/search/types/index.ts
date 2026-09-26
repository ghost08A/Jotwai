export interface SearchResultResponse {
    id: string;
    content: string;
    topicId: string | null;
    categoryIds: string[];
    createdAt: string;
    updatedAt: string;
}

export interface SearchParams {
    q: string;
    topicId: string;
}
