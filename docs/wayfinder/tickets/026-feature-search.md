---
id: 26
title: "สร้าง: Search (เนื้อหารวมถึง code block, Topic, Category — ค้นหาแบบ substring ไม่สนตัวพิมพ์เล็ก-ใหญ่)"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §10–§11; GLOSSARY.md "Search"; BR-014 ถึง BR-016) รวมถึง ADR-0006 ตัดสินใจแล้วว่า: การจับคู่แบบ substring ที่ไม่สนตัวพิมพ์เล็ก-ใหญ่ ครอบคลุมเนื้อหาของ Note (รวมถึงเนื้อหาภายใน code block), ภาษาที่ระบุของ code block, Topic และ Category ผ่านทาง Postgres `ILIKE` + ดัชนี `pg_trgm` GIN บน read model ของ Search Module เอง การกรองตาม Topic ต้นทางจะรวม Topic ลูกหลานด้วย ไม่มี semantic/vector search สำหรับ MVP (เป็น non-goal อย่างชัดเจน) ยังไม่มีสิ่งใดอยู่ในโค้ด — ยืนยันแล้วจากการตรวจสอบ ADR-0006 (ตั๋ว 015) ถูก block อยู่จนกว่า Module Notes/Topic/Category จะมีอยู่จริง (read model ต้องมีสิ่งที่จะ project มาจาก) และกลไก Domain Event (ตั๋ว 012)

## Finding

ยังไม่เริ่มดำเนินการ

## Resolution

ปิดแล้ว (2026-09-25) — build เสร็จและ verify ครบวงจรฝั่ง backend: schema `search` ใหม่ทั้งหมด (`SearchDbContext`, migration แรกเปิดใช้ `pg_trgm` + GIN trigram index บน `SearchDocuments.Content`), `SearchController`/`SearchService` (`GET /api/search?q=&topicId=`)

- **Read model (ADR-0003)**: `SearchDocument` (สำเนา Note ต่อแถว), `SearchTopic`/`SearchCategory` (mirror ของ Organization ทั้งสอง เพื่อ resolve ชื่อ Topic/Category และ Topic descendant ได้โดยไม่ query ข้าม schema), `SearchDocumentCategory` (join) — sync ทั้งหมดผ่าน event handler ใหม่ 3 ตัว (`SearchNoteProjectionHandler`, `SearchTopicProjectionHandler`, `SearchCategoryProjectionHandler`) subscribe event ของ Notes/Organization ที่มีอยู่แล้ว
- **Matching**: `ILIKE` แบบ escape wildcard บน Content, TopicName (denormalized), และ Category name (join กับ `SearchCategory` ที่ query time) ตรงตาม ADR-0006
- **Topic filter รวม descendant**: คำนวณจาก `SearchTopic` mirror ของตัวเอง (BFS ในหน่วยความจำ ไม่มี recursive SQL) ตรงตาม decision ของ ADR-0006
- **พบและปิด gap ระหว่างทาง**: `NotesService.RestoreAsync` ไม่เคย publish event ใดๆ เลย (เขียนแค่ clear `DeletedAt`) ทำให้ Search จะไม่รู้ว่า Note ถูกกู้คืนจาก Trash แล้ว เพิ่ม event `NoteRestored` ใหม่และ publish จากจุดนั้น — Search subscribe เพื่อ clear `DeletedAt` ของตัวเองกลับ
- Deleted/trashed Note ถูกกันออกจากผลการค้นหา (ผ่าน `SearchDocument.DeletedAt`) และกลับมาเมื่อกู้คืน
- Integration test ใหม่ 7 ตัวผ่าน (รวม 66/66): content/code-block substring match, category name match, topic filter รวม descendant, trash/restore, user isolation, `q` ว่างคืน note ทั้งหมด, unauthenticated ถูกปฏิเสธ
- `dotnet format --verify-no-changes` ผ่านทั้ง solution (ต้องแก้ BOM/CRLF ของไฟล์ migration ที่ `dotnet ef migrations add` generate ให้เอง ก่อนจะผ่าน)
- Client UI ยังไม่ทำ ทำให้ [ADR-0006 audit (ticket 015)](015-audit-adr-0006.md) ปิดตามไปด้วยฝั่ง backend
