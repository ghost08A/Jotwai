interface CategoryChipProps {
    label: string;
    isActive: boolean;
    onClick: () => void;
    disabled?: boolean;
}

// Toggle-chip visual for a Category tag - shared by NoteCategoryChips (attach/detach
// on a specific Note) and CategoryFilterChips (a standalone multi-select filter).
export function CategoryChip({ label, isActive, onClick, disabled = false }: CategoryChipProps) {
    return (
        <button
            type="button"
            aria-pressed={isActive}
            onClick={onClick}
            disabled={disabled}
            className={`cursor-pointer rounded-full border px-2.5 py-0.5 text-xs font-medium disabled:cursor-not-allowed disabled:opacity-50 ${
                isActive
                    ? "border-primary bg-primary text-primary-foreground"
                    : "border-input bg-transparent text-muted-foreground hover:bg-accent"
            }`}
        >
            {label}
        </button>
    );
}
