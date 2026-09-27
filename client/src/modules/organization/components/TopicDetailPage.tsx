import { useEffect, useMemo, useRef, useState } from "react";
import { Link, Navigate, useNavigate, useParams } from "react-router-dom";
import { Button } from "@/shared/components/ui/button";
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
        return <div className="w-full px-6 py-6 text-sm text-muted-foreground md:px-12 lg:px-24">Loading…</div>;
    }

    const topic = topics.data?.find((candidate) => candidate.id === topicId);
    if (!topic) {
        return (
            <div className="w-full space-y-2 px-6 py-6 md:px-12 lg:px-24">
                <p className="text-sm text-destructive">Topic not found.</p>
                <Link to={ROUTES.home} className="text-sm text-primary hover:underline">
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
        <div className="w-full space-y-6 px-6 py-6 md:px-12 lg:px-24">
            <nav className="flex flex-wrap items-center gap-1 text-sm text-muted-foreground">
                <Link to={ROUTES.home} className="hover:text-foreground hover:underline">
                    Home
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

            <div className="flex items-center justify-between gap-2">
                {/* Notion-style: the title is always editable in place - no separate
                    Rename button/mode, just type and blur (or Enter) to save. */}
                <input
                    aria-label="Topic name"
                    value={nameDraft}
                    onChange={(event) => setNameDraft(event.target.value)}
                    onBlur={commitRename}
                    onKeyDown={(event) => {
                        if (event.key === "Enter") {
                            event.preventDefault();
                            event.currentTarget.blur();
                        }
                    }}
                    className="w-full bg-transparent text-3xl font-bold text-foreground outline-none focus-visible:outline-none"
                />
                <div className="flex shrink-0 items-center gap-3">
                    <NoteSaveStatus isSaving={upsertTopicNote.isPending} />
                    <Button
                        type="button"
                        size="sm"
                        variant="ghost"
                        className="text-destructive hover:text-destructive"
                        onClick={handleDelete}
                        disabled={deleteTopic.isPending}
                    >
                        {deleteTopic.isPending ? "Deleting…" : "Delete"}
                    </Button>
                </div>
            </div>
            {(renameTopic.isError || deleteTopic.isError) && (
                <p className="text-sm text-destructive">
                    {getErrorMessage(renameTopic.error ?? deleteTopic.error, "Failed to update topic")}
                </p>
            )}

            <TopicSuggestionPanel topicId={topicId} getContent={() => contentRef.current} />

            <section className="space-y-3">
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
                    <p className="text-sm text-destructive">{getErrorMessage(topicNote.error, "Failed to load note")}</p>
                )}
                {upsertTopicNote.isError && (
                    <p className="text-sm text-destructive">{getErrorMessage(upsertTopicNote.error, "Failed to save note")}</p>
                )}
            </section>

            {/* No standalone "Subtopics" section - "/subtopic" inside the editor already
                creates a child Topic and drops a Topic Reference node right where the
                cursor was, so subtopics show up inline as part of the continuous document
                instead of a separate list competing for attention. */}
        </div>
    );
}

function NoteSaveStatus({ isSaving }: { isSaving: boolean }) {
    return <span className="text-xs text-muted-foreground">{isSaving ? "กำลังบันทึก…" : "บันทึกแล้ว"}</span>;
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
