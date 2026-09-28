import { useMemo, useState } from "react";
import { NoteEditor } from "./NoteEditor";
import { Button } from "@/shared/components/ui/button";
import { useUpdateNote } from "../hooks/useUpdateNote";
import { useDeleteNote } from "../hooks/useDeleteNote";
import { useAssignNoteTopic } from "../hooks/useAssignNoteTopic";
import { useTopics } from "../../organization/hooks/useTopics";
import { flattenTopicsForSelect } from "../../organization/utils/flattenTopicsForSelect";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { NoteCategoryChips } from "./NoteCategoryChips";
import { NoteAiSuggestionPanel } from "../../ai-categorization/components/NoteAiSuggestionPanel";
import type { NoteResponse } from "../types";

interface NoteCardProps {
  note: NoteResponse;
}

export function NoteCard({ note }: NoteCardProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [draft, setDraft] = useState(note.content);
  const updateNote = useUpdateNote();
  const deleteNote = useDeleteNote();
  const assignTopic = useAssignNoteTopic();
  const topics = useTopics();
  const topicOptions = useMemo(() => flattenTopicsForSelect(topics.data ?? []), [topics.data]);

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

  return (
    <div className="py-4 first:pt-0">
      <select
        aria-label="Topic"
        value={note.topicId ?? ""}
        onChange={(event) => assignTopic.mutate({ id: note.id, topicId: event.target.value || null })}
        disabled={assignTopic.isPending}
        className="mb-3 cursor-pointer rounded-md border border-input bg-transparent px-2 py-1 text-xs text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
      >
        {topicOptions.map((option) => (
          <option key={option.id} value={option.id}>
            {option.label}
          </option>
        ))}
      </select>

      {assignTopic.isError && (
        <p className="mb-2 text-base text-destructive">{getErrorMessage(assignTopic.error, "Failed to move note")}</p>
      )}

      <NoteCategoryChips noteId={note.id} categoryIds={note.categoryIds} />

      <NoteAiSuggestionPanel noteId={note.id} />

      {isEditing ? (
        <>
          <NoteEditor content={draft} onChange={setDraft} topicId={note.topicId ?? undefined} />

          {updateNote.isError && (
            <p className="mt-2 text-base text-destructive">{getErrorMessage(updateNote.error, "Failed to save note")}</p>
          )}

          <div className="mt-3 flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={handleCancel} disabled={updateNote.isPending}>
              Cancel
            </Button>
            <Button type="button" onClick={handleSave} disabled={updateNote.isPending}>
              {updateNote.isPending ? "Saving…" : "Save"}
            </Button>
          </div>
        </>
      ) : (
        <>
          <button
            type="button"
            onClick={startEditing}
            className="line-clamp-3 block w-full cursor-pointer whitespace-pre-line text-left text-base text-card-foreground"
          >
            {note.content || <span className="text-muted-foreground">Empty note</span>}
          </button>

          {deleteNote.isError && (
            <p className="mt-2 text-base text-destructive">{getErrorMessage(deleteNote.error, "Failed to delete note")}</p>
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
        </>
      )}
    </div>
  );
}
