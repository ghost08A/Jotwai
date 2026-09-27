const BASE_NAME = "Untitled";

// "+ New topic" (ticket 044) creates a placeholder-named root Topic immediately instead of
// asking for a name upfront - this picks a name that won't collide with the sibling-name
// uniqueness constraint (case-insensitive, per parent) if the user creates several in a row
// without renaming any of them yet.
export function nextUntitledTopicName(existingRootNames: string[]): string {
  const normalized = new Set(existingRootNames.map((name) => name.trim().toLowerCase()));

  if (!normalized.has(BASE_NAME.toLowerCase())) return BASE_NAME;

  let suffix = 2;
  while (normalized.has(`${BASE_NAME} ${suffix}`.toLowerCase())) suffix += 1;

  return `${BASE_NAME} ${suffix}`;
}
