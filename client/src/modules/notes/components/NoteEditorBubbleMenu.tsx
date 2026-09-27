import type { ReactNode } from "react";
import { useEditorState } from "@tiptap/react";
import type { Editor } from "@tiptap/react";
import { BubbleMenu } from "@tiptap/react/menus";

interface NoteEditorBubbleMenuProps {
  editor: Editor;
}

// Minimal, non-exhaustive language list - the language select is metadata on
// the code block (GLOSSARY.md "Search"), not a syntax-highlighting config, so
// any value is accepted (the select allows a free "other" fallback via the
// blank option, not built here beyond what a base editor needs).
const CODE_BLOCK_LANGUAGES = [
  { value: "", label: "Plain text" },
  { value: "javascript", label: "JavaScript" },
  { value: "typescript", label: "TypeScript" },
  { value: "python", label: "Python" },
  { value: "csharp", label: "C#" },
  { value: "java", label: "Java" },
  { value: "sql", label: "SQL" },
  { value: "bash", label: "Bash" },
  { value: "json", label: "JSON" },
  { value: "html", label: "HTML" },
  { value: "css", label: "CSS" },
];

// Notion-style: no persistent toolbar - creating a new block goes through the
// "/" slash menu (SlashCommand), and reformatting existing content shows this
// menu the moment text is selected. A code block has no text selection to
// reformat (marks don't apply inside it), so it shows for a bare cursor there
// instead, offering just the language picker.
export function NoteEditorBubbleMenu({ editor }: NoteEditorBubbleMenuProps) {
  const {
    isBulletListActive,
    isOrderedListActive,
    isCodeBlockActive,
    codeBlockLanguage,
    isHeading2Active,
    isHeading3Active,
    isBoldActive,
    isItalicActive,
    isStrikeActive,
    isCodeActive,
    isBlockquoteActive,
  } = useEditorState({
    editor,
    selector: ({ editor: currentEditor }) => ({
      isBulletListActive: currentEditor.isActive("bulletList"),
      isOrderedListActive: currentEditor.isActive("orderedList"),
      isCodeBlockActive: currentEditor.isActive("codeBlock"),
      codeBlockLanguage: (currentEditor.getAttributes("codeBlock").language as string | null) ?? "",
      isHeading2Active: currentEditor.isActive("heading", { level: 2 }),
      isHeading3Active: currentEditor.isActive("heading", { level: 3 }),
      isBoldActive: currentEditor.isActive("bold"),
      isItalicActive: currentEditor.isActive("italic"),
      isStrikeActive: currentEditor.isActive("strike"),
      isCodeActive: currentEditor.isActive("code"),
      isBlockquoteActive: currentEditor.isActive("blockquote"),
    }),
  });

  return (
    <BubbleMenu
      editor={editor}
      shouldShow={({ editor: currentEditor, from, to }) => currentEditor.isActive("codeBlock") || from !== to}
      className="flex flex-wrap items-center gap-1 rounded-md border border-border bg-popover p-1 shadow-md"
    >
      {isCodeBlockActive ? (
        <select
          aria-label="Code block language"
          value={codeBlockLanguage}
          onChange={(event) =>
            editor.chain().focus().updateAttributes("codeBlock", { language: event.target.value }).run()
          }
          className="cursor-pointer rounded-md border border-input bg-background px-2 py-1 text-sm focus:border-ring focus:outline-none"
        >
          {CODE_BLOCK_LANGUAGES.map((language) => (
            <option key={language.value} value={language.value}>
              {language.label}
            </option>
          ))}
        </select>
      ) : (
        <>
          {/* The note's first block is always a level-1 heading (ticket 037) -
              only levels 2/3 are offered here as body-content subheadings. */}
          <BubbleMenuButton
            label="Heading 2"
            active={isHeading2Active}
            onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()}
          >
            H2
          </BubbleMenuButton>

          <BubbleMenuButton
            label="Heading 3"
            active={isHeading3Active}
            onClick={() => editor.chain().focus().toggleHeading({ level: 3 }).run()}
          >
            H3
          </BubbleMenuButton>

          <BubbleMenuButton label="Bold" active={isBoldActive} onClick={() => editor.chain().focus().toggleBold().run()}>
            B
          </BubbleMenuButton>

          <BubbleMenuButton
            label="Italic"
            active={isItalicActive}
            onClick={() => editor.chain().focus().toggleItalic().run()}
          >
            I
          </BubbleMenuButton>

          <BubbleMenuButton
            label="Strikethrough"
            active={isStrikeActive}
            onClick={() => editor.chain().focus().toggleStrike().run()}
          >
            S
          </BubbleMenuButton>

          <BubbleMenuButton
            label="Inline code"
            active={isCodeActive}
            onClick={() => editor.chain().focus().toggleCode().run()}
          >
            {"<>"}
          </BubbleMenuButton>

          <BubbleMenuButton
            label="Quote"
            active={isBlockquoteActive}
            onClick={() => editor.chain().focus().toggleBlockquote().run()}
          >
            "
          </BubbleMenuButton>

          <BubbleMenuButton
            label="Bulleted list"
            active={isBulletListActive}
            onClick={() => editor.chain().focus().toggleBulletList().run()}
          >
            •≡
          </BubbleMenuButton>

          <BubbleMenuButton
            label="Numbered list"
            active={isOrderedListActive}
            onClick={() => editor.chain().focus().toggleOrderedList().run()}
          >
            1.≡
          </BubbleMenuButton>

          <BubbleMenuButton
            label="Code block"
            active={isCodeBlockActive}
            onClick={() => editor.chain().focus().toggleCodeBlock().run()}
          >
            {"</>"}
          </BubbleMenuButton>
        </>
      )}
    </BubbleMenu>
  );
}

interface BubbleMenuButtonProps {
  label: string;
  active: boolean;
  onClick: () => void;
  children: ReactNode;
}

function BubbleMenuButton({ label, active, onClick, children }: BubbleMenuButtonProps) {
  return (
    <button
      type="button"
      aria-label={label}
      aria-pressed={active}
      title={label}
      onClick={onClick}
      className={`cursor-pointer rounded-md px-2 py-1 text-sm font-medium hover:bg-accent active:bg-accent/70 ${
        active ? "bg-accent text-accent-foreground" : "text-muted-foreground"
      }`}
    >
      {children}
    </button>
  );
}
