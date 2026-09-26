// Normalizes a Note's stored Markdown so its first non-blank line is always
// a level-1 heading (GLOSSARY.md "Note", ticket 037) - Notes saved before
// this decision don't have this, so loading one needs a transparent
// migration rather than failing the editor's stricter document schema.
export function ensureLeadingHeading(markdown: string): string {
  if (!markdown.trim()) return markdown;

  const lines = markdown.split("\n");
  const firstLineIndex = lines.findIndex((line) => line.trim() !== "");
  if (firstLineIndex === -1) return markdown;

  const withoutExistingMarker = lines[firstLineIndex].replace(/^#{1,6}\s*/, "");
  lines[firstLineIndex] = `# ${withoutExistingMarker}`;

  return lines.join("\n");
}
