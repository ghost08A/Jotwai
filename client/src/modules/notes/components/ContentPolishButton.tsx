import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { usePolishContent } from "../../ai-categorization/hooks/usePolishContent";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { NoteEditor } from "./NoteEditor";

interface ContentPolishButtonProps {
    content: string;
    onApply: (polishedContent: string) => void;
}

// Content Polish (CONTEXT.md, ticket 041) - on-demand, no state left behind if the user
// doesn't confirm (no entity/table backs this at all, see ContentPolishService). Always
// previews before overwriting anything, since Notes have no version history.
export function ContentPolishButton({ content, onApply }: ContentPolishButtonProps) {
    const [preview, setPreview] = useState<string | null>(null);
    const polish = usePolishContent();

    function handleClick() {
        polish.mutate(content, { onSuccess: (polished) => setPreview(polished) });
    }

    function handleConfirm() {
        if (preview === null) return;
        onApply(preview);
        setPreview(null);
    }

    return (
        <>
            <button
                type="button"
                title="Rewrite the whole document with AI (you confirm before anything changes)"
                onClick={handleClick}
                disabled={polish.isPending || !content.trim()}
                className="cursor-pointer rounded-md px-2 py-1 text-sm text-muted-foreground hover:bg-accent hover:text-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring active:scale-95 disabled:cursor-not-allowed disabled:opacity-50"
            >
                {polish.isPending ? "Polishing…" : "✨ Polish with AI"}
            </button>

            {polish.isError && (
                <p role="alert" className="fixed bottom-6 right-6 z-40 max-w-xs rounded-md bg-destructive px-3 py-2 text-xs text-destructive-foreground shadow-lg">
                    {getErrorMessage(polish.error, "Failed to polish content")}
                </p>
            )}

            {preview !== null && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
                    <div className="flex max-h-[80vh] w-full max-w-2xl flex-col rounded-lg border border-border bg-card p-4 shadow-lg">
                        <h2 className="mb-3 text-base font-medium text-card-foreground">Preview</h2>
                        <div className="flex-1 overflow-y-auto rounded-md border border-border p-3">
                            <NoteEditor content={preview} onChange={() => {}} editable={false} />
                        </div>
                        <div className="mt-3 flex justify-end gap-2">
                            <Button type="button" variant="outline" onClick={() => setPreview(null)}>
                                Cancel
                            </Button>
                            <Button type="button" onClick={handleConfirm}>
                                Confirm
                            </Button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}
