import { useMemo, useState } from "react";
import { Input } from "@/shared/components/ui/input";
import { useTopics } from "../../organization/hooks/useTopics";
import { useCategories } from "../../organization/hooks/useCategories";
import { flattenTopicsForSelect } from "../../organization/utils/flattenTopicsForSelect";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { useSearch } from "../hooks/useSearch";
import { SearchResultCard } from "./SearchResultCard";
import { SearchResultCardSkeleton } from "./SearchResultCardSkeleton";

export function SearchPage() {
    const [query, setQuery] = useState("");
    const [topicId, setTopicId] = useState("");
    const debouncedQuery = useDebouncedValue(query, 300);

    const topics = useTopics();
    const categories = useCategories();
    const topicOptions = useMemo(() => flattenTopicsForSelect(topics.data ?? []), [topics.data]);

    const results = useSearch(debouncedQuery, topicId);

    function topicNameOf(id: string | null): string | undefined {
        return topics.data?.find((topic) => topic.id === id)?.name;
    }

    function categoryNamesOf(categoryIds: string[]): string[] {
        return (categories.data ?? [])
            .filter((category) => categoryIds.includes(category.id))
            .map((category) => category.name);
    }

    return (
        <div className="mx-auto max-w-2xl space-y-4 p-6">
            <h1 className="text-xl font-semibold text-foreground">Search</h1>

            <div className="flex flex-col gap-2 sm:flex-row">
                <Input
                    type="search"
                    placeholder="Search notes and categories…"
                    value={query}
                    onChange={(event) => setQuery(event.target.value)}
                    className="sm:flex-1"
                />

                <select
                    aria-label="Filter by topic"
                    value={topicId}
                    onChange={(event) => setTopicId(event.target.value)}
                    className="cursor-pointer rounded-md border border-input bg-transparent px-3 py-1 text-sm text-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                >
                    <option value="">All topics</option>
                    {topicOptions.map((option) => (
                        <option key={option.id} value={option.id}>
                            {option.label}
                        </option>
                    ))}
                </select>
            </div>

            <div className="divide-y divide-border rounded-lg border border-border bg-card px-4">
                {results.isLoading &&
                    Array.from({ length: 3 }).map((_, index) => <SearchResultCardSkeleton key={index} />)}

                {results.isError && (
                    <p className="py-4 text-sm text-destructive">
                        {getErrorMessage(results.error, "Failed to search notes")}
                    </p>
                )}

                {results.data?.length === 0 && (
                    <p className="py-4 text-center text-sm text-muted-foreground">No matching notes.</p>
                )}

                {results.data?.map((result) => (
                    <SearchResultCard
                        key={result.id}
                        result={result}
                        topicName={topicNameOf(result.topicId)}
                        categoryNames={categoryNamesOf(result.categoryIds)}
                    />
                ))}
            </div>
        </div>
    );
}
