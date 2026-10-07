import { Button } from "@/shared/components/ui/button";
import { useNoteSuggestion } from "../hooks/useNoteSuggestion";
import { useAcceptNoteSuggestion } from "../hooks/useAcceptNoteSuggestion";
import { useIgnoreNoteSuggestion } from "../hooks/useIgnoreNoteSuggestion";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

interface NoteSuggestionPanelProps {
    noteId: string;
}

// Note Suggestion (CONTEXT.md) - for now only Placement: the new Note looks like it belongs
// under an existing one. Accepting moves it right away; the click is the confirmation.
export function NoteSuggestionPanel({ noteId }: NoteSuggestionPanelProps) {
    const suggestion = useNoteSuggestion(noteId);
    const accept = useAcceptNoteSuggestion();
    const ignore = useIgnoreNoteSuggestion();

    if (suggestion.isLoading) {
        return <div className="mb-3 h-16 animate-pulse rounded-md bg-muted" />;
    }

    if (!suggestion.data) return null;

    return (
        <div className="mb-3 rounded-md border border-primary/40 bg-primary/5 p-3">
            <p className="text-base text-foreground">
                This looks like it belongs under <strong>{suggestion.data.targetNoteName}</strong>. Move it there?
            </p>
            <p className="mt-1 text-xs text-muted-foreground">{Math.round(suggestion.data.confidence * 100)}% confidence</p>

            <div className="mt-2 flex justify-end gap-2">
                <Button type="button" variant="ghost" size="sm" onClick={() => ignore.mutate(noteId)} disabled={ignore.isPending}>
                    {ignore.isPending ? "Ignoring…" : "Ignore"}
                </Button>
                <Button type="button" size="sm" onClick={() => accept.mutate(noteId)} disabled={accept.isPending}>
                    {accept.isPending ? "Moving…" : "Move here"}
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
