import type { ReactNode } from "react";
import { useEditorState } from "@tiptap/react";
import type { Editor } from "@tiptap/react";

interface NoteEditorToolbarProps {
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

export function NoteEditorToolbar({ editor }: NoteEditorToolbarProps) {
  // Tiptap v3's useEditor no longer re-renders on every transaction by
  // default - selection-only changes (e.g. moving the cursor into a code
  // block) wouldn't otherwise update these active/attribute reads.
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
    <div className="flex flex-wrap items-center gap-1 rounded-md border border-border bg-muted p-1">
      {/* The note's first block is always a level-1 heading (ticket 037) -
          only levels 2/3 are offered here as body-content subheadings. */}
      <ToolbarButton
        label="Heading 2"
        active={isHeading2Active}
        onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()}
      >
        H2
      </ToolbarButton>

      <ToolbarButton
        label="Heading 3"
        active={isHeading3Active}
        onClick={() => editor.chain().focus().toggleHeading({ level: 3 }).run()}
      >
        H3
      </ToolbarButton>

      <ToolbarButton label="Bold" active={isBoldActive} onClick={() => editor.chain().focus().toggleBold().run()}>
        B
      </ToolbarButton>

      <ToolbarButton
        label="Italic"
        active={isItalicActive}
        onClick={() => editor.chain().focus().toggleItalic().run()}
      >
        I
      </ToolbarButton>

      <ToolbarButton
        label="Strikethrough"
        active={isStrikeActive}
        onClick={() => editor.chain().focus().toggleStrike().run()}
      >
        S
      </ToolbarButton>

      <ToolbarButton label="Inline code" active={isCodeActive} onClick={() => editor.chain().focus().toggleCode().run()}>
        {"<>"}
      </ToolbarButton>

      <ToolbarButton
        label="Quote"
        active={isBlockquoteActive}
        onClick={() => editor.chain().focus().toggleBlockquote().run()}
      >
        "
      </ToolbarButton>

      <ToolbarButton
        label="Divider"
        active={false}
        onClick={() => editor.chain().focus().setHorizontalRule().run()}
      >
        ―
      </ToolbarButton>

      <ToolbarButton
        label="Bulleted list"
        active={isBulletListActive}
        onClick={() => editor.chain().focus().toggleBulletList().run()}
      >
        •≡
      </ToolbarButton>

      <ToolbarButton
        label="Numbered list"
        active={isOrderedListActive}
        onClick={() => editor.chain().focus().toggleOrderedList().run()}
      >
        1.≡
      </ToolbarButton>

      <ToolbarButton
        label="Code block"
        active={isCodeBlockActive}
        onClick={() => editor.chain().focus().toggleCodeBlock().run()}
      >
        {"</>"}
      </ToolbarButton>

      {isCodeBlockActive && (
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
      )}
    </div>
  );
}

interface ToolbarButtonProps {
  label: string;
  active: boolean;
  onClick: () => void;
  children: ReactNode;
}

function ToolbarButton({ label, active, onClick, children }: ToolbarButtonProps) {
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
