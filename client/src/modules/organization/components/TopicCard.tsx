import { Link } from "react-router-dom";
import type { TopicTreeNode } from "../utils/buildTopicTree";
import { ROUTES } from "../../../shared/routes/paths";

interface TopicCardProps {
    node: TopicTreeNode;
    preview: string;
    // ISO timestamp of the latest edit to this Topic (its document or the Topic itself).
    updatedAt: string;
    hasSuggestion: boolean;
}

const MAX_SUBTOPIC_LINKS = 3;

// Stretched-link card: the title link's ::after covers the whole card so the card is one big
// click target, while the subtopic links sit above it (relative z-10) and stay clickable.
export function TopicCard({ node, preview, updatedAt, hasSuggestion }: TopicCardProps) {
    const shownSubtopics = node.children.slice(0, MAX_SUBTOPIC_LINKS);
    const hiddenSubtopicCount = node.children.length - shownSubtopics.length;

    return (
        <article className="relative flex min-h-44 flex-col gap-3 rounded-xl bg-card p-5 ring-1 ring-border hover:bg-accent focus-within:ring-2 focus-within:ring-ring">
            <div className="flex items-start justify-between gap-2">
                <h2 className="min-w-0 text-lg font-semibold leading-snug tracking-tight text-card-foreground">
                    <Link
                        to={ROUTES.topicDetail(node.id)}
                        className="block truncate rounded-sm focus-visible:outline-none after:absolute after:inset-0 after:content-['']"
                    >
                        {node.name}
                    </Link>
                </h2>
                {hasSuggestion && (
                    <span className="shrink-0 rounded bg-primary px-1.5 py-0.5 text-xs font-medium text-primary-foreground">
                        Suggestion
                    </span>
                )}
            </div>

            <p className="line-clamp-3 flex-1 text-base text-muted-foreground text-pretty">
                {preview || "Empty — open to start writing."}
            </p>

            {shownSubtopics.length > 0 && (
                <div className="flex flex-wrap items-center gap-1.5">
                    {shownSubtopics.map((child) => (
                        <Link
                            key={child.id}
                            to={ROUTES.topicDetail(child.id)}
                            className="relative z-10 max-w-40 truncate rounded-md bg-secondary px-2 py-0.5 text-sm text-secondary-foreground hover:bg-primary hover:text-primary-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring active:scale-95"
                        >
                            {child.name}
                        </Link>
                    ))}
                    {hiddenSubtopicCount > 0 && (
                        <span className="text-sm text-muted-foreground">+{hiddenSubtopicCount} more</span>
                    )}
                </div>
            )}

            <p className="text-sm tabular-nums text-muted-foreground">
                Updated {new Date(updatedAt).toLocaleDateString("sv-SE")}
            </p>
        </article>
    );
}
