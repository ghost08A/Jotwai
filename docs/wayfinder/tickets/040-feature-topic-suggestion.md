---
id: 40
title: "สร้าง: Topic Suggestion — AI แนะนำ merge/ย้าย Topic ใหม่"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ผู้ใช้ขอให้ Nerd Assistant (เดิมชื่อ AI Categorization — ดู GLOSSARY.md/ARCHITECTURE.md ที่อัปเดตไปแล้ว) วิเคราะห์ Topic ที่เพิ่งสร้างใหม่และแนะนำการจัดระเบียบให้ — grill กันจนได้ scope ที่ยืนยันแล้วดังนี้ (2026-09-27):

### Trigger

- วิเคราะห์แบบ background ทันทีที่ Topic ถูกสร้าง (subscribe `TopicCreated` ที่มีอยู่แล้ว) เขียนแถว outbox แบบเดียวกับ AI Categorization flow เดิม (ADR-0004) — ไม่บล็อก UI ตอนกด "สร้าง Topic"
- Nerd Assistant ต้องมี read model ของตัวเอง (Id/Name/ParentId ของทุก Topic ของ user) sync ผ่าน `TopicCreated`/`TopicRenamed`/`TopicDeleted` — ห้าม query schema `organization` ตรงๆ (ADR-0003) เหมือน `CategoryProjections` ที่มีอยู่แล้วสำหรับ Category

### ผลลัพธ์จาก AI

- เรียก AI provider **ครั้งเดียว** ต่อ Topic ใหม่หนึ่งตัว เทียบชื่อ Topic ใหม่กับชื่อ Topic ที่มีอยู่แล้ว**ทั้งหมด**ของ user (ทุก branch/ทุกระดับชั้น ไม่จำกัดแค่ sibling เดียวกัน — Topic คนละ parent ชื่อเหมือนกันก็ยังถือว่าอาจซ้ำได้) ให้ตอบกลับมาเป็นหนึ่งในสามแบบ: **Merge**, **Placement**, หรือไม่เข้าเงื่อนไขไหนเลย
- ถ้าเข้าเงื่อนไข **Merge** ให้ Merge ชนะเสมอ — ไม่เสนอ Placement ซ้อนสำหรับ Topic เดียวกัน
- เสนอแค่**ตัวเลือกเดียว**ต่อครั้ง (ไม่ใช่ ranked list แบบ Category Suggestion — Topic มีแม่ได้แค่หนึ่งเดียวเสมอ)
- มี **confidence threshold ของตัวเอง แยกจาก Category Suggestion** (เกณฑ์สูงกว่า เพราะ Merge เป็น action ที่ทำลายข้อมูลจริง) ต่ำกว่า threshold แล้วไม่ต้องสร้าง suggestion เลย เหมือน pattern เดิมของ ticket 009/035

### Merge

- Topic เดิม (ที่มีอยู่ก่อน) เป็นผู้รอดเสมอ Topic ใหม่ถูกลบทิ้ง
- Note ทั้งหมดที่อยู่ใต้ Topic ใหม่ (ถ้ามี) ย้ายไปอยู่ใต้ Topic เดิม
- Subtopic ลูกของ Topic ใหม่ (ถ้ามี ณ จังหวะเดียวกัน) ย้ายไปเป็นลูกของ Topic เดิมด้วย เพื่อไม่ให้ tree ขาด
- เป็น cross-schema operation ที่ต้อง atomic (ห้ามสำเร็จแค่ครึ่งเดียว) — ใช้ pattern เดียวกับ `SuggestionsService.AcceptAsync` (bulk-accept Category Suggestion) ที่มีอยู่แล้ว: เปิด connection/transaction เองข้าม `NotesDbContext`/`OrganizationDbContext`/Nerd Assistant's DbContext แทน Domain Event แบบ fire-and-forget
- ผู้ใช้กด "ยืนยัน" แล้ว**ต้องมี dialog ยืนยันซ้ำอีกชั้น** ("แน่ใจนะ? กู้คืนไม่ได้") ก่อนทำจริง เพราะเป็น action ทำลายข้อมูล

### Placement

- ผู้ใช้กด "ยืนยัน" แล้ว Topic ใหม่ย้ายไปเป็นลูกของ Topic ที่แนะนำทันที **ไม่ต้อง**ยืนยันซ้ำ (ไม่มีข้อมูลสูญหาย)

### User Ownership

- ตอบ suggestion แล้ว (accept/decline ทั้ง Merge และ Placement) ไม่เสนอซ้ำอัตโนมัติอีก แม้จะ rename Topic ทีหลัง (ตาม pattern เดิมของ Category Suggestion)

### UI

- หน้า Topic Browser (ticket 039): จุดเตือนเล็กๆ (badge) ที่ตัว Topic ที่มี suggestion ค้างอยู่
- หน้า Topic Detail (ticket 039): รายละเอียดคำแนะนำ + ปุ่ม "ยืนยัน"/"ไม่เอา" (Merge มี dialog ยืนยันซ้ำตามข้างบน)

### Naming (ดู GLOSSARY.md ที่อัปเดตแล้ว)

- Module เปลี่ยนชื่อในเอกสารจาก "AI Categorization" เป็น **"Nerd Assistant"** — ของจริงในโค้ด (schema `ai_categorization`, class เช่น `AiCategorizationDbContext`) **ยังไม่เปลี่ยน** จะทำเป็น ticket แยกทีหลัง ticket นี้เขียนโค้ดใหม่ในโครงเดิม (schema/namespace เดิม) ไปก่อน
