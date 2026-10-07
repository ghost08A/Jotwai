import { useState } from "react";
import { NoteCardSkeleton } from "./NoteCardSkeleton";
import { CreateNoteForm } from "./CreateNoteForm";
import { NoteFilters } from "./NoteFilters";
import { NoteTreeItem } from "./NoteTreeItem";
import { useNotes } from "../hooks/useNotes";
import { useSearch } from "../../search/hooks/useSearch";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { flattenNoteTree, listMoveTargets } from "../utils/flattenNoteTree";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import type { NoteTreeRow } from "../utils/flattenNoteTree";

export function NotesPage() {
  const [query, setQuery] = useState("");
  const [parentFilterId, setParentFilterId] = useState("");
  const [selectedCategoryIds, setSelectedCategoryIds] = useState<string[]>([]);
  const debouncedQuery = useDebouncedValue(query, 300);

  const notes = useNotes();
  const treeRows = flattenNoteTree(notes.data ?? []);
  const isFiltering = debouncedQuery.trim() !== "" || parentFilterId !== "" || selectedCategoryIds.length > 0;
  const search = useSearch(debouncedQuery, selectedCategoryIds, parentFilterId || undefined);

  function toggleCategory(categoryId: string) {
    setSelectedCategoryIds((previous) =>
      previous.includes(categoryId) ? previous.filter((id) => id !== categoryId) : [...previous, categoryId],
    );
  }

  // Note: search only returns ids - each match is shown as the full note from the list, flat
  // (its place in the tree isn't meaningful without its parents), so every row action still works.
  const notesById = new Map((notes.data ?? []).map((note) => [note.id, note]));
  const matchRows: NoteTreeRow[] = (search.data ?? []).flatMap((result) => {
    const note = notesById.get(result.id);
    return note ? [{ note, depth: 0 }] : [];
  });
  const rows = isFiltering ? matchRows : treeRows;

  const isLoading = notes.isLoading || (isFiltering && search.isLoading);
  const hasNoMatches = isFiltering && search.isSuccess && matchRows.length === 0;

  return (
    <div className="w-full space-y-3 px-6 py-6 md:px-12 lg:px-24">
      <h1 className="text-xl font-semibold text-foreground">Notes</h1>

      <CreateNoteForm submitLabel="New note" />

      <NoteFilters
        query={query}
        onQueryChange={setQuery}
        parentId={parentFilterId}
        onParentIdChange={setParentFilterId}
        parentOptions={treeRows}
        selectedCategoryIds={selectedCategoryIds}
        onToggleCategory={toggleCategory}
      />

      {isLoading && Array.from({ length: 3 }).map((_, index) => <NoteCardSkeleton key={index} />)}

      {(notes.isError || search.isError) && (
        <p className="text-base text-destructive">
          {getErrorMessage(notes.error ?? search.error, "Failed to load notes")}
        </p>
      )}

      {!isFiltering && notes.data?.length === 0 && (
        <p className="text-center text-base text-muted-foreground">No notes yet.</p>
      )}
      {hasNoMatches && <p className="text-center text-base text-muted-foreground">No matching notes.</p>}

      {!isLoading &&
        rows.map((row) => (
          <NoteTreeItem key={row.note.id} row={row} moveTargets={listMoveTargets(treeRows, row.note.id)} />
        ))}
    </div>
  );
}
