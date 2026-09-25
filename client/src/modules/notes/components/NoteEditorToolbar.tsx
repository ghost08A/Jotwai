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
  const { isBulletListActive, isOrderedListActive, isCodeBlockActive, codeBlockLanguage } = useEditorState({
    editor,
    selector: ({ editor: currentEditor }) => ({
      isBulletListActive: currentEditor.isActive("bulletList"),
      isOrderedListActive: currentEditor.isActive("orderedList"),
      isCodeBlockActive: currentEditor.isActive("codeBlock"),
      codeBlockLanguage: (currentEditor.getAttributes("codeBlock").language as string | null) ?? "",
    }),
  });

  return (
    <div className="flex flex-wrap items-center gap-1 rounded-md border border-neutral-200 bg-neutral-50 p-1">
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
          className="cursor-pointer rounded-md border border-neutral-300 bg-white px-2 py-1 text-sm focus:border-neutral-500 focus:outline-none"
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
      className={`cursor-pointer rounded-md px-2 py-1 text-sm font-medium hover:bg-neutral-200 active:bg-neutral-300 ${
        active ? "bg-neutral-200 text-neutral-900" : "text-neutral-600"
      }`}
    >
      {children}
    </button>
  );
}
