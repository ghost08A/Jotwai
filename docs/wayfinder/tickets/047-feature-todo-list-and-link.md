---
id: 47
title: "เพิ่ม To-do list (checkbox) และ Link ใน editor"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

แยกมาจาก [ticket 046](046-feature-topic-page-notion-layout.md) (2026-09-30) ผู้ใช้อนุญาตให้เพิ่ม dependency และเปิด Link แล้ว:

- To-do list: `@tiptap/extension-task-list` + `task-item` (Markdown `- [ ]` ต้อง round-trip ผ่าน `tiptap-markdown` ให้ได้), เพิ่มรายการ "To-do" ใน slash menu
- Link: ตอนนี้ปิดไว้ใน `NoteEditor.tsx` (`link: false`) — เปิด, ต้องตัดสินใจ UX การใส่ URL (เช่น ปุ่มใน bubble menu / วางลิงก์ทับข้อความ) และความสัมพันธ์กับ Topic Reference node (ticket 042) ที่ตั้งใจแยกจาก link mark
- อัปเดต GLOSSARY.md "Note" ที่ระบุ content type ที่รองรับ

## Resolution

Build เสร็จและ verify ผ่าน browser จริงกับ backend จริง (2026-10-01) ฝั่ง client:

- เพิ่ม dependency `@tiptap/extension-list` (pin `3.31.3` ตรงๆ — `^` ดึง 3.31.4 ชนกับ peer ของ extension อื่นที่ล็อก 3.31.3) เพื่อใช้ `TaskList`/`TaskItem`; `tiptap-markdown` มี serializer ของ task list ในตัวอยู่แล้ว
- `NoteEditor.tsx`: เปิด `TaskList` + `TaskItem` (nested) และเปิด Link ผ่าน `StarterKit` (`openOnClick`, `autolink`; ไม่ต้องเพิ่ม dependency เพราะ StarterKit v3 มี Link อยู่แล้ว — ที่ผ่านมาแค่ปิดไว้) พร้อม style ด้วย Tailwind arbitrary variant (เส้นใต้ลิงก์, checkbox แบบ flex, ข้อที่ติ๊กแล้วขีดฆ่าสีจาง)
- `slashCommand.ts`: เพิ่มรายการ "To-do list"
- `NoteEditorBubbleMenu.tsx`: ปุ่ม ☑ (To-do) และ 🔗 (Link) — กด 🔗 แล้วช่อง URL แสดงในเมนูลอยเอง (Enter = Apply, Esc = ยกเลิก, ช่องว่าง + Apply = ลบลิงก์), URL ที่ไม่มี scheme ได้ `https://` ให้ ส่วน scheme อันตรายอย่าง `javascript:` ถูกปฏิเสธโดย allow-list เริ่มต้นของ Link
- `GLOSSARY.md` "Note": เพิ่ม to-do list และลิงก์ภายนอกในรายการ content type

Verify: `npx tsc -b`/`npm run lint`/`npm run build` ผ่าน; browser จริง — Markdown `- [ ]`/`- [x]` และ `[text](url)` โหลดเป็น checkbox/ลิงก์ (ลิงก์เปิดแท็บใหม่), คลิกติ๊ก checkbox แล้ว autosave กลับเป็น `- [x]` ถูกต้อง, เลือกข้อความ → 🔗 → พิมพ์ `example.com/week` → Enter ได้ `[Plan the week](https://example.com/week)`, Topic Reference จาก ticket 042/048 ยังเป็นการ์ดปกติและ reload แล้วยังอยู่ (ไม่ถูก link mark แย่ง)

ข้อจำกัดที่รู้: (1) ไม่มี shortcut Ctrl+K และไม่มี popup แก้ลิงก์เมื่อคลิกลิงก์ (คลิก = เปิดแท็บใหม่ ต้องเลือกข้อความเพื่อแก้) (2) Markdown ของ to-do ถูก serialize เป็น loose list (มีบรรทัดว่างคั่นระหว่างข้อ) — ไม่กระทบการแสดงผล (3) ไม่ได้ทดสอบกด "To-do list" จากเมนู `/` ด้วยมือ, light mode และมือถือ
