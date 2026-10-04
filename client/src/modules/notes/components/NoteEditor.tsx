import { useEffect, useRef } from "react";
import { EditorContent, useEditor } from "@tiptap/react";
import type { Editor, Range } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import Placeholder from "@tiptap/extension-placeholder";
import { TaskItem, TaskList } from "@tiptap/extension-list";
import { Markdown } from "tiptap-markdown";
import type { MarkdownStorage } from "tiptap-markdown";
import { NoteEditorBubbleMenu } from "./NoteEditorBubbleMenu";
import { ContentPolishButton } from "./ContentPolishButton";
import { SlashCommand } from "../extensions/slashCommand";
import { TopicReference } from "../extensions/topicReference";
import { useTopics } from "../../organization/hooks/useTopics";
import { useCreateTopic } from "../../organization/hooks/useCreateTopic";
import { nextUntitledTopicName } from "../../organization/utils/nextUntitledTopicName";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";

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
// Notion-like editor), to-do list and link (ticket 047). `underline` stays disabled -
// not part of the confirmed scope; Topic Reference has its own dedicated node instead
// of the link mark (ticket 042) and is rewritten out of the anchor tags before the
// link mark ever sees them. Link's default URI allow-list already rejects javascript:.
export function NoteEditor({ content, onChange, editable = true, topicId }: NoteEditorProps) {
  const topics = useTopics();
  const createTopic = useCreateTopic();

  // The editor's extensions are built once, so "/subtopic" below can't close over fresh
  // render values - it reads the latest mutation and sibling names through these refs.
  const createTopicRef = useRef(createTopic);
  createTopicRef.current = createTopic;
  const siblingNamesRef = useRef<string[]>([]);
  siblingNamesRef.current = (topics.data ?? []).filter((topic) => topic.parentId === topicId).map((topic) => topic.name);

  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        // Clicking a link opens it in a new tab (target/rel are Link's defaults).
        link: { openOnClick: true, autolink: true },
        underline: false,
      }),
      TaskList,
      TaskItem.configure({ nested: true }),
      Placeholder.configure({ placeholder: "Write, or press '/' for commands…" }),
      TopicReference,
      SlashCommand.configure({
        onCreateSubtopic: topicId
          ? (editorInstance: Editor, range: Range) => {
              // Notion-style "/page": no name prompt - create an "Untitled" child right away
              // (capture first, name it later by opening it) and drop the reference in.
              // Editing is locked while the request is in flight so insertPos can't drift.
              editorInstance.chain().focus().deleteRange(range).run();
              editorInstance.setEditable(false);
              const insertPos = editorInstance.state.selection.from;
              createTopicRef.current.mutate(
                { name: nextUntitledTopicName(siblingNamesRef.current), parentId: topicId },
                {
                  onSuccess: (topic) => {
                    editorInstance
                      .chain()
                      .focus()
                      .insertContentAt(insertPos, { type: "topicReference", attrs: { topicId: topic.id, name: topic.name } })
                      .run();
                    editorInstance.setEditable(editable);
                  },
                  onError: () => editorInstance.setEditable(editable),
                },
              );
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
    // Capture-first: an empty editable document is ready to type into straight away.
    autofocus: editable && !content.trim() ? "start" : false,
    onUpdate: ({ editor: updatedEditor }) => onChange(getMarkdown(updatedEditor)),
    editorProps: {
      attributes: {
        class:
          "min-h-64 text-lg focus:outline-none " +
          "[&_p.is-empty::before]:pointer-events-none [&_p.is-empty::before]:float-left [&_p.is-empty::before]:h-0 [&_p.is-empty::before]:text-muted-foreground [&_p.is-empty::before]:content-[attr(data-placeholder)] " +
          "[&_h1]:mb-2 [&_h1]:mt-1 [&_h1]:text-3xl [&_h1]:font-bold " +
          "[&_h2]:mb-2 [&_h2]:mt-4 [&_h2]:text-xl [&_h2]:font-semibold " +
          "[&_h3]:mb-1 [&_h3]:mt-3 [&_h3]:text-lg [&_h3]:font-semibold " +
          "[&_blockquote]:my-2 [&_blockquote]:border-l-2 [&_blockquote]:border-border [&_blockquote]:pl-3 [&_blockquote]:text-muted-foreground " +
          "[&_hr]:my-4 [&_hr]:border-border " +
          "[&_a]:cursor-pointer [&_a]:underline [&_a]:decoration-muted-foreground [&_a]:underline-offset-4 " +
          "[&_ul[data-type=taskList]]:list-none [&_ul[data-type=taskList]]:pl-0 [&_li[data-checked]]:flex [&_li[data-checked]]:items-start [&_li[data-checked]]:gap-2 " +
          "[&_li[data-checked]>label]:mt-2 [&_li[data-checked]_input]:cursor-pointer [&_li[data-checked]_input]:accent-primary " +
          "[&_li[data-checked=true]>div]:text-muted-foreground [&_li[data-checked=true]>div]:line-through " +
          "[&_code]:rounded [&_code]:bg-muted [&_code]:px-1 [&_code]:py-0.5 [&_code]:font-mono [&_code]:text-base " +
          "[&_ul]:list-disc [&_ul]:pl-6 [&_ol]:list-decimal [&_ol]:pl-6 [&_p]:my-1 " +
          "[&_pre]:my-2 [&_pre]:overflow-x-auto [&_pre]:rounded-md [&_pre]:bg-muted [&_pre]:p-3 [&_pre]:font-mono [&_pre]:text-base " +
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

  if (!editor) return null;

  return (
    <div className="relative flex flex-col gap-2">
      {editable && <NoteEditorBubbleMenu editor={editor} />}
      <EditorContent editor={editor} />
      {createTopic.isError && (
        <p role="alert" className="text-base text-destructive">
          {getErrorMessage(createTopic.error, "Failed to create subtopic")}
        </p>
      )}
      {editable && (
        <div className="flex justify-end">
          <ContentPolishButton content={content} onApply={onChange} />
        </div>
      )}
    </div>
  );
}
