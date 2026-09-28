import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { useCategories } from "../hooks/useCategories";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { CategoryRow } from "./CategoryRow";
import { CreateCategoryForm } from "./CreateCategoryForm";

export function CategoriesPage() {
  const categories = useCategories();
  const [isAdding, setIsAdding] = useState(false);
  const sorted = [...(categories.data ?? [])].sort((a, b) => a.name.localeCompare(b.name));

  return (
    <div className="w-full space-y-4 px-6 py-6 md:px-12 lg:px-24">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-foreground">Categories</h1>
        {!isAdding && (
          <Button type="button" size="sm" onClick={() => setIsAdding(true)}>
            + New category
          </Button>
        )}
      </div>

      {isAdding && (
        <div className="rounded-lg border border-border bg-card p-3">
          <CreateCategoryForm onDone={() => setIsAdding(false)} />
        </div>
      )}

      <div className="rounded-lg border border-border bg-card p-2">
        {categories.isLoading && (
          <div className="space-y-2 p-2">
            {Array.from({ length: 3 }).map((_, index) => (
              <div key={index} className="h-6 animate-pulse rounded bg-muted" />
            ))}
          </div>
        )}

        {categories.isError && (
          <p className="p-2 text-base text-destructive">
            {getErrorMessage(categories.error, "Failed to load categories")}
          </p>
        )}

        {sorted.length === 0 && !categories.isLoading && (
          <p className="p-2 text-center text-base text-muted-foreground">No categories yet.</p>
        )}

        {sorted.map((category) => (
          <CategoryRow key={category.id} category={category} />
        ))}
      </div>
    </div>
  );
}
