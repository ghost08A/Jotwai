import { useEffect, useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { useTopicSuggestion } from "../hooks/useTopicSuggestion";
import { useTriggerTopicSuggestion } from "../hooks/useTriggerTopicSuggestion";
import { useAcceptTopicSuggestion } from "../hooks/useAcceptTopicSuggestion";
import { useIgnoreTopicSuggestion } from "../hooks/useIgnoreTopicSuggestion";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

const POLL_INTERVAL_MS = 3000;
const POLL_TIMEOUT_MS = 30000;

interface TopicSuggestionPanelProps {
    topicId: string;
    // A getter, not a plain string prop - the current draft lives in a ref one level up
    // (TopicDetailPage) so typing in the editor doesn't re-render this panel on every
    // keystroke; this just reads the latest value at the moment the button is clicked.
    getContent: () => string;
}

// Topic Suggestion (GLOSSARY.md, ticket 040) - Merge and Placement share the same
// accept/ignore actions, but Merge is destructive (deletes this Topic) so accepting it goes
// through an extra confirm dialog first; Placement applies immediately. Since ticket 044, a
// Topic's name alone may be a meaningless placeholder ("Untitled") - the "Suggest placement"
// button below re-analyzes using the Topic's current name *and* content on demand (mirrors
// NoteAiSuggestionPanel's trigger+poll pattern for Category Suggestion).
export function TopicSuggestionPanel({ topicId, getContent }: TopicSuggestionPanelProps) {
    const [confirmingMerge, setConfirmingMerge] = useState(false);
    const [isPolling, setIsPolling] = useState(false);
    const suggestion = useTopicSuggestion(topicId, isPolling ? POLL_INTERVAL_MS : false);
    const trigger = useTriggerTopicSuggestion();
    const accept = useAcceptTopicSuggestion();
    const ignore = useIgnoreTopicSuggestion();

    useEffect(() => {
        if (suggestion.data) setIsPolling(false);
    }, [suggestion.data]);

    useEffect(() => {
        if (!isPolling) return;
        const timeout = setTimeout(() => setIsPolling(false), POLL_TIMEOUT_MS);
        return () => clearTimeout(timeout);
    }, [isPolling]);

    function handleTrigger() {
        trigger.mutate({ topicId, content: getContent() }, { onSuccess: () => setIsPolling(true) });
    }

    if (suggestion.isLoading) {
        return <div className="mb-4 h-16 animate-pulse rounded-md bg-muted" />;
    }

    if (!suggestion.data) {
        return (
            <div className="mb-4">
                <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleTrigger}
                    disabled={trigger.isPending || isPolling}
                >
                    {trigger.isPending ? "Requesting…" : isPolling ? "Analyzing…" : "Suggest placement"}
                </Button>

                {trigger.isError && (
                    <p className="mt-1 text-xs text-destructive">
                        {getErrorMessage(trigger.error, "Failed to request suggestion")}
                    </p>
                )}
            </div>
        );
    }

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
