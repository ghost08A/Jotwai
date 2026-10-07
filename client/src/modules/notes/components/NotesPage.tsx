import { NoteCardSkeleton } from "./NoteCardSkeleton";
import { Button } from "@/shared/components/ui/button";
import { useNotes } from "../hooks/useNotes";
import { useDeleteNote } from "../hooks/useDeleteNote";
import { flattenNoteTree } from "../utils/flattenNoteTree";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import type { NoteTreeRow } from "../utils/flattenNoteTree";

// Each level of depth indents the row; Tailwind needs the full class names to be static.
const INDENT_CLASSES = ["pl-0", "pl-6", "pl-12", "pl-18", "pl-24"];

export function NotesPage() {
  const notes = useNotes();
  const rows = flattenNoteTree(notes.data ?? []);

  return (
    <div className="w-full space-y-3 px-6 py-6 md:px-12 lg:px-24">
      <h1 className="text-xl font-semibold text-foreground">Notes</h1>

      {notes.isLoading && Array.from({ length: 3 }).map((_, index) => <NoteCardSkeleton key={index} />)}

      {notes.isError && (
        <p className="text-base text-destructive">{getErrorMessage(notes.error, "Failed to load notes")}</p>
      )}

      {notes.data?.length === 0 && <p className="text-center text-base text-muted-foreground">No notes yet.</p>}

      {rows.map((row) => (
        <NoteTreeItem key={row.note.id} row={row} />
      ))}
    </div>
  );
}

function NoteTreeItem({ row }: { row: NoteTreeRow }) {
  const { note, depth } = row;
  const deleteNote = useDeleteNote();
  const indentClass = INDENT_CLASSES[Math.min(depth, INDENT_CLASSES.length - 1)];

  return (
    <div className={indentClass}>
      <div className="rounded-lg border border-border bg-card p-4 shadow-sm">
        <p className="text-base font-medium text-card-foreground">{note.name || "Untitled"}</p>
        <p className="line-clamp-2 whitespace-pre-line text-base text-muted-foreground">
          {note.content || "Empty note"}
        </p>

        {deleteNote.isError && (
          <p className="mt-2 text-base text-destructive">{getErrorMessage(deleteNote.error, "Failed to delete note")}</p>
        )}

        <div className="mt-3 flex justify-end">
          <Button
            type="button"
            variant="outline"
            onClick={() => deleteNote.mutate(note.id)}
            disabled={deleteNote.isPending}
          >
            {deleteNote.isPending ? "Deleting…" : "Delete"}
          </Button>
        </div>
      </div>
    </div>
  );
}
