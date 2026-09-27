import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { useTopicSuggestion } from "../hooks/useTopicSuggestion";
import { useAcceptTopicSuggestion } from "../hooks/useAcceptTopicSuggestion";
import { useIgnoreTopicSuggestion } from "../hooks/useIgnoreTopicSuggestion";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

interface TopicSuggestionPanelProps {
    topicId: string;
}

// Topic Suggestion (GLOSSARY.md, ticket 040) - Merge and Placement share the same
// accept/ignore actions, but Merge is destructive (deletes this Topic) so accepting it goes
// through an extra confirm dialog first; Placement applies immediately.
export function TopicSuggestionPanel({ topicId }: TopicSuggestionPanelProps) {
    const [confirmingMerge, setConfirmingMerge] = useState(false);
    const suggestion = useTopicSuggestion(topicId);
    const accept = useAcceptTopicSuggestion();
    const ignore = useIgnoreTopicSuggestion();

    if (suggestion.isLoading) {
        return <div className="mb-4 h-16 animate-pulse rounded-md bg-muted" />;
    }

    if (!suggestion.data) return null;

    const isMerge = suggestion.data.kind === "Merge";

    function handleAcceptClick() {
        if (isMerge) {
            setConfirmingMerge(true);
        } else {
            accept.mutate(topicId);
        }
    }

    function handleConfirmMerge() {
        accept.mutate(topicId, { onSuccess: () => setConfirmingMerge(false) });
    }

    return (
        <div className="mb-4 rounded-md border border-primary/40 bg-primary/5 p-3">
            <p className="text-sm text-foreground">
                {isMerge ? (
                    <>
                        This looks like a duplicate of <strong>{suggestion.data.targetTopicName}</strong>. Merge into it?
                        Its Notes and subtopics will move over and this Topic will be deleted.
                    </>
                ) : (
                    <>
                        This looks like it should be a subtopic of <strong>{suggestion.data.targetTopicName}</strong>. Move it
                        there?
                    </>
                )}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">{Math.round(suggestion.data.confidence * 100)}% confidence</p>

            <div className="mt-2 flex justify-end gap-2">
                <Button type="button" variant="ghost" size="sm" onClick={() => ignore.mutate(topicId)} disabled={ignore.isPending}>
                    {ignore.isPending ? "Ignoring…" : "Ignore"}
                </Button>
                <Button type="button" size="sm" onClick={handleAcceptClick} disabled={accept.isPending}>
                    {accept.isPending ? "Applying…" : isMerge ? "Merge" : "Move here"}
                </Button>
            </div>

            {(accept.isError || ignore.isError) && (
                <p className="mt-1 text-xs text-destructive">
                    {getErrorMessage(accept.error ?? ignore.error, "Failed to update suggestion")}
                </p>
            )}

            {confirmingMerge && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
                    <div className="w-full max-w-sm rounded-lg border border-border bg-card p-4 shadow-lg">
                        <h2 className="mb-2 text-sm font-medium text-card-foreground">Are you sure?</h2>
                        <p className="mb-3 text-sm text-muted-foreground">
                            This deletes this Topic and moves its Notes and subtopics into{" "}
                            <strong>{suggestion.data.targetTopicName}</strong>. This can't be undone.
                        </p>
                        <div className="flex justify-end gap-2">
                            <Button type="button" variant="outline" onClick={() => setConfirmingMerge(false)} disabled={accept.isPending}>
                                Cancel
                            </Button>
                            <Button type="button" onClick={handleConfirmMerge} disabled={accept.isPending}>
                                {accept.isPending ? "Merging…" : "Yes, merge"}
                            </Button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
