import { useState } from "react";
import { NoteEditor } from "./NoteEditor";
import { NoteCard } from "./NoteCard";
import { NoteCardSkeleton } from "./NoteCardSkeleton";
import { Button } from "@/shared/components/ui/button";
import { useNotes } from "../hooks/useNotes";
import { useCreateNote } from "../hooks/useCreateNote";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

export function NotesPage() {
  const [draft, setDraft] = useState("");
  const notes = useNotes();
  const createNote = useCreateNote();

  function handleCapture() {
    if (!draft.trim()) return;
    createNote.mutate({ content: draft }, { onSuccess: () => setDraft("") });
  }

  return (
    <div className="w-full space-y-6 px-6 py-6 md:px-12 lg:px-24">
      <div className="pb-4">
        <NoteEditor content={draft} onChange={setDraft} />

        {createNote.isError && (
          <p className="mt-2 text-sm text-destructive">{getErrorMessage(createNote.error, "Failed to save note")}</p>
        )}

        <div className="mt-3 flex justify-end">
          <Button type="button" onClick={handleCapture} disabled={createNote.isPending || !draft.trim()}>
            {createNote.isPending ? "Saving…" : "Save note"}
          </Button>
        </div>
      </div>

      <div className="divide-y divide-border">
        {notes.isLoading && Array.from({ length: 3 }).map((_, index) => <NoteCardSkeleton key={index} />)}

        {notes.isError && (
          <p className="text-sm text-destructive">{getErrorMessage(notes.error, "Failed to load notes")}</p>
        )}

        {notes.data?.length === 0 && (
          <p className="text-center text-sm text-muted-foreground">No notes yet — capture your first one above.</p>
        )}

        {notes.data?.map((note) => <NoteCard key={note.id} note={note} />)}
      </div>
    </div>
  );
}
