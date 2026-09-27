import { NodeViewWrapper } from "@tiptap/react";
import type { NodeViewProps } from "@tiptap/react";
import { useNavigate } from "react-router-dom";
import { ROUTES } from "@/shared/routes/paths";

// Renders a Topic Reference (GLOSSARY.md) as a distinct card, not an inline
// text link - clicking navigates to that Topic's detail page.
export function TopicReferenceView({ node }: NodeViewProps) {
  const navigate = useNavigate();
  const topicId = node.attrs.topicId as string;
  const name = node.attrs.name as string;

  return (
    <NodeViewWrapper data-topic-reference-wrapper>
      <button
        type="button"
        contentEditable={false}
        onClick={() => navigate(ROUTES.topicDetail(topicId))}
        className="my-1 flex w-full cursor-pointer items-center gap-2 rounded-md border border-border bg-card px-3 py-2 text-left text-sm text-foreground hover:bg-accent"
      >
        <span aria-hidden="true">📄</span>
        <span className="font-medium">{name}</span>
      </button>
    </NodeViewWrapper>
  );
}
