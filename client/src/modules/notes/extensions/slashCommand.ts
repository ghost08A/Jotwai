import { Extension } from "@tiptap/core";
import type { Editor, Range } from "@tiptap/core";
import Suggestion from "@tiptap/suggestion";
import { ReactRenderer } from "@tiptap/react";
import { SlashCommandMenu } from "../components/SlashCommandMenu";
import type { SlashCommandMenuHandle } from "../components/SlashCommandMenu";

export interface SlashCommandItem {
  title: string;
  run: (editor: Editor, range: Range) => void;
}

// Block-level items only (Notion's own slash menu works the same way) -
// inline marks like bold/italic go through the toolbar or markdown input
// rules instead, not this menu.
export const SLASH_COMMAND_ITEMS: SlashCommandItem[] = [
  {
    title: "Text",
    run: (editor, range) => editor.chain().focus().deleteRange(range).setParagraph().run(),
  },
  {
    title: "Heading 1",
    run: (editor, range) => editor.chain().focus().deleteRange(range).setNode("heading", { level: 1 }).run(),
  },
  {
    title: "Heading 2",
    run: (editor, range) => editor.chain().focus().deleteRange(range).setNode("heading", { level: 2 }).run(),
  },
  {
    title: "Heading 3",
    run: (editor, range) => editor.chain().focus().deleteRange(range).setNode("heading", { level: 3 }).run(),
  },
  {
    title: "Bulleted list",
    run: (editor, range) => editor.chain().focus().deleteRange(range).toggleBulletList().run(),
  },
  {
    title: "Numbered list",
    run: (editor, range) => editor.chain().focus().deleteRange(range).toggleOrderedList().run(),
  },
  {
    title: "To-do list",
    run: (editor, range) => editor.chain().focus().deleteRange(range).toggleTaskList().run(),
  },
  {
    title: "Quote",
    run: (editor, range) => editor.chain().focus().deleteRange(range).setBlockquote().run(),
  },
  {
    title: "Code block",
    run: (editor, range) => editor.chain().focus().deleteRange(range).toggleCodeBlock().run(),
  },
  {
    title: "Divider",
    run: (editor, range) => editor.chain().focus().deleteRange(range).setHorizontalRule().run(),
  },
];

export function filterSlashCommandItems(query: string, items: SlashCommandItem[] = SLASH_COMMAND_ITEMS): SlashCommandItem[] {
  const normalized = query.trim().toLowerCase();
  if (!normalized) return items;
  return items.filter((item) => item.title.toLowerCase().includes(normalized));
}

// Matches SlashCommandMenu's max-h-72 (18rem) plus its padding - how much room the menu
// needs below the caret before it should flip above it instead of running off-screen.
const MENU_MAX_HEIGHT_PX = 300;

function positionMenu(element: HTMLElement, rect: DOMRect | null) {
  if (!rect) return;
  const spaceBelow = window.innerHeight - rect.bottom;
  const flipAbove = spaceBelow < MENU_MAX_HEIGHT_PX && rect.top > spaceBelow;

  element.style.position = "fixed";
  element.style.left = `${Math.min(rect.left, window.innerWidth - 240)}px`;
  element.style.top = flipAbove ? "auto" : `${rect.bottom + 4}px`;
  element.style.bottom = flipAbove ? `${window.innerHeight - rect.top + 4}px` : "auto";
  element.style.zIndex = "50";
}

export interface SlashCommandOptions {
  // Only offered when a parent Topic is known (ticket 042/GLOSSARY.md "Topic
  // Reference") - creating a subtopic needs somewhere to attach it.
  onCreateSubtopic?: (editor: Editor, range: Range) => void;
}

export const SlashCommand = Extension.create<SlashCommandOptions>({
  name: "slashCommand",

  addOptions() {
    return { onCreateSubtopic: undefined };
  },

  addProseMirrorPlugins() {
    const { onCreateSubtopic } = this.options;
    const items = (query: string): SlashCommandItem[] => {
      const allItems: SlashCommandItem[] = onCreateSubtopic
        ? [...SLASH_COMMAND_ITEMS, { title: "Subtopic", run: (editor, range) => onCreateSubtopic(editor, range) }]
        : SLASH_COMMAND_ITEMS;
      return filterSlashCommandItems(query, allItems);
    };

    return [
      Suggestion<SlashCommandItem>({
        editor: this.editor,
        char: "/",
        items: ({ query }) => items(query),
        command: ({ editor, range, props }) => props.run(editor, range),
        render: () => {
          let component: ReactRenderer<SlashCommandMenuHandle>;

          return {
            onStart: (props) => {
              component = new ReactRenderer(SlashCommandMenu, {
                props,
                editor: props.editor,
              });
              document.body.appendChild(component.element);
              positionMenu(component.element as HTMLElement, props.clientRect?.() ?? null);
            },
            onUpdate(props) {
              component.updateProps(props);
              positionMenu(component.element as HTMLElement, props.clientRect?.() ?? null);
            },
            onKeyDown(props) {
              if (props.event.key === "Escape") {
                component.destroy();
                component.element.remove();
                return true;
              }
              return component.ref?.onKeyDown(props) ?? false;
            },
            onExit() {
              component.element.remove();
              component.destroy();
            },
          };
        },
      }),
    ];
  },
});
