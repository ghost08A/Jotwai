import type { NoteResponse } from "../types";

export interface NoteTreeRow {
  note: NoteResponse;
  depth: number;
}

// Orders notes parent-first, each parent followed by its children, so a list can render
// the hierarchy with simple indentation. A note whose parent isn't in the list (it can't
// happen for active notes, but don't hide it if it does) is shown as a top-level note.
export function flattenNoteTree(notes: NoteResponse[]): NoteTreeRow[] {
  const knownIds = new Set(notes.map((note) => note.id));
  const childrenByParentId = new Map<string | null, NoteResponse[]>();
  for (const note of notes) {
    const parentId = note.parentId !== null && knownIds.has(note.parentId) ? note.parentId : null;
    childrenByParentId.set(parentId, [...(childrenByParentId.get(parentId) ?? []), note]);
  }

  const rows: NoteTreeRow[] = [];
  function visit(parentId: string | null, depth: number) {
    for (const note of childrenByParentId.get(parentId) ?? []) {
      rows.push({ note, depth });
      visit(note.id, depth + 1);
    }
  }
  visit(null, 0);
  return rows;
}

// The rows a note may be moved under: everything except the note itself and its own
// descendants (rows are parent-first, so the descendants are the deeper rows right after it).
// Note: the backend is the real guard - this only keeps impossible choices out of the list.
export function listMoveTargets(rows: NoteTreeRow[], noteId: string): NoteTreeRow[] {
  const index = rows.findIndex((row) => row.note.id === noteId);
  if (index === -1) return rows;

  let end = index + 1;
  while (end < rows.length && rows[end].depth > rows[index].depth) end++;
  return [...rows.slice(0, index), ...rows.slice(end)];
}
