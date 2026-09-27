import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { usePolishContent } from "../../ai-categorization/hooks/usePolishContent";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { NoteEditor } from "./NoteEditor";

interface ContentPolishButtonProps {
    content: string;
    onApply: (polishedContent: string) => void;
}

// Content Polish (GLOSSARY.md, ticket 041) - on-demand, no state left behind if the user
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
                aria-label="Polish content with AI"
                title="Polish content with AI"
                onClick={handleClick}
                disabled={polish.isPending || !content.trim()}
                className="absolute bottom-3 right-3 z-10 flex h-11 w-11 cursor-pointer items-center justify-center rounded-full bg-primary text-primary-foreground shadow-lg hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-50"
            >
                {polish.isPending ? "…" : "✨"}
            </button>

            {polish.isError && (
                <p className="absolute bottom-16 right-3 z-10 max-w-xs rounded-md bg-destructive px-3 py-2 text-xs text-destructive-foreground shadow-lg">
                    {getErrorMessage(polish.error, "Failed to polish content")}
                </p>
            )}

            {preview !== null && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
                    <div className="flex max-h-[80vh] w-full max-w-2xl flex-col rounded-lg border border-border bg-card p-4 shadow-lg">
                        <h2 className="mb-3 text-sm font-medium text-card-foreground">Preview</h2>
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
