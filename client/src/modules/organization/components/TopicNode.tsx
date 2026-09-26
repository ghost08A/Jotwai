import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useRenameTopic } from "../hooks/useRenameTopic";
import { useDeleteTopic } from "../hooks/useDeleteTopic";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { CreateTopicForm } from "./CreateTopicForm";
import type { TopicTreeNode } from "../utils/buildTopicTree";

interface TopicNodeProps {
  node: TopicTreeNode;
  depth: number;
}

// Fixed Tailwind steps rather than an inline style (client/CLAUDE.md forbids
// raw inline style/CSS) - indentation visually saturates past this depth,
// but Topic nesting itself stays unlimited (GLOSSARY.md "Topic").
const INDENT_CLASSES = ["pl-2", "pl-6", "pl-10", "pl-14", "pl-20", "pl-24", "pl-28", "pl-32"] as const;

function indentClassName(depth: number): string {
  return INDENT_CLASSES[Math.min(depth, INDENT_CLASSES.length - 1)];
}

export function TopicNode({ node, depth }: TopicNodeProps) {
  const [isRenaming, setIsRenaming] = useState(false);
  const [isAddingChild, setIsAddingChild] = useState(false);
  const [nameDraft, setNameDraft] = useState(node.name);
  const renameTopic = useRenameTopic();
  const deleteTopic = useDeleteTopic();

  function startRenaming() {
    setNameDraft(node.name);
    setIsRenaming(true);
  }

  function handleRename() {
    if (!nameDraft.trim()) return;
    renameTopic.mutate(
      { id: node.id, request: { name: nameDraft.trim() } },
      { onSuccess: () => setIsRenaming(false) },
    );
  }

  return (
    <div>
      <div className={`flex flex-wrap items-center gap-2 rounded-md py-1.5 hover:bg-accent ${indentClassName(depth)}`}>
        {isRenaming ? (
          <>
            <Input
              value={nameDraft}
              onChange={(event) => setNameDraft(event.target.value)}
              className="h-8 max-w-xs"
              autoFocus
            />
            <Button size="sm" onClick={handleRename} disabled={renameTopic.isPending}>
              {renameTopic.isPending ? "Saving…" : "Save"}
            </Button>
            <Button size="sm" variant="outline" onClick={() => setIsRenaming(false)}>
              Cancel
            </Button>
          </>
        ) : (
          <>
            <span className="flex-1 text-sm text-foreground">{node.name}</span>
            <Button type="button" size="sm" variant="ghost" onClick={() => setIsAddingChild((value) => !value)}>
              + Subtopic
            </Button>
            <Button type="button" size="sm" variant="ghost" onClick={startRenaming}>
              Rename
            </Button>
            <Button
              type="button"
              size="sm"
              variant="ghost"
              className="text-destructive hover:text-destructive"
              onClick={() => deleteTopic.mutate(node.id)}
              disabled={deleteTopic.isPending}
            >
              {deleteTopic.isPending ? "Deleting…" : "Delete"}
            </Button>
          </>
        )}
      </div>

      {renameTopic.isError && (
        <p className={`text-xs text-destructive ${indentClassName(depth)}`}>
          {getErrorMessage(renameTopic.error, "Failed to rename topic")}
        </p>
      )}
      {deleteTopic.isError && (
        <p className={`text-xs text-destructive ${indentClassName(depth)}`}>
          {getErrorMessage(deleteTopic.error, "Failed to delete topic")}
        </p>
      )}

      {isAddingChild && (
        <div className={`py-1 ${indentClassName(depth + 1)}`}>
          <CreateTopicForm parentId={node.id} onDone={() => setIsAddingChild(false)} />
        </div>
      )}

      {node.children.map((child) => (
        <TopicNode key={child.id} node={child} depth={depth + 1} />
      ))}
    </div>
  );
}
