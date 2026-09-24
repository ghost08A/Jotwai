---
id: 25
title: "สร้าง: การบังคับใช้ User Ownership (ไม่มีการจัดหมวดหมู่หรือย้ายซ้ำโดยอัตโนมัติหลังจากจัดระเบียบครั้งแรก)"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: [020-feature-note-capture-crud.md, 022-feature-topic-hierarchy.md, 023-feature-category-flat.md, 024-feature-ai-categorization-flow.md]
---

## Question

Product spec (Requirements v0.1 §8–§9; GLOSSARY.md "User Ownership") ตัดสินใจแล้วว่า: เมื่อ Topic/Category ของ Note ถูกกำหนดแล้ว (โดยผู้ใช้หรือโดยการยอมรับ Suggestion ของ AI) ระบบจะไม่ย้ายหรือจัดหมวดหมู่ใหม่โดยอัตโนมัติอีกเลย — การแก้ไขเนื้อหาจะไม่ทริกเกอร์ AI ซ้ำ และมีเพียงการกระทำที่ผู้ใช้สั่งอย่างชัดเจนเท่านั้นที่จะเปลี่ยน Topic/Category ได้ นี่ไม่ใช่ subsystem แยกต่างหากที่ต้องสร้างขึ้น แต่เป็น invariant ที่ Notes/Topic/Category/AI Categorization ทั้งหมดต้องปฏิบัติตาม — ถูกติดตามเป็นตั๋วของตัวเองเพื่อไม่ให้ถูกมองข้ามอย่างเงียบ ๆ ในขณะที่ส่วนเหล่านั้นถูกสร้างขึ้นทีละส่วน (เช่น อย่าเชื่อมต่อ "เมื่อมีการอัปเดต Note" ให้รัน AI Categorization ซ้ำโดยไม่ตั้งใจ)

## Finding

ยังไม่เริ่มดำเนินการ — ยังไม่มีโค้ดสำหรับส่วนใด ๆ ที่ invariant นี้กำกับควบคุมอยู่เลย

## Resolution

Audit เสร็จแล้ว (2026-09-24) — ตามที่ ticket นี้ระบุไว้เองว่า "ไม่ใช่ subsystem แยกต่างหาก" invariant นี้ถูกบังคับใช้ผ่านการออกแบบของ 020/022/023/024 อยู่แล้ว ไม่ต้องสร้างโค้ดใหม่ ยกเว้น gap หนึ่งจุดที่เจอระหว่าง audit:

**ยืนยันว่าถูกต้อง**:
- `NotesService.UpdateAsync` (overwrite เนื้อหา) ไม่แตะ `TopicId`/`NoteCategories` เลย
- Topic ย้ายได้ผ่าน `PUT /api/notes/{id}/topic` เท่านั้น — ไม่มีที่ไหนใน codebase เรียก `AssignTopicAsync` โดยอัตโนมัติ
- Category attach/detach ผ่าน `POST/DELETE /api/notes/{id}/categories/{categoryId}` เท่านั้น — ไม่มีการเรียกอัตโนมัติเช่นกัน
- `Notes.AiCategorizationTriggeredAt` (ticket 033) กันไม่ให้ auto-trigger ซ้ำ แม้แก้ไขเนื้อหากี่ครั้งก็ตาม — พิสูจน์ด้วย `AiCategorizationTests.Editing_a_note_never_triggers_a_second_categorization`
- Accept/Ignore suggestion resolve ชุดนั้นถาวร ไม่มีการสร้าง suggestion ใหม่แทนที่อัตโนมัติ

**Gap ที่พบและแก้แล้ว**: ไม่มี endpoint on-demand re-trigger ตามที่ GLOSSARY.md/ARCHITECTURE.md ระบุไว้ ("ผู้ใช้ trigger ซ้ำเองได้ผ่าน action แบบ on-demand") — เพิ่ม `POST /api/notes/{id}/suggestion/trigger` แล้ว บันทึกรายละเอียดไว้ที่ [ticket 024](024-feature-ai-categorization-flow.md) (Update 2026-09-24) เพราะเป็นโค้ดของ feature นั้น ไม่ใช่ของ ticket นี้โดยตรง

Integration test ที่พิสูจน์ invariant นี้ครบอยู่ใน `NotesTests.cs`, `TopicsTests.cs`, `CategoriesTests.cs`, `AiCategorizationTests.cs` แล้ว (รวม 59/59) — ไม่ต้องเพิ่ม test ใหม่เฉพาะสำหรับ ticket นี้
