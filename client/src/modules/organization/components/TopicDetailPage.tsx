import { useEffect, useMemo, useRef, useState } from "react";
import { Link, Navigate, useNavigate, useParams } from "react-router-dom";
import { useDebouncedValue } from "@/shared/hooks/useDebouncedValue";
import { NoteEditor } from "../../notes/components/NoteEditor";
import { useTopicNote } from "../../notes/hooks/useTopicNote";
import { useUpsertTopicNote } from "../../notes/hooks/useUpsertTopicNote";
import { useTopics } from "../hooks/useTopics";
import { useRenameTopic } from "../hooks/useRenameTopic";
import { useDeleteTopic } from "../hooks/useDeleteTopic";
import { buildAncestorChain } from "../utils/buildAncestorChain";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { ROUTES } from "../../../shared/routes/paths";
import { TopicSuggestionPanel } from "../../ai-categorization/components/TopicSuggestionPanel";
import { useTriggerTopicSuggestion } from "../../ai-categorization/hooks/useTriggerTopicSuggestion";
import type { TopicResponse } from "../types";

// `key={topicId}` below forces a full remount on every navigation to a
// different Topic (react-router keeps the same element mounted across param
// changes on the same route) - that's what resets all the local state here
// (title draft, note editor draft) instead of leaking a previous Topic's
// in-progress edits into the next one.
export function TopicDetailPage() {
    const { topicId } = useParams<{ topicId: string }>();
    const topics = useTopics();

    if (!topicId) return <Navigate to={ROUTES.home} replace />;

    if (topics.isLoading) {
        return <div className="w-full px-6 py-6 text-base text-muted-foreground md:px-12 lg:px-24">Loading…</div>;
    }

    const topic = topics.data?.find((candidate) => candidate.id === topicId);
    if (!topic) {
        return (
            <div className="w-full space-y-2 px-6 py-6 md:px-12 lg:px-24">
                <p className="text-base text-destructive">Topic not found.</p>
                <Link to={ROUTES.home} className="text-base text-primary hover:underline">
                    Back to home
                </Link>
            </div>
        );
    }

    return <TopicDetailPageContent key={topicId} topicId={topicId} topic={topic} allTopics={topics.data ?? []} />;
}

interface TopicDetailPageContentProps {
    topicId: string;
    topic: TopicResponse;
    allTopics: TopicResponse[];
}

// `topic` is guaranteed loaded by the wrapper above before this ever mounts,
// so `nameDraft`'s initial value is always correct on first render - and
// `key={topicId}` on the wrapper's call site forces a full remount (instead
// of a prop update) on every navigation to a different Topic, which is what
// resets all the local state here (title draft, note editor draft) instead of
// leaking a previous Topic's in-progress edits.
function TopicDetailPageContent({ topicId, topic, allTopics }: TopicDetailPageContentProps) {
    const navigate = useNavigate();
    const renameTopic = useRenameTopic();
    const deleteTopic = useDeleteTopic();
    const topicNote = useTopicNote(topicId);
    const upsertTopicNote = useUpsertTopicNote(topicId);
    const triggerTopicSuggestion = useTriggerTopicSuggestion();

    const [nameDraft, setNameDraft] = useState(topic.name);

    // Ref, not state - the "Suggest placement" button and the auto-trigger-on-leave below
    // (ticket 044) both need the *current* draft content, but reading it shouldn't
    // re-render this whole page (title input, breadcrumb, panel, ...) on every keystroke;
    // TopicDocumentEditor already isolates that churn to itself.
    const contentRef = useRef("");
    const documentRef = useRef<HTMLElement>(null);
    const hasEditedRef = useRef(false);

    // Fires exactly once, when this Topic's page is left (navigating elsewhere, or to a
    // different Topic - key={topicId} on the wrapper forces a remount either way) - only if
    // the user actually edited something this visit, so just opening and leaving an
    // already-analyzed Topic doesn't waste an AI call re-suggesting the same thing.
    useEffect(() => {
        return () => {
            if (hasEditedRef.current) {
                triggerTopicSuggestion.mutate({ topicId, content: contentRef.current });
            }
        };
    }, [topicId]);

    const ancestorChain = useMemo(() => buildAncestorChain(allTopics, topicId), [allTopics, topicId]);

    function commitRename() {
        const trimmed = nameDraft.trim();
        if (!trimmed) {
            setNameDraft(topic.name);
            return;
        }
        if (trimmed === topic.name) return;
        renameTopic.mutate({ id: topicId, request: { name: trimmed } });
    }

    function handleDelete() {
        deleteTopic.mutate(topicId, {
            onSuccess: () => navigate(topic.parentId ? ROUTES.topicDetail(topic.parentId) : ROUTES.home),
        });
    }

    return (
        <main className="mx-auto w-full max-w-[59rem] space-y-4 px-6 pb-24 pt-8">
            <div className="flex items-center justify-between gap-4">
            <nav className="flex min-w-0 flex-wrap items-center gap-1 text-sm text-muted-foreground">
                <Link to={ROUTES.home} className="hover:text-foreground hover:underline">
                    Topics
                </Link>
                {ancestorChain.map((ancestor, index) => (
                    <span key={ancestor.id} className="flex items-center gap-1">
                        <span>/</span>
                        {index === ancestorChain.length - 1 ? (
                            <span className="text-foreground">{ancestor.name}</span>
                        ) : (
                            <Link to={ROUTES.topicDetail(ancestor.id)} className="hover:text-foreground hover:underline">
                                {ancestor.name}
                            </Link>
                        )}
                    </span>
                ))}
            </nav>

            <div className="flex shrink-0 items-center gap-2">
                <NoteSaveStatus isSaving={upsertTopicNote.isPending} />
                {/* Native <details> as the menu: no dependency, keyboard-toggleable.
                    ponytail: doesn't close on outside click/Escape - swap for a Radix dropdown if that matters. */}
                <details className="group relative">
                    <summary
                        aria-label="Topic actions"
                        className="flex h-8 w-8 cursor-pointer list-none items-center justify-center rounded-md text-lg text-muted-foreground hover:bg-accent hover:text-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring active:scale-95 [&::-webkit-details-marker]:hidden"
                    >
                        ⋯
                    </summary>
                    <div className="absolute right-0 z-20 mt-1 w-40 rounded-md bg-card p-1 shadow-lg ring-1 ring-border">
                        <button
                            type="button"
                            onClick={handleDelete}
                            disabled={deleteTopic.isPending}
                            className="w-full cursor-pointer rounded-sm px-2 py-1.5 text-left text-base text-destructive hover:bg-accent focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            {deleteTopic.isPending ? "Deleting…" : "Delete topic"}
                        </button>
                    </div>
                </details>
            </div>
            </div>

            {/* Notion-style: the title is always editable in place - no separate
                Rename button/mode, just type and blur to save. Enter drops the caret
                into the document below instead of just blurring. */}
            <input
                aria-label="Topic name"
                value={nameDraft}
                onChange={(event) => setNameDraft(event.target.value)}
                onBlur={commitRename}
                onKeyDown={(event) => {
                    if (event.key === "Enter") {
                        event.preventDefault();
                        documentRef.current?.querySelector<HTMLElement>("[contenteditable=true]")?.focus();
                    }
                }}
                className="w-full bg-transparent text-4xl font-bold tracking-tight text-foreground outline-none placeholder:text-muted-foreground focus-visible:outline-none"
                placeholder="Untitled"
            />
            {(renameTopic.isError || deleteTopic.isError) && (
                <p className="text-base text-destructive">
                    {getErrorMessage(renameTopic.error ?? deleteTopic.error, "Failed to update topic")}
                </p>
            )}

            <TopicSuggestionPanel topicId={topicId} getContent={() => contentRef.current} />

            <section ref={documentRef} className="space-y-3">
                {topicNote.isLoading ? (
                    <div className="h-40 animate-pulse rounded-md bg-muted" />
                ) : (
                    <TopicDocumentEditor
                        topicId={topicId}
                        initialContent={topicNote.data?.content ?? ""}
                        onAutosave={(content) => upsertTopicNote.mutate({ content })}
                        onContentChange={(content) => {
                            contentRef.current = content;
                        }}
                        onEdited={() => {
                            hasEditedRef.current = true;
                        }}
                    />
                )}
                {topicNote.isError && (
                    <p className="text-base text-destructive">{getErrorMessage(topicNote.error, "Failed to load note")}</p>
                )}
                {upsertTopicNote.isError && (
                    <p className="text-base text-destructive">{getErrorMessage(upsertTopicNote.error, "Failed to save note")}</p>
                )}
            </section>

            {/* No standalone "Subtopics" section - "/subtopic" inside the editor already
                creates a child Topic and drops a Topic Reference node right where the
                cursor was, so subtopics show up inline as part of the continuous document
                instead of a separate list competing for attention. */}
        </main>
    );
}

function NoteSaveStatus({ isSaving }: { isSaving: boolean }) {
    return <span className="text-sm text-muted-foreground">{isSaving ? "Saving…" : "Saved"}</span>;
}

interface TopicDocumentEditorProps {
    topicId: string;
    initialContent: string;
    onAutosave: (content: string) => void;
    onContentChange: (content: string) => void;
    onEdited: () => void;
}

// The continuous-document composer: no Save button - types straight into the Topic's one
// Note, debounced-autosaved via `onAutosave`. `initialContent` is only read once per mount
// (the parent remounts this whole component via key={topicId} on navigation, so it never
// goes stale for a *different* Topic).
function TopicDocumentEditor({ topicId, initialContent, onAutosave, onContentChange, onEdited }: TopicDocumentEditorProps) {
    const [content, setContent] = useState(initialContent);
    const debouncedContent = useDebouncedValue(content, 600);

    // Keeps the parent's contentRef in sync with every keystroke (not just the debounced
    // value) so a trigger fired right after typing - the "Suggest placement" button, or
    // leaving the page - never reads a stale draft.
    useEffect(() => {
        onContentChange(content);
    }, [content, onContentChange]);

    // Only the debounced value should retrigger this - `initialContent`/`onAutosave` are
    // stable for the lifetime of this mount (remounted per Topic via key={topicId}).
    useEffect(() => {
        if (debouncedContent === initialContent) return;
        onEdited();
        onAutosave(debouncedContent);
    }, [debouncedContent]);

    return <NoteEditor content={content} onChange={setContent} topicId={topicId} />;
}
