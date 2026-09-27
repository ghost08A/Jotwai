import { useEffect, useState } from "react";
import { EditorContent, useEditor } from "@tiptap/react";
import type { Editor, Range } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import { Markdown } from "tiptap-markdown";
import type { MarkdownStorage } from "tiptap-markdown";
import { NoteEditorBubbleMenu } from "./NoteEditorBubbleMenu";
import { CreateSubtopicDialog } from "./CreateSubtopicDialog";
import { SlashCommand } from "../extensions/slashCommand";
import { TopicReference } from "../extensions/topicReference";
import type { TopicResponse } from "../../organization/types";

// tiptap-markdown doesn't ship a `@tiptap/core` Storage module augmentation,
// so `editor.storage.markdown` isn't visible to the type checker on its own.
function getMarkdown(editor: Editor): string {
  return (editor.storage as unknown as { markdown: MarkdownStorage }).markdown.getMarkdown();
}

interface NoteEditorProps {
  content: string;
  onChange: (markdown: string) => void;
  editable?: boolean;
  // The Topic this Note currently belongs to - lets "/subtopic" (ticket 042)
  // create its new Topic as a child of this one. The item is hidden from the
  // slash menu entirely when this is undefined (nothing to attach it under).
  topicId?: string;
}

// Note content types (GLOSSARY.md "Note"): plain text, heading (levels 1-3),
// blockquote, bulleted list, numbered list, code block, horizontal rule,
// Topic Reference, plus bold/italic/strike/inline-code marks (ticket 037 -
// Notion-like editor). `link`/`underline` stay disabled - not part of the
// confirmed scope; Topic Reference has its own dedicated node instead of the
// generic link mark (ticket 042).
export function NoteEditor({ content, onChange, editable = true, topicId }: NoteEditorProps) {
  const [subtopicPrompt, setSubtopicPrompt] = useState<{ insertPos: number } | null>(null);

  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        link: false,
        underline: false,
      }),
      TopicReference,
      SlashCommand.configure({
        onCreateSubtopic: topicId
          ? (editorInstance: Editor, range: Range) => {
              editorInstance.chain().focus().deleteRange(range).run();
              editorInstance.setEditable(false);
              setSubtopicPrompt({ insertPos: editorInstance.state.selection.from });
            }
          : undefined,
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
          "min-h-40 focus:outline-none " +
          "[&_h1]:mb-2 [&_h1]:mt-1 [&_h1]:text-3xl [&_h1]:font-bold " +
          "[&_h2]:mb-2 [&_h2]:mt-4 [&_h2]:text-xl [&_h2]:font-semibold " +
          "[&_h3]:mb-1 [&_h3]:mt-3 [&_h3]:text-lg [&_h3]:font-semibold " +
          "[&_blockquote]:my-2 [&_blockquote]:border-l-2 [&_blockquote]:border-border [&_blockquote]:pl-3 [&_blockquote]:text-muted-foreground " +
          "[&_hr]:my-4 [&_hr]:border-border " +
          "[&_code]:rounded [&_code]:bg-muted [&_code]:px-1 [&_code]:py-0.5 [&_code]:font-mono [&_code]:text-sm " +
          "[&_ul]:list-disc [&_ul]:pl-6 [&_ol]:list-decimal [&_ol]:pl-6 [&_p]:my-1 " +
          "[&_pre]:my-2 [&_pre]:overflow-x-auto [&_pre]:rounded-md [&_pre]:bg-muted [&_pre]:p-3 [&_pre]:font-mono [&_pre]:text-sm " +
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
      editor.commands.setContent(content, { emitUpdate: false });
    }
  }, [content, editor]);

  function handleSubtopicCreated(topic: TopicResponse) {
    if (!editor || !subtopicPrompt) return;

    editor
      .chain()
      .focus()
      .insertContentAt(subtopicPrompt.insertPos, { type: "topicReference", attrs: { topicId: topic.id, name: topic.name } })
      .run();
    editor.setEditable(editable);
    setSubtopicPrompt(null);
  }

  function handleSubtopicCancelled() {
    editor?.setEditable(editable);
    setSubtopicPrompt(null);
  }

  if (!editor) return null;

  return (
    <div className="flex flex-col gap-2">
      {editable && <NoteEditorBubbleMenu editor={editor} />}
      <EditorContent editor={editor} />
      {subtopicPrompt && topicId && (
        <CreateSubtopicDialog parentId={topicId} onCreated={handleSubtopicCreated} onCancel={handleSubtopicCancelled} />
      )}
    </div>
  );
}
