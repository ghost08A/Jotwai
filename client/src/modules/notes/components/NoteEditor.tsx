import { useEffect } from "react";
import { EditorContent, useEditor } from "@tiptap/react";
import type { Editor } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import { Markdown } from "tiptap-markdown";
import type { MarkdownStorage } from "tiptap-markdown";
import { NoteEditorToolbar } from "./NoteEditorToolbar";
import { TitleDocument } from "../extensions/titleDocument";
import { TitleLevelGuard } from "../extensions/titleLevelGuard";
import { SlashCommand } from "../extensions/slashCommand";
import { ensureLeadingHeading } from "../utils/ensureLeadingHeading";

// tiptap-markdown doesn't ship a `@tiptap/core` Storage module augmentation,
// so `editor.storage.markdown` isn't visible to the type checker on its own.
function getMarkdown(editor: Editor): string {
  return (editor.storage as unknown as { markdown: MarkdownStorage }).markdown.getMarkdown();
}

interface NoteEditorProps {
  content: string;
  onChange: (markdown: string) => void;
  editable?: boolean;
}

// Note content types (GLOSSARY.md "Note"): plain text, heading (levels 1-3),
// blockquote, bulleted list, numbered list, code block, horizontal rule, plus
// bold/italic/strike/inline-code marks (ticket 037 - Notion-like editor).
// `link`/`underline` stay disabled - not part of the confirmed scope.
// `document` is disabled too: TitleDocument replaces it so the Note's first
// block is always a level-1 heading.
export function NoteEditor({ content, onChange, editable = true }: NoteEditorProps) {
  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        document: false,
        link: false,
        underline: false,
        // StarterKit's TrailingNode extension picks its node type from the
        // schema's default content match, which for TitleDocument's `heading
        // block*` resolves to "heading" (the required first type) instead of
        // "paragraph" - force it explicitly so the trailing node isn't
        // another empty heading.
        trailingNode: { node: "paragraph" },
      }),
      TitleDocument,
      TitleLevelGuard,
      SlashCommand,
      // Markdown is the Note's source of truth (ARCHITECTURE.md) - the editor
      // is only a view onto it. html: false so nothing but Markdown ever
      // round-trips through the editor.
      Markdown.configure({ html: false }),
    ],
    content: ensureLeadingHeading(content),
    editable,
    onUpdate: ({ editor: updatedEditor }) => onChange(getMarkdown(updatedEditor)),
    editorProps: {
      attributes: {
        class:
          "min-h-40 focus:outline-none " +
          "[&_h1]:mb-2 [&_h1]:mt-1 [&_h1]:text-3xl [&_h1]:font-bold " +
          "[&_h2]:mb-2 [&_h2]:mt-4 [&_h2]:text-xl [&_h2]:font-semibold " +
          "[&_h3]:mb-1 [&_h3]:mt-3 [&_h3]:text-lg [&_h3]:font-semibold " +
          "[&_blockquote]:my-2 [&_blockquote]:border-l-2 [&_blockquote]:border-neutral-300 [&_blockquote]:pl-3 [&_blockquote]:text-neutral-600 " +
          "[&_hr]:my-4 [&_hr]:border-neutral-200 " +
          "[&_code]:rounded [&_code]:bg-neutral-100 [&_code]:px-1 [&_code]:py-0.5 [&_code]:font-mono [&_code]:text-sm " +
          "[&_ul]:list-disc [&_ul]:pl-6 [&_ol]:list-decimal [&_ol]:pl-6 [&_p]:my-1 " +
          "[&_pre]:my-2 [&_pre]:overflow-x-auto [&_pre]:rounded-md [&_pre]:bg-neutral-100 [&_pre]:p-3 [&_pre]:font-mono [&_pre]:text-sm " +
          "[&_pre_code]:bg-transparent [&_pre_code]:p-0",
      },
    },
  });

  useEffect(() => {
    if (!editor) return;

    // Only re-sync when content changes from outside (e.g. switching to a
    // different Note) - comparing against the editor's own serialized output
    // avoids fighting the user's cursor on every local keystroke.
    const currentMarkdown = getMarkdown(editor);
    if (content !== currentMarkdown) {
      editor.commands.setContent(ensureLeadingHeading(content), { emitUpdate: false });
    }
  }, [content, editor]);

  if (!editor) return null;

  return (
    <div className="flex flex-col gap-2">
      {editable && <NoteEditorToolbar editor={editor} />}
      <EditorContent editor={editor} />
    </div>
  );
}
