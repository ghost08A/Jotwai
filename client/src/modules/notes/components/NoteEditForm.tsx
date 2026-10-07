import { useState } from "react";
import type { FormEvent } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useRenameNote } from "../hooks/useRenameNote";
import { useUpdateNote } from "../hooks/useUpdateNote";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import type { NoteResponse } from "../types";

interface NoteEditFormProps {
  note: NoteResponse;
  // duplicateName is the saved name when it clashes with a sibling's, otherwise undefined.
  onSaved: (duplicateName: string | undefined) => void;
  onCancel: () => void;
}

export function NoteEditForm({ note, onSaved, onCancel }: NoteEditFormProps) {
  const [name, setName] = useState(note.name);
  const [content, setContent] = useState(note.content);
  const [errorMessage, setErrorMessage] = useState<string>();
  const renameNote = useRenameNote();
  const updateNote = useUpdateNote();
  const isSaving = renameNote.isPending || updateNote.isPending;

  async function handleSave(event: FormEvent) {
    event.preventDefault();
    setErrorMessage(undefined);
    try {
      // Name and content are separate endpoints, so only call the one that actually changed.
      let duplicateName: string | undefined;
      if (name.trim() !== note.name) {
        const renamed = await renameNote.mutateAsync({ id: note.id, request: { name } });
        duplicateName = renamed.hasDuplicateSiblingName ? renamed.name : undefined;
      }
      if (content !== note.content) {
        await updateNote.mutateAsync({ id: note.id, request: { content } });
      }
      onSaved(duplicateName);
    } catch (err) {
      setErrorMessage(getErrorMessage(err, "Failed to save note"));
    }
  }

  return (
    <form onSubmit={handleSave} className="space-y-2">
      <Input
        aria-label="Note name"
        value={name}
        onChange={(event) => setName(event.target.value)}
        maxLength={200}
        disabled={isSaving}
      />
      <textarea
        aria-label="Note content"
        value={content}
        onChange={(event) => setContent(event.target.value)}
        rows={6}
        disabled={isSaving}
        className="w-full rounded-md border border-input bg-transparent px-3 py-2 text-base shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
      />

      {errorMessage && <p className="text-base text-destructive">{errorMessage}</p>}

      <div className="flex justify-end gap-2">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isSaving}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSaving || name.trim() === ""}>
          {isSaving ? "Saving…" : "Save"}
        </Button>
      </div>
    </form>
  );
}
