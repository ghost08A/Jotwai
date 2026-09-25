---
id: 17
title: "ตรวจสอบการ implement: ADR-0008 Frontend stack, styling, and note editor"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/17
---

## Question

โค้ดเบสปัจจุบัน implement การตัดสินใจที่บันทึกไว้ใน `docs/adr/0008-frontend-stack-and-editor.md` หรือไม่? อ่าน `client/package.json` เพื่อดู dependency และตรวจสอบ `client/src` ว่ามีโค้ดแอปจริง, การทำ routing, หรือ component ของ editor หรือไม่

## Finding (implement บางส่วนแล้ว)

ติดตั้งและใช้งานแล้ว: React 19, Vite, react-router-dom, TanStack Query, Tailwind — `main.tsx` เชื่อมต่อ `BrowserRouter` + `QueryClientProvider` ไว้แล้ว, `App.tsx` มี route ตัวอย่างเพียงหนึ่งเดียว `axios` เป็น dependency ที่ติดตั้งไว้แต่ยังไม่ได้ใช้งาน (ยังไม่มี API client/hook ใดอ้างอิงถึงมัน)

**ขาดไปทั้งหมด**: Tiptap (ไม่มี package `@tiptap/*` อยู่เลย), component library สไตล์ shadcn/ui ใดๆ (มีเพียง Tailwind class ดิบๆ เท่านั้นในตอนนี้), โค้ดฟีเจอร์ของ Note/Topic/Category ใดๆ, hook สำหรับดึงข้อมูลจริงใดๆ, editor ใดๆ

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่ เป็นเรื่องที่คาดไว้แล้ว: ฝั่ง frontend มีเพียงโครงสร้างเบื้องต้นเท่านั้นในตอนนี้

**Re-audit 2026-09-25**: `client/package.json` ถูก commit ไว้แล้วจริง (ตรงข้ามกับ finding เดิมที่ไม่แน่ใจ) และ `axios` ถูกใช้งานแล้วจริงผ่าน `core/http/httpClient.ts` (มาจาก [ticket 027 frontend login flow](027-feature-frontend-login.md)) — ส่วนนั้นของ finding เดิม stale แล้ว **Base editor (Tiptap) ถูก build แล้ว**: `@tiptap/react`, `@tiptap/pm`, `@tiptap/starter-kit`, `tiptap-markdown` ติดตั้งและใช้งานจริงใน `modules/notes/components/NoteEditor.tsx` + `NoteEditorToolbar.tsx` — StarterKit ถูก configure ให้เหลือเฉพาะ content type ที่ GLOSSARY.md "Note" ระบุ (plain text/bulleted list/numbered list/code block) ปิด heading/bold/italic/strike/blockquote/horizontalRule/code/link/underline ทั้งหมด เพราะไม่อยู่ใน scope, `tiptap-markdown` (`html: false`) ทำหน้าที่ serialize/deserialize เป็น Markdown ล้วนตาม ADR-0008 (Markdown เป็น source of truth, editor เป็นแค่ view), code block มี language selector ที่ round-trip เป็น Markdown fence (` ```javascript `) ได้จริง verify ผ่าน browser click-through (bullet/numbered list, code block toggle, เปลี่ยนภาษา, ดู markdown output ตรงกัน)

**ยังขาด**: component library สไตล์ shadcn/ui (ยังไม่ได้ตั้ง — งานนี้ scope แค่ base editor ตามที่ผู้ใช้ร้องขอ ยังไม่แตะ headless component library), โค้ดฟีเจอร์ Note/Topic/Category/Search จริงที่เรียก backend API ([ticket 020](020-feature-note-capture-crud.md)/[022](022-feature-topic-hierarchy.md)/[023](023-feature-category-flat.md)/[024](024-feature-ai-categorization-flow.md)/[026](026-feature-search.md) ฝั่ง frontend), [Code Block ticket 021](021-feature-code-block.md) เอง (editor รองรับ code block แล้ว แต่ยังไม่ผูกกับ Note ตัวจริงผ่าน API)

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่ เหลือ headless component library + feature code ที่ต้องต่อ
