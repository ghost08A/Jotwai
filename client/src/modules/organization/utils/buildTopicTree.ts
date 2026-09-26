import type { TopicResponse } from "../types";

export interface TopicTreeNode extends TopicResponse {
  children: TopicTreeNode[];
}

export function buildTopicTree(topics: TopicResponse[]): TopicTreeNode[] {
  const nodesById = new Map<string, TopicTreeNode>();
  for (const topic of topics) {
    nodesById.set(topic.id, { ...topic, children: [] });
  }

  const roots: TopicTreeNode[] = [];
  for (const topic of topics) {
    const node = nodesById.get(topic.id);
    if (!node) continue;

    const parent = topic.parentId ? nodesById.get(topic.parentId) : undefined;
    if (parent) {
      parent.children.push(node);
    } else {
      roots.push(node);
    }
  }

  sortByName(roots);
  return roots;
}

function sortByName(nodes: TopicTreeNode[]): void {
  nodes.sort((a, b) => a.name.localeCompare(b.name));
  for (const node of nodes) sortByName(node.children);
}
