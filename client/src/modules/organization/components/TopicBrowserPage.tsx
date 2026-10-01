import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Input } from "@/shared/components/ui/input";
import { useTopics } from "../hooks/useTopics";
import { useCreateTopic } from "../hooks/useCreateTopic";
import { buildTopicTree } from "../utils/buildTopicTree";
import { buildNotePreview } from "../utils/buildNotePreview";
import { computeVisibleTopicIds } from "../utils/computeVisibleTopicIds";
import { nextUntitledTopicName } from "../utils/nextUntitledTopicName";
import { useNotes } from "../../notes/hooks/useNotes";
import { useSearch } from "../../search/hooks/useSearch";
import { useActiveTopicSuggestions } from "../../ai-categorization/hooks/useActiveTopicSuggestions";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { ROUTES } from "../../../shared/routes/paths";
import { CategoryFilterChips } from "./CategoryFilterChips";
import { TopicCard } from "./TopicCard";

export function TopicBrowserPage() {
    const navigate = useNavigate();
    const [query, setQuery] = useState("");
    const [selectedCategoryIds, setSelectedCategoryIds] = useState<string[]>([]);
    const debouncedQuery = useDebouncedValue(query, 300);

    const topics = useTopics();
    const notes = useNotes();
    const createTopic = useCreateTopic();
    const isFiltering = debouncedQuery.trim() !== "" || selectedCategoryIds.length > 0;
    const search = useSearch(debouncedQuery, selectedCategoryIds);
    const activeSuggestions = useActiveTopicSuggestions();
    const topicIdsWithSuggestions = useMemo(
        () => new Set(activeSuggestions.data ?? []),
        [activeSuggestions.data],
    );

    // A Topic is one continuous document (ticket 043), so GET /api/notes yields at most one
    // active Note per Topic - it feeds each card's preview and last-edited date. If it fails
    // the cards still render, just without a preview.
    const noteByTopicId = useMemo(() => {
        const map = new Map<string, { content: string; updatedAt: string }>();
        for (const note of notes.data ?? []) {
            if (note.topicId) map.set(note.topicId, note);
        }
        return map;
    }, [notes.data]);

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

    // Most recently touched first - what a returning user most likely wants, and it puts a
    // brand-new Topic right next to the "+ New topic" card.
    const cards = useMemo(
        () =>
            tree
                .map((node) => {
                    const note = noteByTopicId.get(node.id);
                    const updatedAt = note && note.updatedAt > node.updatedAt ? note.updatedAt : node.updatedAt;
                    return { node, note, updatedAt };
                })
                .sort((a, b) => b.updatedAt.localeCompare(a.updatedAt)),
        [tree, noteByTopicId],
    );

    const isListLoading = topics.isLoading || (isFiltering && search.isLoading);
    const hasLoadError = topics.isError || search.isError;
    const hasNoTopicsAtAll = !isListLoading && !hasLoadError && !isFiltering && cards.length === 0;
    const hasNoMatches = !isListLoading && !hasLoadError && isFiltering && cards.length === 0;

    return (
        <main className="mx-auto w-full max-w-5xl space-y-8 px-6 pb-16 pt-10">
            <header className="space-y-1.5">
                <h1 className="text-3xl font-semibold tracking-tight text-foreground">Topics</h1>
                <p className="max-w-lg text-base text-muted-foreground text-pretty">
                    Everything you write lives in a topic. Start a new one and just write &mdash; you can name and
                    organise it later.
                </p>
            </header>

            {createTopic.isError && (
                <p role="alert" className="text-base text-destructive">
                    {getErrorMessage(createTopic.error, "Failed to create topic")}
                </p>
            )}

            {/* Search and filters only earn their place once there is something to find. */}
            {!hasNoTopicsAtAll && (
                <section aria-label="Find a topic" className="max-w-xl space-y-3">
                    <Input
                        type="search"
                        placeholder="Search topics and notes…"
                        aria-label="Search topics and notes"
                        value={query}
                        onChange={(event) => setQuery(event.target.value)}
                    />
                    <CategoryFilterChips selectedCategoryIds={selectedCategoryIds} onToggle={toggleCategory} />
                </section>
            )}

            {hasLoadError && (
                <p role="alert" className="rounded-xl bg-muted p-4 text-base text-destructive">
                    {getErrorMessage(topics.error ?? search.error, "Failed to load topics")}
                </p>
            )}

            {hasNoMatches && (
                <p className="rounded-xl bg-muted px-6 py-10 text-center text-base text-muted-foreground">
                    No matching topics or notes.
                </p>
            )}

            <section aria-label="Topic list" className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {isListLoading &&
                    Array.from({ length: 6 }).map((_, index) => (
                        <div key={index} className="h-44 animate-pulse rounded-xl bg-muted" />
                    ))}

                {!isListLoading && !hasLoadError && !isFiltering && (
                    <button
                        type="button"
                        onClick={handleNewTopic}
                        disabled={createTopic.isPending}
                        className="flex min-h-44 cursor-pointer flex-col items-start justify-center gap-2 rounded-xl border-2 border-dashed border-border p-5 text-left hover:border-primary hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-50"
                    >
                        <span className="text-lg font-semibold text-foreground">
                            {createTopic.isPending ? "Creating…" : "+ New topic"}
                        </span>
                        <span className="text-base text-muted-foreground text-pretty">
                            {hasNoTopicsAtAll
                                ? "Nothing here yet. Press to start writing — no name, no folder. Jotwai will suggest where it belongs afterwards."
                                : "Start writing right away. Name it later."}
                        </span>
                    </button>
                )}

                {!isListLoading &&
                    cards.map(({ node, note, updatedAt }) => (
                        <TopicCard
                            key={node.id}
                            node={node}
                            preview={buildNotePreview(note?.content)}
                            updatedAt={updatedAt}
                            hasSuggestion={topicIdsWithSuggestions.has(node.id)}
                        />
                    ))}
            </section>
        </main>
    );
}
