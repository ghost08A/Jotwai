export const NOTES_QUERY_KEY = ["notes", "list"] as const;
export const TRASH_QUERY_KEY = ["notes", "trash"] as const;
export const topicNoteQueryKey = (topicId: string) => ["notes", "byTopic", topicId] as const;
