const MAX_PREVIEW_LENGTH = 160;

// Topic cards on Home (ticket 045) show a short plain-text peek of the Topic's continuous
// document. Note content is Markdown (ADR-0008), so strip the syntax the editor can produce
// (headings, lists, quotes, code fences, inline marks) and collapse whitespace.
export function buildNotePreview(markdown: string | undefined): string {
  if (!markdown) return "";

  const plain = markdown
    // A leading "# Title" line just repeats the card title above the preview.
    .replace(/^\s*#\s[^\n]*\n?/, "")
    .replace(/```[^\n]*\n?/g, " ")
    .replace(/^\s{0,3}(#{1,6}|>|[-*+]|\d+\.|---+)\s*/gm, "")
    .replace(/(\*\*|__|~~|`|\*|_)/g, "")
    .replace(/\s+/g, " ")
    .trim();

  return plain.length > MAX_PREVIEW_LENGTH ? `${plain.slice(0, MAX_PREVIEW_LENGTH).trimEnd()}…` : plain;
}
