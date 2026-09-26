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
