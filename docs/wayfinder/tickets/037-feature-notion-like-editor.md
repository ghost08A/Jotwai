---
id: 37
title: "สร้าง: ขยาย base editor ให้เป็น Notion-like — เปิด block/mark ของ StarterKit ทั้งหมด, slash menu, บังคับ block แรกเป็น heading ระดับ 1"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ผู้ใช้ทดลองใช้ base editor (ตั๋ว 017, จำกัดแค่ plain text/bulleted/numbered list/code block ตาม GLOSSARY.md เดิม) แล้วไม่ชอบ ต้องการ editor ที่รู้สึกเหมือน Notion มากขึ้น: ปลดล็อก block/mark พื้นฐานของ Tiptap StarterKit ทั้งหมด, มี "/" slash menu สำหรับแทรก block, และ block แรกสุดของ Note ต้องใหญ่เป็น heading ระดับ 1 เสมอโดยอัตโนมัติ (ไม่มีกล่องครอบ ไม่มี field แยก) เพื่อให้รู้ทันทีว่าเป็น "หัวเรื่อง" ของ Note

Scope ที่ยืนยันกับผู้ใช้ระหว่าง grill:
- Block/mark: StarterKit ทั้งหมด (heading H1-3, blockquote, bold/italic/strike/inline-code mark, horizontal rule) — ไม่รวม table/image/embed อื่นๆ (ไม่มี Markdown มาตรฐานรองรับตรงๆ, การแนบไฟล์เป็น non-goal อยู่แล้วใน ARCHITECTURE.md)
- Slash menu: เรียบง่าย เลื่อนด้วยลูกศร/เมาส์ กด Enter เลือก — เฉพาะ block-level item เท่านั้น (ไม่รวม inline mark อย่าง bold/italic เข้า menu นี้ ตาม convention ของ Notion จริง ที่ mark ใช้ toolbar หรือ markdown input rule แทน)
- Heading แรก: บังคับเป็น **heading node จริงในระดับ 1** (มี `# ` นำหน้าจริงตอน save เป็น Markdown) ไม่ใช่แค่ styling ด้วย CSS — ต้อง handle Note เก่าที่บรรทัดแรกไม่ใช่ heading ด้วย (แปลงอัตโนมัติตอนโหลด)

## Finding

ยังไม่เริ่มดำเนินการ

## Resolution

Build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง client (2026-09-26)

**Block/mark ทั้งหมดของ StarterKit เปิดใช้งานแล้ว** ([NoteEditor.tsx](../../../client/src/modules/notes/components/NoteEditor.tsx)) — heading (level 1-3), blockquote, bold, italic, strike, horizontalRule, code (inline mark) ทั้งหมดเปิด (`link`/`underline` ยังปิดอยู่ — ไม่ได้อยู่ใน scope ที่ยืนยัน) `tiptap-markdown` serialize/parse เป็น syntax มาตรฐาน (`#`, `##`, `###`, `**`, `*`, `~~`, `` ` ``, `>`, `---`) ตรงตามที่ verify ผ่าน browser จริง

**Block แรกบังคับเป็น heading ระดับ 1**: custom `Document` node override ที่ [titleDocument.ts](../../../client/src/modules/notes/extensions/titleDocument.ts) กำหนด content schema เป็น `heading block*` (บังคับ node แรกต้องเป็น heading เสมอ, ห้ามลบทิ้งจนเหลือ node อื่นเป็นตัวแรก — ProseMirror เองปฏิเสธ transaction ที่จะทำให้ doc ผิด schema โดยอัตโนมัติอยู่แล้ว ไม่ต้องเขียน guard เพิ่ม) ผูกกับ plugin แยก ([titleLevelGuard.ts](../../../client/src/modules/notes/extensions/titleLevelGuard.ts)) ที่ `appendTransaction` บังคับ node แรกให้ `level` เป็น 1 เสมอ แม้ผู้ใช้จะพยายามเปลี่ยนเป็น H2/H3 ผ่าน toolbar/slash-menu ตอน cursor อยู่ใน block แรก

**Backward-compat กับ Note เก่า**: Note ที่มีอยู่แล้วก่อน migration นี้ไม่มีบรรทัดแรกเป็น heading เลย (เช่น "My first capture note — edited") — ทำ [ensureLeadingHeading.ts](../../../client/src/modules/notes/utils/ensureLeadingHeading.ts) เป็น pure function แปลง Markdown string ก่อนป้อนเข้า editor: ถ้าบรรทัดแรกที่ไม่ว่างไม่ใช่ heading (หรือเป็น heading ระดับอื่น) จะ normalize เป็น `# ` เสมอ เพื่อให้ parse เข้า schema ใหม่ได้โดยไม่ error — เป็นการ migrate แบบ transparent ตอนโหลด (บันทึกกลับเป็น heading ระดับ 1 จริงตอน save ครั้งถัดไป) **Known edge case**: ถ้าบรรทัดแรกเดิมเป็น list item/code block (ไม่ใช่ plain text) การ prefix `# ` จะทำให้เนื้อหาดูแปลกเล็กน้อย (เช่น `# - foo`) ยอมรับ trade-off นี้เพราะเป็นข้อมูลช่วง dev เท่านั้น ไม่ใช่ production data

**Slash menu**: extension ใหม่ที่ [slashCommand.ts](../../../client/src/modules/notes/extensions/slashCommand.ts) + [SlashCommandMenu.tsx](../../../client/src/modules/notes/components/SlashCommandMenu.tsx) ใช้ `@tiptap/suggestion` (official utility ของ tiptap เอง, เพิ่มเป็น dependency ใหม่) trigger ด้วย `/`, popup คำนวณตำแหน่งเองจาก cursor coordinate (ไม่ใช้ tippy.js หรือ positioning library เพิ่ม) รายการ: Text, Heading 1/2/3, Bulleted list, Numbered list, Quote, Code block, Divider เลือกด้วยลูกศร/เมาส์/Enter

**Borderless "blank page" look**: เอา `border`/`rounded-md` ออกจาก editor container ([NoteEditor.tsx](../../../client/src/modules/notes/components/NoteEditor.tsx)) เพิ่ม CSS ให้ `h1`/`h2`/`h3`/`blockquote`/`hr` มีขนาด/นำ้หนักต่างกันชัดเจนตาม hierarchy — ขยายต่อ (2026-09-26): เอากล่อง `border`/`bg-card`/`shadow` ออกจาก composer และ [NoteCard.tsx](../../../client/src/modules/notes/components/NoteCard.tsx)/[NoteCardSkeleton.tsx](../../../client/src/modules/notes/components/NoteCardSkeleton.tsx) ทั้งหมด, เอา `max-w-2xl` ที่จำกัดความกว้างออกจาก [NotesPage.tsx](../../../client/src/modules/notes/components/NotesPage.tsx) (เปลี่ยนเป็น `w-full` เต็มความกว้างจอ), ใช้ `divide-y` คั่นระหว่าง Note แต่ละอันแทนกล่องครอบ — ยังไม่แตะ `TrashPage.tsx` เพราะไม่ใช่ "พื้นที่จด" ตามที่ผู้ใช้ระบุ

**Toolbar**: เพิ่มปุ่ม Heading (H2/H3 — H1 สงวนไว้เฉพาะ title, ไม่ให้เลือกจาก toolbar เพราะ block แรกถูก guard บังคับเป็น H1 อยู่แล้วเสมอ), Bold, Italic, Strikethrough, Inline code, Quote, Divider ใน [NoteEditorToolbar.tsx](../../../client/src/modules/notes/components/NoteEditorToolbar.tsx) ต่อจากปุ่มเดิม (Bulleted/Numbered list, Code block)

**Bug พบระหว่าง verify**: StarterKit v3 มี `TrailingNode` extension ติดมาด้วย (คอยแทรก node ว่างท้าย document เสมอ เพื่อให้ผู้ใช้คลิกพื้นที่ว่างท้ายสุดแล้วพิมพ์ต่อได้) มัน auto-detect node type ที่จะแทรกจาก `schema.topNodeType.contentMatch.defaultType` — สำหรับ schema ใหม่ `heading block*` ค่านี้ resolve เป็น `heading` (type ที่ schema บังคับต้องมาก่อน) แทนที่จะเป็น `paragraph` ตามปกติ ทำให้มี `<h1>` ว่างๆ โผล่ท้าย document ทุกครั้งที่ document เปลี่ยน (ยืนยันจริงจากการ save แล้วเห็น `# ` โผล่แปลกๆ ในเนื้อหา) แก้โดยกำหนด `trailingNode: { node: "paragraph" }` ตรงๆ ใน `StarterKit.configure(...)` ไม่ให้พึ่ง auto-detect

verify ผ่าน browser จริงกับ backend จริง: พิมพ์ content ใหม่ (บรรทัดแรกกลายเป็น H1 อัตโนมัติ, ไม่มี node แปลกปลอมท้าย document), เปิด Note เก่าที่ไม่มี heading (auto-migrate ถูกต้อง), ใช้ slash menu แทรกทุก block type ด้วยลูกศร+Enter, ลองเปลี่ยน block แรกเป็น H2 ผ่าน toolbar (ถูก guard บังคับกลับเป็น H1), ทดสอบ mark ทุกตัว (bold/italic/strike/inline-code) ผ่าน toolbar, แทรก horizontal rule, ตรวจ markdown output ตรงตาม syntax มาตรฐานทุกตัว (`#`, `` ` ``, `---`), reload แล้ว content ยัง persist ถูกต้อง, ยืนยัน Search read model ยัง sync เนื้อหาถูกต้องเหมือนเดิม (ไม่กระทบ)

**ไม่ได้ทำใน scope นี้ (ตั้งใจ)**: table/image/embed block (ตาม non-goal ของ ARCHITECTURE.md), placeholder text ("Untitled"/"Type '/' for commands") ตอน editor ว่าง — ไม่ได้ถูกขอ ไม่เพิ่มเพื่อเลี่ยง over-engineering, unit test สำหรับ `ensureLeadingHeading.ts` — client ยังไม่มี test runner ตั้งค่าไว้ (`client/CLAUDE.md` "Test Runner" ระบุว่ายังไม่ได้ตั้งค่า Vitest) เป็น debt ที่เปิดไว้ตาม convention เดียวกับ section "Test Runner" ของ client/CLAUDE.md เอง ไม่ใช่ scope ของตั๋วนี้
