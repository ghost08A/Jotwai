import type { TopicResponse } from "../types";

// Root-first chain from the topmost ancestor down to (and including) the given Topic -
// used to render the detail page's breadcrumb.
export function buildAncestorChain(topics: TopicResponse[], topicId: string): TopicResponse[] {
    const byId = new Map(topics.map((topic) => [topic.id, topic]));
    const chain: TopicResponse[] = [];

    let current = byId.get(topicId);
    while (current) {
        chain.unshift(current);
        current = current.parentId ? byId.get(current.parentId) : undefined;
    }

    return chain;
}
