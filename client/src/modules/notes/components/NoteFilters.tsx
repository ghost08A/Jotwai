import { Input } from "@/shared/components/ui/input";
import { CategoryFilterChips } from "../../organization/components/CategoryFilterChips";
import type { NoteTreeRow } from "../utils/flattenNoteTree";

interface NoteFiltersProps {
  query: string;
  onQueryChange: (query: string) => void;
  // Empty string means no parent filter.
  parentId: string;
  onParentIdChange: (parentId: string) => void;
  parentOptions: NoteTreeRow[];
  selectedCategoryIds: string[];
  onToggleCategory: (categoryId: string) => void;
}

export function NoteFilters({
  query,
  onQueryChange,
  parentId,
  onParentIdChange,
  parentOptions,
  selectedCategoryIds,
  onToggleCategory,
}: NoteFiltersProps) {
  return (
    <section aria-label="Find a note" className="space-y-3">
      <Input
        type="search"
        placeholder="Search names, content and categories…"
        aria-label="Search notes"
        value={query}
        onChange={(event) => onQueryChange(event.target.value)}
      />

      <select
        aria-label="Only notes under"
        value={parentId}
        onChange={(event) => onParentIdChange(event.target.value)}
        className="cursor-pointer rounded-md border border-input bg-transparent px-2 py-1 text-base text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
      >
        <option value="">All notes</option>
        {parentOptions.map((option) => (
          <option key={option.note.id} value={option.note.id}>
            {`Under: ${"— ".repeat(option.depth)}${option.note.name}`}
          </option>
        ))}
      </select>

      <CategoryFilterChips selectedCategoryIds={selectedCategoryIds} onToggle={onToggleCategory} />
    </section>
  );
}
