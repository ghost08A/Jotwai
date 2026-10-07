export function DuplicateNameWarning({ name }: { name: string }) {
  return (
    <p role="status" className="mt-2 text-base text-muted-foreground">
      Another note under the same parent is also named “{name}”.
    </p>
  );
}
