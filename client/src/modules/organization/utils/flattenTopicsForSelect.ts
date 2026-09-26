import { buildTopicTree } from "./buildTopicTree";
import type { TopicTreeNode } from "./buildTopicTree";
import type { TopicResponse } from "../types";

export interface TopicOption {
  id: string;
  label: string;
}

// Depth-first flattening of the hierarchy into a flat list suitable for a
// <select> - each descendant's label is indented so the hierarchy still
// reads clearly without a tree widget.
export function flattenTopicsForSelect(topics: TopicResponse[]): TopicOption[] {
  const options: TopicOption[] = [];
  visit(buildTopicTree(topics), 0, options);
  return options;
}

function visit(nodes: TopicTreeNode[], depth: number, options: TopicOption[]): void {
  for (const node of nodes) {
    options.push({ id: node.id, label: `${"— ".repeat(depth)}${node.name}` });
    visit(node.children, depth + 1, options);
  }
}
