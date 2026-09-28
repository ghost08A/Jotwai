import { NoteCardSkeleton } from "./NoteCardSkeleton";
import { Button } from "@/shared/components/ui/button";
import { useTrash } from "../hooks/useTrash";
import { useRestoreNote } from "../hooks/useRestoreNote";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import type { NoteResponse } from "../types";

export function TrashPage() {
  const trash = useTrash();

  return (
    <div className="w-full space-y-3 px-6 py-6 md:px-12 lg:px-24">
      <h1 className="text-xl font-semibold text-foreground">Trash</h1>

      {trash.isLoading && Array.from({ length: 3 }).map((_, index) => <NoteCardSkeleton key={index} />)}

      {trash.isError && (
        <p className="text-base text-destructive">{getErrorMessage(trash.error, "Failed to load trash")}</p>
      )}

      {trash.data?.length === 0 && <p className="text-center text-base text-muted-foreground">Trash is empty.</p>}

      {trash.data?.map((note) => <TrashedNoteCard key={note.id} note={note} />)}
    </div>
  );
}

function TrashedNoteCard({ note }: { note: NoteResponse }) {
  const restoreNote = useRestoreNote();

  return (
    <div className="rounded-lg border border-border bg-card p-4 shadow-sm">
      <p className="line-clamp-3 whitespace-pre-line text-base text-muted-foreground">
        {note.content || "Empty note"}
      </p>

      {restoreNote.isError && (
        <p className="mt-2 text-base text-destructive">{getErrorMessage(restoreNote.error, "Failed to restore note")}</p>
      )}

      <div className="mt-3 flex justify-end">
        <Button
          type="button"
          variant="outline"
          onClick={() => restoreNote.mutate(note.id)}
          disabled={restoreNote.isPending}
        >
          {restoreNote.isPending ? "Restoring…" : "Restore"}
        </Button>
      </div>
    </div>
  );
}
