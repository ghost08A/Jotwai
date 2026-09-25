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

**ยังขาด (ตอนนั้น)**: component library สไตล์ shadcn/ui (ยังไม่ได้ตั้ง — งานตอนนั้น scope แค่ base editor ตามที่ผู้ใช้ร้องขอ ยังไม่แตะ headless component library), โค้ดฟีเจอร์ Note/Topic/Category/Search จริงที่เรียก backend API ([ticket 020](020-feature-note-capture-crud.md)/[022](022-feature-topic-hierarchy.md)/[023](023-feature-category-flat.md)/[024](024-feature-ai-categorization-flow.md)/[026](026-feature-search.md) ฝั่ง frontend), [Code Block ticket 021](021-feature-code-block.md) เอง (editor รองรับ code block แล้ว แต่ยังไม่ผูกกับ Note ตัวจริงผ่าน API)

**Re-audit 2026-09-25 (รอบสอง — headless component library)**: shadcn/ui-style component ถูกตั้งค่าแล้ว — `components.json` (style `new-york`, base color `neutral`, alias ชี้เข้า `shared/components`/`shared/utils` ตาม folder convention ของ project แทน default `@/lib`/`@/components`), path alias `@/*` → `src/*` (`tsconfig.json`, `tsconfig.app.json`, `vite.config.ts`), `cn()` helper (`shared/utils/cn.ts`, clsx + tailwind-merge) Design token ประกาศครั้งเดียวใน `index.css` ผ่าน Tailwind v4 `@theme inline` (ตาม client/CLAUDE.md "UI & Design System" เป๊ะ) — `background`/`foreground`/`card`/`primary`/`secondary`/`muted`/`accent`/`destructive`/`border`/`input`/`ring` และ `radius-*` เพิ่ม component ปฐมภูมิ 3 ตัวแรก: `Button`, `Input`, `Label` (`shared/components/ui/`) มาจาก shadcn CLI จริง (`npx shadcn@2.3.0 add`, Radix primitives อยู่หลัง `Label`/`asChild` ของ `Button`) แก้ `transition-colors` ที่ generator ใส่มาให้เป็น `transition-[background-color]`/`transition-[color,box-shadow]` ตามกฎ client/CLAUDE.md ที่ห้ามใช้ `transition-colors` ตรงๆ

พิสูจน์ว่าใช้งานจริงไม่ใช่ abstraction ที่ไม่มีคนเรียก: refactor `LoginPage`/`RegisterPage`/`Home` ให้ใช้ `Button`/`Input`/`Label` + design token แทน raw `neutral-*` Tailwind class เดิมทั้งหมด verify ผ่าน browser จริง (login/register page render ถูกต้อง, validation error `text-destructive` แสดงผลถูกต้อง, พฤติกรรมเดิมไม่เปลี่ยน) `npm run lint`/`npm run build` ผ่านสะอาด

**ยังขาด**: component เพิ่มเติมที่ feature อื่นจะต้องใช้ (เช่น Select/Dialog/Textarea สำหรับ Topic/Category/Suggestion UI — ยังไม่สร้างเพราะยังไม่มี consumer จริง), โค้ดฟีเจอร์ Note/Topic/Category/AI Categorization/Search จริงที่เรียก backend API ยังไม่ได้ทำ

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่ เหลือแค่ feature code ที่ต้องต่อ (base editor + component library พื้นฐานพร้อมแล้วทั้งคู่)
