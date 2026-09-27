import type { TopicResponse } from "../types";

// Filtering the home topic browser (search text and/or Category selection) must reveal
// the whole path down to a match, not just the matching Topic itself - otherwise a match
// nested several levels deep would be unreachable behind collapsed ancestors that don't
// themselves match anything.
export function computeVisibleTopicIds(topics: TopicResponse[], matchedIds: ReadonlySet<string>): Set<string> {
    const parentById = new Map(topics.map((topic) => [topic.id, topic.parentId]));
    const visible = new Set<string>(matchedIds);

    for (const id of matchedIds) {
        let parentId = parentById.get(id) ?? null;
        while (parentId !== null && !visible.has(parentId)) {
            visible.add(parentId);
            parentId = parentById.get(parentId) ?? null;
        }
    }

    return visible;
}
