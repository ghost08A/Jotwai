import { useState } from "react";
import { NoteCardSkeleton } from "./NoteCardSkeleton";
import { Button } from "@/shared/components/ui/button";
import { useTrash } from "../hooks/useTrash";
import { useRestoreNote } from "../hooks/useRestoreNote";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import type { NoteResponse } from "../types";

export function TrashPage() {
  const trash = useTrash();
  // Note: kept here, not on the card - the card unmounts as soon as the restored Note leaves the Trash list.
  const [duplicateNameWarning, setDuplicateNameWarning] = useState<string>();

  return (
    <div className="w-full space-y-3 px-6 py-6 md:px-12 lg:px-24">
      <h1 className="text-xl font-semibold text-foreground">Trash</h1>

      {duplicateNameWarning && (
        <p role="status" className="text-base text-muted-foreground">
          Restored. Another note under the same parent is also named “{duplicateNameWarning}”.
        </p>
      )}

      {trash.isLoading && Array.from({ length: 3 }).map((_, index) => <NoteCardSkeleton key={index} />)}

      {trash.isError && (
        <p className="text-base text-destructive">{getErrorMessage(trash.error, "Failed to load trash")}</p>
      )}

      {trash.data?.length === 0 && <p className="text-center text-base text-muted-foreground">Trash is empty.</p>}

      {trash.data?.map((note) => (
        <TrashedNoteCard key={note.id} note={note} onRestored={setDuplicateNameWarning} />
      ))}
    </div>
  );
}

interface TrashedNoteCardProps {
  note: NoteResponse;
  onRestored: (duplicateName: string | undefined) => void;
}

function TrashedNoteCard({ note, onRestored }: TrashedNoteCardProps) {
  const restoreNote = useRestoreNote();

  function handleRestore() {
    restoreNote.mutate(note.id, {
      onSuccess: (restored) => onRestored(restored.hasDuplicateSiblingName ? restored.name : undefined),
    });
  }

  return (
    <div className="rounded-lg border border-border bg-card p-4 shadow-sm">
      {note.name && <p className="text-base font-medium text-card-foreground">{note.name}</p>}
      <p className="line-clamp-3 whitespace-pre-line text-base text-muted-foreground">
        {note.content || "Empty note"}
      </p>

      {restoreNote.isError && (
        <p className="mt-2 text-base text-destructive">{getErrorMessage(restoreNote.error, "Failed to restore note")}</p>
      )}

      <div className="mt-3 flex justify-end">
        <Button type="button" variant="outline" onClick={handleRestore} disabled={restoreNote.isPending}>
          {restoreNote.isPending ? "Restoring…" : "Restore"}
        </Button>
      </div>
    </div>
  );
}
