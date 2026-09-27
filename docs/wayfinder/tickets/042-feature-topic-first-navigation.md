---
id: 42
title: "สร้าง: Topic-first navigation — ตัด Notes nav, เลิกบังคับ H1 ของ Note, เพิ่ม /subtopic"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ผู้ใช้ขอปรับ UI/navigation ของ client ให้เป็น Topic-first มากขึ้น หลัง ticket 039 (topic browser + topic detail) — grill กันจนได้ scope ที่ยืนยันแล้วดังนี้ (2026-09-27):

### เอา "Notes" ออกจาก nav และลบ `/notes` ทิ้ง

- เอาลิงก์ "Notes" ออกจาก `PrivateLayout` — Categories/Trash ยังอยู่ใน nav เหมือนเดิม
- ลบ route `/notes` และ `NotesPage.tsx` ทิ้งทั้งหมด — Note ทุกอันตอนนี้เข้าถึง/สร้าง/แก้ไขผ่านการกด Topic เท่านั้น (หน้า Topic Detail จาก ticket 039)
- เอาตัวเลือก **"No topic"** ออกจาก dropdown ย้าย Topic ใน `NoteCard` ด้วย — เพราะถ้าเลือกแล้ว Note จะไม่มีหน้าไหนให้เห็น/แก้ไขมันได้อีกเลยจาก UI (หายไปจากสายตาแต่ยังอยู่ใน database) การจัดการผ่าน UI จากนี้ Note ต้องมี Topic เสมอ

**หมายเหตุ**: ที่ระดับ data model, Note ยังคงมีอยู่ได้โดยไม่มี Topic ตาม GLOSSARY.md เดิม (ไม่เปลี่ยน backend/API) — นี่เป็นแค่ข้อจำกัดระดับ client UI เท่านั้น

### เลิกบังคับ heading ระดับ 1 เป็น block แรกของทุก Note (ย้อนกลับบางส่วนของตั๋ว 037)

- เอา `TitleDocument`/`TitleLevelGuard` ออกจาก `NoteEditor` และเลิกเรียก `ensureLeadingHeading` — Note ใหม่ที่ว่างเปล่าเริ่มต้นเป็น paragraph ธรรมดา ไม่ใช่ heading บังคับอีกต่อไป
- เหตุผล: ตอนนี้หน้า Topic Detail แสดงชื่อ Topic เป็นหัวเรื่องขนาดใหญ่อยู่แล้วเหนือ note list ทั้งหมด — การให้แต่ละ Note มี H1 ของตัวเองซ้ำซ้อน ไม่จำเป็นอีกต่อไป
- Note เก่าที่มี `#` บังคับติดมาอยู่แล้วจากกฎเดิม **ปล่อยไว้เฉยๆ ไม่ migrate ย้อนหลัง** (ไม่มี version history อยู่แล้ว ไม่ควรแตะเนื้อหาของผู้ใช้แบบ retroactive โดยไม่มีใครขอ)
- "/" slash menu ยังมี "Heading 1/2/3" ให้เลือกใช้ได้ตามปกติในเนื้อหา แค่ไม่มีการ "บังคับ"/pin ตำแหน่งแรกอีกต่อไป
- GLOSSARY.md อัปเดตแล้ว (ตัดข้อความเรื่อง H1 บังคับออกจากนิยาม Note)

### Slash command ใหม่: `/subtopic`

- เพิ่มเข้าไปในเมนู slash command ของ `NoteEditor` (เดิมมีแค่ Text/Heading/List/Quote/Code block/Divider — ทั้งหมดเป็น block-transform ทันที ไม่ต้องมี input เพิ่ม)
- ต่างจากคำสั่งอื่นทั้งหมด: `/subtopic` ต้องมีชื่อ subtopic ก่อนถึงจะทำงานได้ — เลือกจากเมนูแล้วเปิด **popup/inline form แยกต่างหาก** ให้พิมพ์ชื่อ (ไม่ใช่พิมพ์ต่อท้ายในบรรทัดเดียวกันแบบ `/subtopic ชื่อ...`)
- ยืนยันชื่อแล้ว: สร้าง Topic ใหม่เป็น**ลูกของ Topic ปัจจุบันของ Note ที่กำลังแก้ไขอยู่** (ใช้ endpoint สร้าง Topic ที่มีอยู่แล้ว ระบุ `parentId`) แล้วแทรก **Topic Reference** (ดู GLOSSARY.md) ชี้ไปยัง Topic ใหม่นั้น ณ ตำแหน่ง cursor ในเนื้อหา
- Topic Reference แสดงเป็น**การ์ด block แยกบรรทัดของตัวเอง มีไอคอนกำกับ** (คล้าย nested-page block ของ Notion) กดแล้ว navigate ไปหน้า Topic Detail ของ Topic นั้น — **ไม่ใช่**ลิงก์ข้อความธรรมดาแบบ inline
- Note ที่พิมพ์อยู่**ไม่ย้าย**ไปอยู่ Topic ใหม่ ยังคงอยู่ Topic เดิมของตัวเองเหมือนเดิม — Topic Reference เป็นแค่ทางลัดให้กดไปดูเนื้อหาต่อ (อ่านมาถึงจุดนี้ ไปดู subtopic ต่อได้)
- **ขยาย scope ของ Note ใน GLOSSARY.md**: เพิ่มความสามารถเก็บ Topic Reference เป็น content type ใหม่ (เดิม `link: false` ถูกปิดไว้ตั้งใจตาม comment ใน `NoteEditor.tsx` — ตอนนี้เปิดใช้เฉพาะสำหรับ Topic Reference นี้เท่านั้น ไม่ใช่ลิงก์ทั่วไป)
- **ยังไม่ตัดสินใจในตั๋วนี้ (implementer ต้องออกแบบตอนเขียนโค้ด)**: วิธี serialize Topic Reference เป็น Markdown ให้ round-trip ผ่าน `tiptap-markdown` ได้ถูกต้อง (ต้องมีทั้งชื่อ Topic ที่แสดงและ Topic ID ที่อ้างอิงอยู่ในนั้น) — ถ้าเป็นการตัดสินใจที่ยาก reverse และมี trade-off จริง ให้พิจารณาเขียน ADR ประกอบตอน implement
