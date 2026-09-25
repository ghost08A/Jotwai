import { useState } from "react";
import { NoteEditor } from "./NoteEditor";
import { Button } from "@/shared/components/ui/button";
import { useUpdateNote } from "../hooks/useUpdateNote";
import { useDeleteNote } from "../hooks/useDeleteNote";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import type { NoteResponse } from "../types";

interface NoteCardProps {
  note: NoteResponse;
}

export function NoteCard({ note }: NoteCardProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [draft, setDraft] = useState(note.content);
  const updateNote = useUpdateNote();
  const deleteNote = useDeleteNote();

  function startEditing() {
    setDraft(note.content);
    setIsEditing(true);
  }

  function handleSave() {
    updateNote.mutate({ id: note.id, request: { content: draft } }, { onSuccess: () => setIsEditing(false) });
  }

  function handleCancel() {
    setDraft(note.content);
    setIsEditing(false);
  }

  if (isEditing) {
    return (
      <div className="rounded-lg border border-border bg-card p-4 shadow-sm">
        <NoteEditor content={draft} onChange={setDraft} />

        {updateNote.isError && (
          <p className="mt-2 text-sm text-destructive">{getErrorMessage(updateNote.error, "Failed to save note")}</p>
        )}

        <div className="mt-3 flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={handleCancel} disabled={updateNote.isPending}>
            Cancel
          </Button>
          <Button type="button" onClick={handleSave} disabled={updateNote.isPending}>
            {updateNote.isPending ? "Saving…" : "Save"}
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-border bg-card p-4 shadow-sm">
      <button
        type="button"
        onClick={startEditing}
        className="line-clamp-3 block w-full cursor-pointer whitespace-pre-line text-left text-sm text-card-foreground"
      >
        {note.content || <span className="text-muted-foreground">Empty note</span>}
      </button>

      {deleteNote.isError && (
        <p className="mt-2 text-sm text-destructive">{getErrorMessage(deleteNote.error, "Failed to delete note")}</p>
      )}

      <div className="mt-3 flex justify-end">
        <Button
          type="button"
          variant="ghost"
          className="text-destructive hover:text-destructive"
          onClick={() => deleteNote.mutate(note.id)}
          disabled={deleteNote.isPending}
        >
          {deleteNote.isPending ? "Deleting…" : "Delete"}
        </Button>
      </div>
    </div>
  );
}
