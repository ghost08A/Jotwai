import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import type { SuggestionKeyDownProps, SuggestionProps } from "@tiptap/suggestion";
import type { SlashCommandItem } from "../extensions/slashCommand";

export interface SlashCommandMenuHandle {
  onKeyDown: (props: SuggestionKeyDownProps) => boolean;
}

export const SlashCommandMenu = forwardRef<SlashCommandMenuHandle, SuggestionProps<SlashCommandItem>>(
  ({ items, command }, ref) => {
    const [selectedIndex, setSelectedIndex] = useState(0);

    useEffect(() => setSelectedIndex(0), [items]);

    useImperativeHandle(ref, () => ({
      onKeyDown: ({ event }) => {
        if (event.key === "ArrowUp") {
          setSelectedIndex((prev) => (prev + items.length - 1) % items.length);
          return true;
        }
        if (event.key === "ArrowDown") {
          setSelectedIndex((prev) => (prev + 1) % items.length);
          return true;
        }
        if (event.key === "Enter") {
          const item = items[selectedIndex];
          if (item) command(item);
          return true;
        }
        return false;
      },
    }));

    if (items.length === 0) {
      return (
        <div className="w-56 rounded-md border border-border bg-popover px-3 py-2 text-sm text-muted-foreground shadow-md">
          No matches
        </div>
      );
    }

    return (
      <div className="w-56 rounded-md border border-border bg-popover p-1 shadow-md">
        {items.map((item, index) => (
          <button
            key={item.title}
            type="button"
            onClick={() => command(item)}
            className={`block w-full cursor-pointer rounded px-2 py-1.5 text-left text-sm ${
              index === selectedIndex ? "bg-accent text-accent-foreground" : "text-popover-foreground"
            }`}
          >
            {item.title}
          </button>
        ))}
      </div>
    );
  },
);
SlashCommandMenu.displayName = "SlashCommandMenu";
