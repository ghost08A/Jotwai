import { NodeViewWrapper } from "@tiptap/react";
import type { NodeViewProps } from "@tiptap/react";
import { useNavigate } from "react-router-dom";
import { ROUTES } from "@/shared/routes/paths";

// Renders a Topic Reference (CONTEXT.md) as a distinct card, not an inline
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
        className="my-0.5 flex w-full cursor-pointer items-center gap-2 rounded-md px-1.5 py-1 text-left text-base text-foreground hover:bg-accent focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring active:scale-[0.99]"
      >
        <span aria-hidden="true">📄</span>
        <span className="underline decoration-muted-foreground/40 underline-offset-4">{name}</span>
      </button>
    </NodeViewWrapper>
  );
}
