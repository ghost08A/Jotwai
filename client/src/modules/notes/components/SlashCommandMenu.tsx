import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react";
import type { SuggestionKeyDownProps, SuggestionProps } from "@tiptap/suggestion";
import type { SlashCommandItem } from "../extensions/slashCommand";

export interface SlashCommandMenuHandle {
  onKeyDown: (props: SuggestionKeyDownProps) => boolean;
}

export const SlashCommandMenu = forwardRef<SlashCommandMenuHandle, SuggestionProps<SlashCommandItem>>(
  ({ items, command }, ref) => {
    const [selectedIndex, setSelectedIndex] = useState(0);

    const selectedRef = useRef<HTMLButtonElement>(null);

    useEffect(() => setSelectedIndex(0), [items]);

    // Keep the arrow-key selection visible now that the list scrolls (max-h below).
    useEffect(() => selectedRef.current?.scrollIntoView({ block: "nearest" }), [selectedIndex]);

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
        <div className="w-56 rounded-md border border-border bg-popover px-3 py-2 text-base text-muted-foreground shadow-md">
          No matches
        </div>
      );
    }

    return (
      <div className="max-h-72 w-56 overflow-y-auto rounded-md border border-border bg-popover p-1 shadow-md">
        {items.map((item, index) => (
          <button
            key={item.title}
            ref={index === selectedIndex ? selectedRef : undefined}
            type="button"
            onClick={() => command(item)}
            className={`block w-full cursor-pointer rounded px-2 py-1.5 text-left text-base ${
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
