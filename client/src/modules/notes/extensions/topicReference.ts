import { Node, mergeAttributes } from "@tiptap/core";
import { ReactNodeViewRenderer } from "@tiptap/react";
import { TopicReferenceView } from "../components/TopicReferenceView";

export interface TopicReferenceAttrs {
  topicId: string;
  name: string;
}

// Block card referencing another Topic (CONTEXT.md "Topic Reference", ticket
// 042) - inserted via the "/subtopic" slash command. Stored in Markdown as a
// plain CommonMark link with a non-http scheme (`topic-ref:<id>`) so it
// round-trips through tiptap-markdown without a custom markdown-it rule; the
// `parse.updateDOM` hook below rewrites that rendered `<a>` back into this
// node's own HTML shape before the schema parses it.
export const TopicReference = Node.create({
  name: "topicReference",
  group: "block",
  atom: true,
  selectable: true,
  draggable: false,

  addAttributes() {
    return {
      topicId: { default: null },
      name: { default: "" },
    };
  },

  parseHTML() {
    return [
      {
        tag: "div[data-topic-reference]",
        getAttrs: (element) => ({
          topicId: (element as HTMLElement).getAttribute("data-topic-id"),
          name: (element as HTMLElement).getAttribute("data-topic-name"),
        }),
      },
    ];
  },

  renderHTML({ node, HTMLAttributes }) {
    return [
      "div",
      mergeAttributes(HTMLAttributes, {
        "data-topic-reference": "",
        "data-topic-id": node.attrs.topicId,
        "data-topic-name": node.attrs.name,
      }),
    ];
  },

  addNodeView() {
    return ReactNodeViewRenderer(TopicReferenceView);
  },

  addStorage() {
    return {
      markdown: {
        serialize(state: { write: (text: string) => void; closeBlock: (node: unknown) => void }, node: { attrs: TopicReferenceAttrs }) {
          state.write(`[${node.attrs.name}](topic-ref:${node.attrs.topicId})`);
          state.closeBlock(node);
        },
        parse: {
          updateDOM(element: HTMLElement) {
            element.querySelectorAll('a[href^="topic-ref:"]').forEach((anchor) => {
              const href = anchor.getAttribute("href") ?? "";
              const topicId = href.slice("topic-ref:".length);
              const name = anchor.textContent ?? "";
              const div = element.ownerDocument.createElement("div");
              div.setAttribute("data-topic-reference", "");
              div.setAttribute("data-topic-id", topicId);
              div.setAttribute("data-topic-name", name);
              anchor.replaceWith(div);
            });
          },
        },
      },
    };
  },
});
