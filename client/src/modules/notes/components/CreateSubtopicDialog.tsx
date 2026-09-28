import { useState } from "react";
import type { FormEvent } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useCreateTopic } from "../../organization/hooks/useCreateTopic";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import type { TopicResponse } from "../../organization/types";

interface CreateSubtopicDialogProps {
  parentId: string;
  onCreated: (topic: TopicResponse) => void;
  onCancel: () => void;
}

// Small modal opened by the "/subtopic" slash command (ticket 042) - unlike
// every other slash item, this one needs a name before it can do anything,
// so it can't run inline like a plain block transform.
export function CreateSubtopicDialog({ parentId, onCreated, onCancel }: CreateSubtopicDialogProps) {
  const [name, setName] = useState("");
  const createTopic = useCreateTopic();

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!name.trim()) return;

    createTopic.mutate({ name: name.trim(), parentId }, { onSuccess: onCreated });
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="w-full max-w-sm rounded-lg border border-border bg-card p-4 shadow-lg">
        <h2 className="mb-3 text-base font-medium text-card-foreground">New subtopic</h2>
        <form onSubmit={handleSubmit} className="space-y-3">
          <Input
            value={name}
            onChange={(event) => setName(event.target.value)}
            placeholder="Subtopic name"
            autoFocus
          />
          {createTopic.isError && (
            <p className="text-base text-destructive">{getErrorMessage(createTopic.error, "Failed to create topic")}</p>
          )}
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onCancel} disabled={createTopic.isPending}>
              Cancel
            </Button>
            <Button type="submit" disabled={createTopic.isPending || !name.trim()}>
              {createTopic.isPending ? "Creating…" : "Create"}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
