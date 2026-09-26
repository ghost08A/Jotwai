import { Extension } from "@tiptap/core";
import { Plugin } from "@tiptap/pm/state";

// Keeps the first node's heading level pinned to 1 - selecting Heading 2/3
// via toolbar/slash-menu while the cursor sits in the title block would
// otherwise downgrade the title itself (ticket 037: block แรกเป็น heading
// ระดับ 1 เสมอ).
export const TitleLevelGuard = Extension.create({
  name: "titleLevelGuard",

  addProseMirrorPlugins() {
    return [
      new Plugin({
        appendTransaction(transactions, _oldState, newState) {
          if (!transactions.some((transaction) => transaction.docChanged)) return null;

          const firstNode = newState.doc.firstChild;
          if (!firstNode || firstNode.type.name !== "heading" || firstNode.attrs.level === 1) {
            return null;
          }

          return newState.tr.setNodeMarkup(0, undefined, { ...firstNode.attrs, level: 1 });
        },
      }),
    ];
  },
});
