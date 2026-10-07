import { useState } from "react";
import type { FormEvent } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useCreateNote } from "../hooks/useCreateNote";
import { DuplicateNameWarning } from "./DuplicateNameWarning";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

interface CreateNoteFormProps {
  // Undefined creates a top-level note.
  parentId?: string;
  submitLabel: string;
  onCreated?: () => void;
}

export function CreateNoteForm({ parentId, submitLabel, onCreated }: CreateNoteFormProps) {
  const [name, setName] = useState("");
  const createNote = useCreateNote();

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    createNote.mutate(
      { name, content: "", parentId },
      {
        onSuccess: () => {
          setName("");
          onCreated?.();
        },
      },
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-2">
      <div className="flex gap-2">
        <Input
          aria-label="Note name"
          placeholder="Note name"
          value={name}
          onChange={(event) => setName(event.target.value)}
          maxLength={200}
          disabled={createNote.isPending}
        />
        <Button type="submit" disabled={createNote.isPending || name.trim() === ""}>
          {createNote.isPending ? "Creating…" : submitLabel}
        </Button>
      </div>

      {createNote.isError && (
        <p className="text-base text-destructive">{getErrorMessage(createNote.error, "Failed to create note")}</p>
      )}
      {createNote.data?.hasDuplicateSiblingName && <DuplicateNameWarning name={createNote.data.name} />}
    </form>
  );
}
