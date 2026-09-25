import { useEffect } from "react";
import { EditorContent, useEditor } from "@tiptap/react";
import type { Editor } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import { Markdown } from "tiptap-markdown";
import type { MarkdownStorage } from "tiptap-markdown";
import { NoteEditorToolbar } from "./NoteEditorToolbar";

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

// Note content types (GLOSSARY.md "Note"): plain text, bulleted list, numbered
// list, code block - only. StarterKit's other marks/nodes (heading, bold,
// italic, strike, blockquote, horizontalRule, inline code, link, underline)
// are disabled rather than left registered with no toolbar control for them.
export function NoteEditor({ content, onChange, editable = true }: NoteEditorProps) {
  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        heading: false,
        bold: false,
        italic: false,
        strike: false,
        blockquote: false,
        horizontalRule: false,
        code: false,
        link: false,
        underline: false,
      }),
      // Markdown is the Note's source of truth (ARCHITECTURE.md) - the editor
      // is only a view onto it. html: false so nothing but Markdown ever
      // round-trips through the editor.
      Markdown.configure({ html: false }),
    ],
    content,
    editable,
    onUpdate: ({ editor: updatedEditor }) => onChange(getMarkdown(updatedEditor)),
    editorProps: {
      attributes: {
        class:
          "min-h-40 rounded-md border border-neutral-300 px-3 py-2 focus:border-neutral-500 focus:outline-none " +
          "[&_ul]:list-disc [&_ul]:pl-6 [&_ol]:list-decimal [&_ol]:pl-6 [&_p]:my-1 " +
          "[&_pre]:my-2 [&_pre]:overflow-x-auto [&_pre]:rounded-md [&_pre]:bg-neutral-100 [&_pre]:p-3 [&_pre]:font-mono [&_pre]:text-sm",
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
      editor.commands.setContent(content, { emitUpdate: false });
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
