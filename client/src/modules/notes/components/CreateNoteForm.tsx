import { useState } from "react";
import type { FormEvent } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useCreateNote } from "../hooks/useCreateNote";
import { DuplicateNameWarning } from "./DuplicateNameWarning";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

// Creates a top-level note. A sub-note is created from inside its parent note, not from here.
export function CreateNoteForm() {
  const [name, setName] = useState("");
  const createNote = useCreateNote();

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    createNote.mutate({ name, content: "" }, { onSuccess: () => setName("") });
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
          {createNote.isPending ? "Creating…" : "New note"}
        </Button>
      </div>

      {createNote.isError && (
        <p className="text-base text-destructive">{getErrorMessage(createNote.error, "Failed to create note")}</p>
      )}
      {createNote.data?.hasDuplicateSiblingName && <DuplicateNameWarning name={createNote.data.name} />}
    </form>
  );
}
