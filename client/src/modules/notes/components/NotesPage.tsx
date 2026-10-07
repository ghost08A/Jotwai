import { NoteCardSkeleton } from "./NoteCardSkeleton";
import { CreateNoteForm } from "./CreateNoteForm";
import { NoteTreeItem } from "./NoteTreeItem";
import { useNotes } from "../hooks/useNotes";
import { flattenNoteTree, listMoveTargets } from "../utils/flattenNoteTree";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

export function NotesPage() {
  const notes = useNotes();
  const rows = flattenNoteTree(notes.data ?? []);

  return (
    <div className="w-full space-y-3 px-6 py-6 md:px-12 lg:px-24">
      <h1 className="text-xl font-semibold text-foreground">Notes</h1>

      <CreateNoteForm submitLabel="New note" />

      {notes.isLoading && Array.from({ length: 3 }).map((_, index) => <NoteCardSkeleton key={index} />)}

      {notes.isError && (
        <p className="text-base text-destructive">{getErrorMessage(notes.error, "Failed to load notes")}</p>
      )}

      {notes.data?.length === 0 && <p className="text-center text-base text-muted-foreground">No notes yet.</p>}

      {rows.map((row) => (
        <NoteTreeItem key={row.note.id} row={row} moveTargets={listMoveTargets(rows, row.note.id)} />
      ))}
    </div>
  );
}
