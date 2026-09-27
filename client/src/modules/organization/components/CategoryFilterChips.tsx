import { useCategories } from "../hooks/useCategories";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { CategoryChip } from "../../../shared/components/CategoryChip";

interface CategoryFilterChipsProps {
    selectedCategoryIds: string[];
    onToggle: (categoryId: string) => void;
}

// Same toggle-chip interaction as NoteCategoryChips, but standalone (local filter
// selection, not attach/detach mutations against a specific Note).
export function CategoryFilterChips({ selectedCategoryIds, onToggle }: CategoryFilterChipsProps) {
    const categories = useCategories();

    if (categories.isLoading) {
        return (
            <div className="flex flex-wrap gap-1.5">
                {Array.from({ length: 3 }).map((_, index) => (
                    <div key={index} className="h-6 w-16 animate-pulse rounded-full bg-muted" />
                ))}
            </div>
        );
    }

    if (categories.isError) {
        return <p className="text-xs text-destructive">{getErrorMessage(categories.error, "Failed to load categories")}</p>;
    }

    if (!categories.data || categories.data.length === 0) return null;

    return (
        <div className="flex flex-wrap gap-1.5">
            {categories.data.map((category) => (
                <CategoryChip
                    key={category.id}
                    label={category.name}
                    isActive={selectedCategoryIds.includes(category.id)}
                    onClick={() => onToggle(category.id)}
                />
            ))}
        </div>
    );
}
