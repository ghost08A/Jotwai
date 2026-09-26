import { useState } from "react";
import type { FormEvent } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useCreateCategory } from "../hooks/useCreateCategory";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

interface CreateCategoryFormProps {
  onDone: () => void;
}

export function CreateCategoryForm({ onDone }: CreateCategoryFormProps) {
  const [name, setName] = useState("");
  const createCategory = useCreateCategory();

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!name.trim()) return;

    createCategory.mutate(
      { name: name.trim() },
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
        placeholder="New category name"
        className="h-8 max-w-xs"
        autoFocus
      />
      <Button type="submit" size="sm" disabled={createCategory.isPending || !name.trim()}>
        {createCategory.isPending ? "Adding…" : "Add"}
      </Button>
      <Button type="button" size="sm" variant="outline" onClick={onDone}>
        Cancel
      </Button>
      {createCategory.isError && (
        <span className="text-xs text-destructive">
          {getErrorMessage(createCategory.error, "Failed to create category")}
        </span>
      )}
    </form>
  );
}
