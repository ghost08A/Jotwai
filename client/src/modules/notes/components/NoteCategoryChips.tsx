import { useAttachCategory } from "../hooks/useAttachCategory";
import { useDetachCategory } from "../hooks/useDetachCategory";
import { useCategories } from "../../organization/hooks/useCategories";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { CategoryChip } from "../../../shared/components/CategoryChip";

interface NoteCategoryChipsProps {
  noteId: string;
  categoryIds: string[];
}

// Category is a many-to-many tag on a Note (GLOSSARY.md "Category"), unlike
// Topic's single-value assignment - a toggleable chip per Category reads
// clearer here than a multi-select, and works on touch without ctrl/cmd-click.
export function NoteCategoryChips({ noteId, categoryIds }: NoteCategoryChipsProps) {
  const categories = useCategories();
  const attachCategory = useAttachCategory();
  const detachCategory = useDetachCategory();
  const isPending = attachCategory.isPending || detachCategory.isPending;

  if (!categories.data || categories.data.length === 0) return null;

  function toggle(categoryId: string, isAttached: boolean) {
    if (isAttached) {
      detachCategory.mutate({ noteId, categoryId });
    } else {
      attachCategory.mutate({ noteId, categoryId });
    }
  }

  return (
    <div className="mb-3">
      <div className="flex flex-wrap gap-1.5">
        {categories.data.map((category) => {
          const isAttached = categoryIds.includes(category.id);
          return (
            <CategoryChip
              key={category.id}
              label={category.name}
              isActive={isAttached}
              onClick={() => toggle(category.id, isAttached)}
              disabled={isPending}
            />
          );
        })}
      </div>

      {(attachCategory.isError || detachCategory.isError) && (
        <p className="mt-1 text-xs text-destructive">
          {getErrorMessage(attachCategory.error ?? detachCategory.error, "Failed to update categories")}
        </p>
      )}
    </div>
  );
}
