import { useEffect, useMemo, useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { useSuggestion } from "../hooks/useSuggestion";
import { useTriggerSuggestion } from "../hooks/useTriggerSuggestion";
import { useAcceptSuggestion } from "../hooks/useAcceptSuggestion";
import { useIgnoreSuggestion } from "../hooks/useIgnoreSuggestion";
import { useCategories } from "../../organization/hooks/useCategories";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

const POLL_INTERVAL_MS = 3000;
const POLL_TIMEOUT_MS = 30000;

interface NoteAiSuggestionPanelProps {
    noteId: string;
}

// AI Categorization (CONTEXT.md) analyzes a Note in the background; a
// suggestion set only becomes visible once the outbox worker resolves it, so
// after an on-demand trigger this polls briefly instead of leaving the user
// staring at a stale "no suggestion" state.
export function NoteAiSuggestionPanel({ noteId }: NoteAiSuggestionPanelProps) {
    const [isPolling, setIsPolling] = useState(false);
    const [selectedIds, setSelectedIds] = useState<string[]>([]);
    const suggestion = useSuggestion(noteId, isPolling ? POLL_INTERVAL_MS : false);
    const categories = useCategories();
    const trigger = useTriggerSuggestion();
    const accept = useAcceptSuggestion();
    const ignore = useIgnoreSuggestion();

    const categoryNameById = useMemo(() => new Map(categories.data?.map((category) => [category.id, category.name])), [
        categories.data,
    ]);

    useEffect(() => {
        if (suggestion.data) setIsPolling(false);
    }, [suggestion.data]);

    useEffect(() => {
        if (!isPolling) return;
        const timeout = setTimeout(() => setIsPolling(false), POLL_TIMEOUT_MS);
        return () => clearTimeout(timeout);
    }, [isPolling]);

    useEffect(() => {
        setSelectedIds(suggestion.data?.candidates.map((candidate) => candidate.id) ?? []);
    }, [suggestion.data]);

    function toggleCandidate(candidateId: string) {
        setSelectedIds((prev) =>
            prev.includes(candidateId) ? prev.filter((id) => id !== candidateId) : [...prev, candidateId],
        );
    }

    function handleTrigger() {
        trigger.mutate(noteId, { onSuccess: () => setIsPolling(true) });
    }

    function handleAccept() {
        accept.mutate({ noteId, request: { candidateIds: selectedIds } });
    }

    function handleIgnore() {
        ignore.mutate(noteId);
    }

    if (suggestion.isLoading) {
        return <div className="mb-3 h-6 w-40 animate-pulse rounded bg-muted" />;
    }

    if (!suggestion.data) {
        return (
            <div className="mb-3">
                <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleTrigger}
                    disabled={trigger.isPending || isPolling}
                >
                    {trigger.isPending ? "Requesting…" : isPolling ? "Analyzing…" : "Suggest category"}
                </Button>

                {trigger.isError && (
                    <p className="mt-1 text-xs text-destructive">
                        {getErrorMessage(trigger.error, "Failed to request suggestion")}
                    </p>
                )}
            </div>
        );
    }

    const candidates = [...suggestion.data.candidates].sort((a, b) => a.rank - b.rank);

    return (
        <div className="mb-3 rounded-md border border-border bg-muted/40 p-3">
            <p className="mb-2 text-xs font-medium text-muted-foreground">Suggested categories</p>

            <div className="flex flex-wrap gap-1.5">
                {candidates.map((candidate) => {
                    const label = candidate.categoryId
                        ? categoryNameById.get(candidate.categoryId) ?? "…"
                        : candidate.newCategoryName;
                    const isSelected = selectedIds.includes(candidate.id);

                    return (
                        <button
                            key={candidate.id}
                            type="button"
                            aria-pressed={isSelected}
                            onClick={() => toggleCandidate(candidate.id)}
                            disabled={accept.isPending || ignore.isPending}
                            className={`cursor-pointer rounded-full border px-2.5 py-0.5 text-xs font-medium disabled:cursor-not-allowed disabled:opacity-50 ${
                                isSelected
                                    ? "border-primary bg-primary text-primary-foreground"
                                    : "border-input bg-transparent text-muted-foreground hover:bg-accent"
                            }`}
                        >
                            {label}
                            {!candidate.categoryId && " (new)"} · {Math.round(candidate.confidence * 100)}%
                        </button>
                    );
                })}
            </div>

            <div className="mt-2 flex justify-end gap-2">
                <Button type="button" variant="ghost" size="sm" onClick={handleIgnore} disabled={ignore.isPending}>
                    {ignore.isPending ? "Ignoring…" : "Ignore"}
                </Button>
                <Button
                    type="button"
                    size="sm"
                    onClick={handleAccept}
                    disabled={accept.isPending || selectedIds.length === 0}
                >
                    {accept.isPending ? "Accepting…" : "Accept selected"}
                </Button>
            </div>

            {(accept.isError || ignore.isError) && (
                <p className="mt-1 text-xs text-destructive">
                    {getErrorMessage(accept.error ?? ignore.error, "Failed to update suggestion")}
                </p>
            )}
        </div>
    );
}
