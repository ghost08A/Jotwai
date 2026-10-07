export const SUGGESTION_QUERY_KEY = (noteId: string) => ["ai-categorization", "suggestion", noteId] as const;
export const TOPIC_SUGGESTION_QUERY_KEY = (topicId: string) => ["ai-categorization", "topic-suggestion", topicId] as const;
export const ACTIVE_TOPIC_SUGGESTIONS_QUERY_KEY = ["ai-categorization", "topic-suggestions", "active"] as const;
export const NOTE_SUGGESTION_QUERY_KEY = (noteId: string) => ["ai-categorization", "note-suggestion", noteId] as const;
export const ACTIVE_NOTE_SUGGESTIONS_QUERY_KEY = ["ai-categorization", "note-suggestions", "active"] as const;
