export const SUGGESTION_QUERY_KEY = (noteId: string) => ["ai-categorization", "suggestion", noteId] as const;
export const TOPIC_SUGGESTION_QUERY_KEY = (topicId: string) => ["ai-categorization", "topic-suggestion", topicId] as const;
export const ACTIVE_TOPIC_SUGGESTIONS_QUERY_KEY = ["ai-categorization", "topic-suggestions", "active"] as const;
