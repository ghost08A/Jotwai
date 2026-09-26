import { Node } from "@tiptap/core";

// Note's first block is always a level-1 heading (GLOSSARY.md "Note", ticket
// 037) - acts as a visual/semantic title without a separate Title field
// (ticket 030). ProseMirror refuses any transaction that would leave the doc
// without a leading heading, so no extra delete-guard is needed here.
export const TitleDocument = Node.create({
  name: "doc",
  topNode: true,
  content: "heading block*",
});
