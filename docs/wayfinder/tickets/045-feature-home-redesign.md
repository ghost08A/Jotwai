---
id: 45
title: "ปรับหน้า Home (Topic browser) ให้ดูดีขึ้นและไม่งงสำหรับคนเริ่มใช้"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ผู้ใช้แจ้งว่าหน้า Home (`/`, `TopicBrowserPage`) ซึ่งเป็นหน้าแรกรวม root Topic ทั้งหมด ดูไม่สวยและทำให้คนเริ่มใช้งงว่าต้องทำอะไร (2026-09-29)

ปัญหาที่วินิจฉัยได้จากโค้ด:

- หัวข้อ `Jotwai` ซ้ำกับโลโก้ใน header และไม่บอกว่าหน้านี้คืออะไร
- ไม่มีคำอธิบายว่า Topic คืออะไร / เริ่มยังไง — ปุ่ม "+ New topic" เล็ก (`size="sm"`) หลบอยู่มุมขวาของ label "Topics"
- เนื้อหาเต็มความกว้างจอ (`lg:px-24`) อ่านยากบนจอกว้าง
- ทุก Topic เป็นแถวข้อความล้วนในกล่องเดียว ลูกศร expand เล็ก (20px), จุด suggestion เล็กและไม่บอกความหมาย, ไม่บอกว่ามี subtopic กี่อัน
- empty state เป็นแค่ข้อความ "No topics yet."

## Decision

ผู้ใช้เลือก Layout แบบ **Grid Cards** (2026-09-29) และอนุญาตให้เพิ่มข้อมูล preview/วันที่แก้ล่าสุด ผลการตรวจพบว่า **ไม่ต้องแก้ backend เลย** เพราะ `GET /api/notes` ที่มีอยู่แล้วคืน Note ที่ active ทั้งหมดพร้อม `content`, `topicId`, `updatedAt` และ 1 Topic มี Note เดียว (ticket 043) — client จึงประกอบ preview เองจาก `useNotes()`

## Resolution

Build เสร็จและ verify ผ่าน browser จริงกับ backend จริง (2026-09-29) เฉพาะฝั่ง client:

- `TopicBrowserPage.tsx`: grid 1/2/3 คอลัมน์ (`max-w-5xl`), การ์ดแรกเป็น "+ New topic" (แทนปุ่มเล็กเดิม), เรียง Topic ตามแก้ล่าสุดก่อน, ซ่อนช่องค้นหา/chip เมื่อยังไม่มี Topic เลย, skeleton เป็นการ์ด, empty state อธิบายในการ์ด "+ New topic"
- `TopicCard.tsx` (ใหม่, แทน `TopicBrowserNode.tsx` ที่ลบแล้ว): ชื่อ + preview 3 บรรทัด + chip subtopic (สูงสุด 3, "+N more") + วันที่แก้ล่าสุด (`YYYY-MM-DD`) + ป้าย "Suggestion"; ทั้งการ์ดคลิกได้ผ่าน stretched link โดย chip subtopic ยังคลิกแยกได้
- `utils/buildNotePreview.ts` (ใหม่): แปลง Markdown เป็น plain-text preview (ตัด H1 บรรทัดแรกที่ซ้ำชื่อการ์ด) — ยังไม่มี unit test เพราะ client ยังไม่มี test runner (ดู client/CLAUDE.md)

Verify: `npx tsc -b`/`npm run lint`/`npm run build` ผ่าน; ผ่าน browser จริง (dark mode) ทั้ง empty state, grid ที่มี Topic + subtopic + preview, และคลิก chip subtopic ไป Topic ที่ถูกต้อง ยังไม่ได้เช็ก light mode, mobile และ filter mode
