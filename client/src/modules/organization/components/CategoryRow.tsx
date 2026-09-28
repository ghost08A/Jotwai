import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useRenameCategory } from "../hooks/useRenameCategory";
import { useDeleteCategory } from "../hooks/useDeleteCategory";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import type { CategoryResponse } from "../types";

interface CategoryRowProps {
  category: CategoryResponse;
}

export function CategoryRow({ category }: CategoryRowProps) {
  const [isRenaming, setIsRenaming] = useState(false);
  const [nameDraft, setNameDraft] = useState(category.name);
  const renameCategory = useRenameCategory();
  const deleteCategory = useDeleteCategory();

  function startRenaming() {
    setNameDraft(category.name);
    setIsRenaming(true);
  }

  function handleRename() {
    if (!nameDraft.trim()) return;
    renameCategory.mutate(
      { id: category.id, request: { name: nameDraft.trim() } },
      { onSuccess: () => setIsRenaming(false) },
    );
  }

  return (
    <div>
      <div className="flex flex-wrap items-center gap-2 rounded-md px-2 py-1.5 hover:bg-accent">
        {isRenaming ? (
          <>
            <Input
              value={nameDraft}
              onChange={(event) => setNameDraft(event.target.value)}
              className="h-8 max-w-xs"
              autoFocus
            />
            <Button size="sm" onClick={handleRename} disabled={renameCategory.isPending}>
              {renameCategory.isPending ? "Saving…" : "Save"}
            </Button>
            <Button size="sm" variant="outline" onClick={() => setIsRenaming(false)}>
              Cancel
            </Button>
          </>
        ) : (
          <>
            <span className="flex-1 text-base text-foreground">{category.name}</span>
            <Button type="button" size="sm" variant="ghost" onClick={startRenaming}>
              Rename
            </Button>
            <Button
              type="button"
              size="sm"
              variant="ghost"
              className="text-destructive hover:text-destructive"
              onClick={() => deleteCategory.mutate(category.id)}
              disabled={deleteCategory.isPending}
            >
              {deleteCategory.isPending ? "Deleting…" : "Delete"}
            </Button>
          </>
        )}
      </div>

      {renameCategory.isError && (
        <p className="px-2 text-xs text-destructive">
          {getErrorMessage(renameCategory.error, "Failed to rename category")}
        </p>
      )}
      {deleteCategory.isError && (
        <p className="px-2 text-xs text-destructive">
          {getErrorMessage(deleteCategory.error, "Failed to delete category")}
        </p>
      )}
    </div>
  );
}
