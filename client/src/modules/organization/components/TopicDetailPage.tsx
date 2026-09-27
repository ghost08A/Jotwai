import { useMemo, useState } from "react";
import { Link, Navigate, useNavigate, useParams } from "react-router-dom";
import { Button } from "@/shared/components/ui/button";
import { NoteEditor } from "../../notes/components/NoteEditor";
import { NoteCard } from "../../notes/components/NoteCard";
import { NoteCardSkeleton } from "../../notes/components/NoteCardSkeleton";
import { useNotes } from "../../notes/hooks/useNotes";
import { useCreateNote } from "../../notes/hooks/useCreateNote";
import { useAssignNoteTopic } from "../../notes/hooks/useAssignNoteTopic";
import { useTopics } from "../hooks/useTopics";
import { useRenameTopic } from "../hooks/useRenameTopic";
import { useDeleteTopic } from "../hooks/useDeleteTopic";
import { buildAncestorChain } from "../utils/buildAncestorChain";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { ROUTES } from "../../../shared/routes/paths";
import { CreateTopicForm } from "./CreateTopicForm";
import type { TopicResponse } from "../types";

// `key={topicId}` below forces a full remount on every navigation to a
// different Topic (react-router keeps the same element mounted across param
// changes on the same route) - that's what resets all the local state here
// (title draft, note composer draft, subtopic form) instead of leaking a
// previous Topic's in-progress edits into the next one.
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
// resets all the local state here (title draft, note composer draft,
// subtopic form) instead of leaking a previous Topic's in-progress edits.
function TopicDetailPageContent({ topicId, topic, allTopics }: TopicDetailPageContentProps) {
    const navigate = useNavigate();
    const notes = useNotes();
    const renameTopic = useRenameTopic();
    const deleteTopic = useDeleteTopic();
    const createNote = useCreateNote();
    const assignTopic = useAssignNoteTopic();

    const [nameDraft, setNameDraft] = useState(topic.name);
    const [draft, setDraft] = useState("");
    const [isAddingChild, setIsAddingChild] = useState(false);

    const ancestorChain = useMemo(() => buildAncestorChain(allTopics, topicId), [allTopics, topicId]);
    const directChildren = useMemo(
        () => allTopics.filter((candidate) => candidate.parentId === topicId).sort((a, b) => a.name.localeCompare(b.name)),
        [allTopics, topicId],
    );
    const directNotes = (notes.data ?? []).filter((note) => note.topicId === topicId);

    function commitRename(currentTopic: TopicResponse) {
        const trimmed = nameDraft.trim();
        if (!trimmed) {
            setNameDraft(currentTopic.name);
            return;
        }
        if (trimmed === currentTopic.name) return;
        renameTopic.mutate({ id: topicId, request: { name: trimmed } });
    }

    function handleDelete() {
        deleteTopic.mutate(topicId, {
            onSuccess: () => navigate(topic.parentId ? ROUTES.topicDetail(topic.parentId) : ROUTES.home),
        });
    }

    function handleCapture() {
        if (!draft.trim()) return;
        createNote.mutate(
            { content: draft },
            { onSuccess: (note) => assignTopic.mutate({ id: note.id, topicId }) },
        );
        setDraft("");
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
                    onBlur={() => commitRename(topic)}
                    onKeyDown={(event) => {
                        if (event.key === "Enter") {
                            event.preventDefault();
                            event.currentTarget.blur();
                        }
                    }}
                    className="w-full bg-transparent text-3xl font-bold text-foreground outline-none focus-visible:outline-none"
                />
                <Button
                    type="button"
                    size="sm"
                    variant="ghost"
                    className="shrink-0 text-destructive hover:text-destructive"
                    onClick={handleDelete}
                    disabled={deleteTopic.isPending}
                >
                    {deleteTopic.isPending ? "Deleting…" : "Delete"}
                </Button>
            </div>
            {(renameTopic.isError || deleteTopic.isError) && (
                <p className="text-sm text-destructive">
                    {getErrorMessage(renameTopic.error ?? deleteTopic.error, "Failed to update topic")}
                </p>
            )}

            <section className="space-y-3">
                <NoteEditor content={draft} onChange={setDraft} />
                {createNote.isError && (
                    <p className="text-sm text-destructive">{getErrorMessage(createNote.error, "Failed to save note")}</p>
                )}
                <div className="flex justify-end">
                    <Button type="button" onClick={handleCapture} disabled={createNote.isPending || !draft.trim()}>
                        {createNote.isPending ? "Saving…" : "Save note"}
                    </Button>
                </div>

                <div className="divide-y divide-border">
                    {notes.isLoading && Array.from({ length: 2 }).map((_, index) => <NoteCardSkeleton key={index} />)}
                    {notes.isError && (
                        <p className="text-sm text-destructive">{getErrorMessage(notes.error, "Failed to load notes")}</p>
                    )}
                    {!notes.isLoading && directNotes.length === 0 && (
                        <p className="py-2 text-sm text-muted-foreground">No notes in this topic yet.</p>
                    )}
                    {directNotes.map((note) => (
                        <NoteCard key={note.id} note={note} />
                    ))}
                </div>
            </section>

            <section className="space-y-3">
                <div className="flex items-center justify-between">
                    <h2 className="text-sm font-medium text-muted-foreground">Subtopics</h2>
                    {!isAddingChild && (
                        <Button type="button" size="sm" onClick={() => setIsAddingChild(true)}>
                            + New subtopic
                        </Button>
                    )}
                </div>

                {isAddingChild && (
                    <div className="rounded-lg border border-border bg-card p-3">
                        <CreateTopicForm parentId={topicId} onDone={() => setIsAddingChild(false)} />
                    </div>
                )}

                {directChildren.length === 0 ? (
                    <p className="text-sm text-muted-foreground">No subtopics yet.</p>
                ) : (
                    <div className="rounded-lg border border-border bg-card p-2">
                        {directChildren.map((child) => (
                            <Link
                                key={child.id}
                                to={ROUTES.topicDetail(child.id)}
                                className="block rounded-md px-2 py-1.5 text-sm text-foreground hover:bg-accent hover:underline"
                            >
                                {child.name}
                            </Link>
                        ))}
                    </div>
                )}
            </section>
        </div>
    );
}
