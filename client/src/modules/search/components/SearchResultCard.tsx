import type { SearchResultResponse } from "../types";

interface SearchResultCardProps {
    result: SearchResultResponse;
    topicName: string | undefined;
    categoryNames: string[];
}

// Read-only preview of a matching Note - editing happens on the Notes page
// (NoteCard), this is just "where did I see that" search.
export function SearchResultCard({ result, topicName, categoryNames }: SearchResultCardProps) {
    return (
        <div className="py-4 first:pt-0">
            {(topicName || categoryNames.length > 0) && (
                <div className="mb-2 flex flex-wrap items-center gap-1.5 text-xs text-muted-foreground">
                    {topicName && <span className="rounded-full border border-input px-2 py-0.5">{topicName}</span>}
                    {categoryNames.map((name) => (
                        <span key={name} className="rounded-full border border-input px-2 py-0.5">
                            {name}
                        </span>
                    ))}
                </div>
            )}

            <p className="line-clamp-3 whitespace-pre-line text-sm text-card-foreground">
                {result.content || <span className="text-muted-foreground">Empty note</span>}
            </p>
        </div>
    );
}
