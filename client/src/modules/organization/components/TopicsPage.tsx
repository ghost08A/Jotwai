import { useMemo, useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { useTopics } from "../hooks/useTopics";
import { buildTopicTree } from "../utils/buildTopicTree";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { CreateTopicForm } from "./CreateTopicForm";
import { TopicNode } from "./TopicNode";

export function TopicsPage() {
  const topics = useTopics();
  const [isAddingRoot, setIsAddingRoot] = useState(false);
  const tree = useMemo(() => buildTopicTree(topics.data ?? []), [topics.data]);

  return (
    <div className="mx-auto max-w-2xl space-y-4 p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-foreground">Topics</h1>
        {!isAddingRoot && (
          <Button type="button" size="sm" onClick={() => setIsAddingRoot(true)}>
            + New topic
          </Button>
        )}
      </div>

      {isAddingRoot && (
        <div className="rounded-lg border border-border bg-card p-3">
          <CreateTopicForm onDone={() => setIsAddingRoot(false)} />
        </div>
      )}

      <div className="rounded-lg border border-border bg-card p-2">
        {topics.isLoading && (
          <div className="space-y-2 p-2">
            {Array.from({ length: 3 }).map((_, index) => (
              <div key={index} className="h-6 animate-pulse rounded bg-muted" />
            ))}
          </div>
        )}

        {topics.isError && (
          <p className="p-2 text-sm text-destructive">{getErrorMessage(topics.error, "Failed to load topics")}</p>
        )}

        {topics.data?.length === 0 && (
          <p className="p-2 text-center text-sm text-muted-foreground">No topics yet.</p>
        )}

        {tree.map((node) => (
          <TopicNode key={node.id} node={node} depth={0} />
        ))}
      </div>
    </div>
  );
}
