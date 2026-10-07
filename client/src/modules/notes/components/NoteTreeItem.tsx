import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { useDeleteNote } from "../hooks/useDeleteNote";
import { useMoveNote } from "../hooks/useMoveNote";
import { DuplicateNameWarning } from "./DuplicateNameWarning";
import { NoteCategoryChips } from "./NoteCategoryChips";
import { NoteEditForm } from "./NoteEditForm";
import { NoteAiSuggestionPanel } from "../../ai-categorization/components/NoteAiSuggestionPanel";
import { NoteSuggestionPanel } from "../../ai-categorization/components/NoteSuggestionPanel";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import type { NoteTreeRow } from "../utils/flattenNoteTree";

// Each level of depth indents the row; Tailwind needs the full class names to be static.
const INDENT_CLASSES = ["pl-0", "pl-6", "pl-12", "pl-18", "pl-24"];

interface NoteTreeItemProps {
  row: NoteTreeRow;
  moveTargets: NoteTreeRow[];
  hasSuggestion: boolean;
}

export function NoteTreeItem({ row, moveTargets, hasSuggestion }: NoteTreeItemProps) {
  const { note, depth } = row;
  const [isEditing, setIsEditing] = useState(false);
  // Note: UI-only; set when the last rename/move produced a duplicate-name warning.
  const [duplicateName, setDuplicateName] = useState<string>();
  const deleteNote = useDeleteNote();
  const moveNote = useMoveNote();
  const indentClass = INDENT_CLASSES[Math.min(depth, INDENT_CLASSES.length - 1)];

  function handleMove(parentId: string) {
    moveNote.mutate(
      { id: note.id, request: { parentId: parentId || null } },
      { onSuccess: (moved) => setDuplicateName(moved.hasDuplicateSiblingName ? moved.name : undefined) },
    );
  }

  return (
    <div className={indentClass}>
      <div className="rounded-lg border border-border bg-card p-4 shadow-sm">
        {isEditing ? (
          <NoteEditForm
            note={note}
            onCancel={() => setIsEditing(false)}
            onSaved={(savedDuplicateName) => {
              setDuplicateName(savedDuplicateName);
              setIsEditing(false);
            }}
          />
        ) : (
          <>
            <p className="text-base font-medium text-card-foreground">{note.name || "Untitled"}</p>
            <p className="mb-3 line-clamp-2 whitespace-pre-line text-base text-muted-foreground">
              {note.content || "Empty note"}
            </p>
            {hasSuggestion && <NoteSuggestionPanel noteId={note.id} />}
            <NoteCategoryChips noteId={note.id} categoryIds={note.categoryIds} />
            <NoteAiSuggestionPanel noteId={note.id} />
          </>
        )}

        {duplicateName !== undefined && <DuplicateNameWarning name={duplicateName} />}

        {(deleteNote.isError || moveNote.isError) && (
          <p className="mt-2 text-base text-destructive">
            {getErrorMessage(deleteNote.error ?? moveNote.error, "Failed to update note")}
          </p>
        )}

        {!isEditing && (
          <div className="mt-3 flex flex-wrap items-center justify-between gap-2">
            <select
              aria-label={`Move ${note.name}`}
              value={note.parentId ?? ""}
              onChange={(event) => handleMove(event.target.value)}
              disabled={moveNote.isPending}
              className="cursor-pointer rounded-md border border-input bg-transparent px-2 py-1 text-base text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="">Top level</option>
              {moveTargets.map((target) => (
                <option key={target.note.id} value={target.note.id}>
                  {`${"— ".repeat(target.depth)}${target.note.name}`}
                </option>
              ))}
            </select>

            <div className="flex flex-wrap gap-2">
              <Button type="button" variant="outline" onClick={() => setIsEditing(true)}>
                Edit
              </Button>
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
        )}
      </div>
    </div>
  );
}
