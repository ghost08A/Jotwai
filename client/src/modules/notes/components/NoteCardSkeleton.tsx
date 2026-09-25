export function NoteCardSkeleton() {
  return (
    <div className="animate-pulse rounded-lg border border-border bg-card p-4 shadow-sm">
      <div className="h-4 w-3/4 rounded bg-muted" />
      <div className="mt-2 h-4 w-1/2 rounded bg-muted" />
    </div>
  );
}
