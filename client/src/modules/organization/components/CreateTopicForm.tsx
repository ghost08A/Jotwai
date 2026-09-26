import { useState } from "react";
import type { FormEvent } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useCreateTopic } from "../hooks/useCreateTopic";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

interface CreateTopicFormProps {
  parentId?: string;
  onDone: () => void;
}

export function CreateTopicForm({ parentId, onDone }: CreateTopicFormProps) {
  const [name, setName] = useState("");
  const createTopic = useCreateTopic();

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!name.trim()) return;

    createTopic.mutate(
      { name: name.trim(), parentId },
      {
        onSuccess: () => {
          setName("");
          onDone();
        },
      },
    );
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-wrap items-center gap-2">
      <Input
        value={name}
        onChange={(event) => setName(event.target.value)}
        placeholder={parentId ? "Subtopic name" : "New topic name"}
        className="h-8 max-w-xs"
        autoFocus
      />
      <Button type="submit" size="sm" disabled={createTopic.isPending || !name.trim()}>
        {createTopic.isPending ? "Adding…" : "Add"}
      </Button>
      <Button type="button" size="sm" variant="outline" onClick={onDone}>
        Cancel
      </Button>
      {createTopic.isError && (
        <span className="text-xs text-destructive">
          {getErrorMessage(createTopic.error, "Failed to create topic")}
        </span>
      )}
    </form>
  );
}
