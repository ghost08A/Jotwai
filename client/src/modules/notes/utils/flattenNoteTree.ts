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
