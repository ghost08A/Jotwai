import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Input } from "@/shared/components/ui/input";
import { Button } from "@/shared/components/ui/button";
import { useTopics } from "../hooks/useTopics";
import { useCreateTopic } from "../hooks/useCreateTopic";
import { buildTopicTree } from "../utils/buildTopicTree";
import { computeVisibleTopicIds } from "../utils/computeVisibleTopicIds";
import { nextUntitledTopicName } from "../utils/nextUntitledTopicName";
import { useSearch } from "../../search/hooks/useSearch";
import { useActiveTopicSuggestions } from "../../ai-categorization/hooks/useActiveTopicSuggestions";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { ROUTES } from "../../../shared/routes/paths";
import { CategoryFilterChips } from "./CategoryFilterChips";
import { TopicBrowserNode } from "./TopicBrowserNode";

export function TopicBrowserPage() {
    const navigate = useNavigate();
    const [query, setQuery] = useState("");
    const [selectedCategoryIds, setSelectedCategoryIds] = useState<string[]>([]);
    const debouncedQuery = useDebouncedValue(query, 300);

    const topics = useTopics();
    const createTopic = useCreateTopic();
    const isFiltering = debouncedQuery.trim() !== "" || selectedCategoryIds.length > 0;
    const search = useSearch(debouncedQuery, selectedCategoryIds);
    const activeSuggestions = useActiveTopicSuggestions();
    const topicIdsWithSuggestions = useMemo(
        () => new Set(activeSuggestions.data ?? []),
        [activeSuggestions.data],
    );

    // "Capture first, organize later" (root CLAUDE.md) applied to Topic creation itself
    // (ticket 044): no name/parent prompt upfront - create a placeholder-named root Topic
    // immediately and jump straight into its continuous-document writing page. Topic
    // Suggestion (name + content, see TopicSuggestionPanel) decides afterward whether it
    // should merge into or move under an existing Topic.
    function handleNewTopic() {
        const rootNames = (topics.data ?? []).filter((topic) => !topic.parentId).map((topic) => topic.name);
        createTopic.mutate(
            { name: nextUntitledTopicName(rootNames) },
            { onSuccess: (topic) => navigate(ROUTES.topicDetail(topic.id)) },
        );
    }

    function toggleCategory(categoryId: string) {
        setSelectedCategoryIds((current) =>
            current.includes(categoryId) ? current.filter((id) => id !== categoryId) : [...current, categoryId],
        );
    }

    const tree = useMemo(() => {
        if (!topics.data) return [];
        if (!isFiltering) return buildTopicTree(topics.data);

        const matchedIds = new Set<string>();
        for (const result of search.data ?? []) {
            if (result.topicId) matchedIds.add(result.topicId);
        }

        // A Topic's own name only counts as a match on its own (with no Category
        // selected) - once a Category filter is active, a match must come from an
        // actual tagged Note, since an empty Topic can never satisfy it.
        if (selectedCategoryIds.length === 0) {
            const lowerQuery = debouncedQuery.trim().toLowerCase();
            for (const topic of topics.data) {
                if (topic.name.toLowerCase().includes(lowerQuery)) matchedIds.add(topic.id);
            }
        }

        const visibleIds = computeVisibleTopicIds(topics.data, matchedIds);
        return buildTopicTree(topics.data.filter((topic) => visibleIds.has(topic.id)));
    }, [topics.data, isFiltering, search.data, selectedCategoryIds, debouncedQuery]);

    return (
        <div className="w-full space-y-4 px-6 py-6 md:px-12 lg:px-24">
            <h1 className="text-xl font-semibold text-foreground">Jotwai</h1>

            <Input
                type="search"
                placeholder="Search topics and notes…"
                value={query}
                onChange={(event) => setQuery(event.target.value)}
            />

            <CategoryFilterChips selectedCategoryIds={selectedCategoryIds} onToggle={toggleCategory} />

            <div className="flex items-center justify-between">
                <h2 className="text-base font-medium text-muted-foreground">Topics</h2>
                <Button type="button" size="sm" onClick={handleNewTopic} disabled={createTopic.isPending}>
                    {createTopic.isPending ? "Creating…" : "+ New topic"}
                </Button>
            </div>

            {createTopic.isError && (
                <p className="text-base text-destructive">{getErrorMessage(createTopic.error, "Failed to create topic")}</p>
            )}

            <div className="rounded-lg border border-border bg-card p-2">
                {(topics.isLoading || (isFiltering && search.isLoading)) && (
                    <div className="space-y-2 p-2">
                        {Array.from({ length: 3 }).map((_, index) => (
                            <div key={index} className="h-6 animate-pulse rounded bg-muted" />
                        ))}
                    </div>
                )}

                {(topics.isError || search.isError) && (
                    <p className="p-2 text-base text-destructive">
                        {getErrorMessage(topics.error ?? search.error, "Failed to load topics")}
                    </p>
                )}

                {!topics.isLoading && !search.isLoading && tree.length === 0 && (
                    <p className="p-2 text-center text-base text-muted-foreground">
                        {isFiltering ? "No matching topics or notes." : "No topics yet."}
                    </p>
                )}

                {tree.map((node) => (
                    <TopicBrowserNode
                        key={node.id}
                        node={node}
                        depth={0}
                        isFiltering={isFiltering}
                        topicIdsWithSuggestions={topicIdsWithSuggestions}
                    />
                ))}
            </div>
        </div>
    );
}
