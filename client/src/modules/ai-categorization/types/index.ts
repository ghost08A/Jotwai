export interface SuggestionCandidateResponse {
    id: string;
    categoryId: string | null;
    newCategoryName: string | null;
    rank: number;
    confidence: number;
}

export interface SuggestionResponse {
    id: string;
    noteId: string;
    candidates: SuggestionCandidateResponse[];
    createdAt: string;
}

export interface AcceptSuggestionRequest {
    candidateIds: string[];
}

export type TopicSuggestionKind = "Merge" | "Placement";

export interface TopicSuggestionResponse {
    id: string;
    topicId: string;
    kind: TopicSuggestionKind;
    targetTopicId: string;
    targetTopicName: string;
    confidence: number;
    createdAt: string;
}

export interface PolishContentRequest {
    content: string;
}

export interface PolishContentResponse {
    content: string;
}
