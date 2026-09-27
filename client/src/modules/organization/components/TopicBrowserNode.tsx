import { useState } from "react";
import { Link } from "react-router-dom";
import type { TopicTreeNode } from "../utils/buildTopicTree";
import { ROUTES } from "../../../shared/routes/paths";

interface TopicBrowserNodeProps {
    node: TopicTreeNode;
    depth: number;
    // true while a search/Category filter is active - the tree passed in has
    // already been pruned to matches and their ancestors, so every remaining
    // node should render fully expanded instead of respecting local collapse state.
    isFiltering: boolean;
}

const INDENT_CLASSES = ["pl-2", "pl-6", "pl-10", "pl-14", "pl-20", "pl-24", "pl-28", "pl-32"] as const;

function indentClassName(depth: number): string {
    return INDENT_CLASSES[Math.min(depth, INDENT_CLASSES.length - 1)];
}

export function TopicBrowserNode({ node, depth, isFiltering }: TopicBrowserNodeProps) {
    const [isExpanded, setIsExpanded] = useState(false);
    const hasChildren = node.children.length > 0;
    const expanded = isFiltering || isExpanded;

    return (
        <div>
            <div className={`flex items-center gap-1 rounded-md py-1.5 hover:bg-accent ${indentClassName(depth)}`}>
                {hasChildren ? (
                    <button
                        type="button"
                        aria-label={expanded ? "Collapse" : "Expand"}
                        aria-expanded={expanded}
                        disabled={isFiltering}
                        onClick={() => setIsExpanded((value) => !value)}
                        className="flex h-5 w-5 shrink-0 cursor-pointer items-center justify-center text-muted-foreground hover:text-foreground disabled:cursor-default"
                    >
                        {expanded ? "▾" : "▸"}
                    </button>
                ) : (
                    <span className="h-5 w-5 shrink-0" />
                )}

                <Link to={ROUTES.topicDetail(node.id)} className="flex-1 text-sm text-foreground hover:underline">
                    {node.name}
                </Link>
            </div>

            {hasChildren &&
                expanded &&
                node.children.map((child) => (
                    <TopicBrowserNode key={child.id} node={child} depth={depth + 1} isFiltering={isFiltering} />
                ))}
        </div>
    );
}
