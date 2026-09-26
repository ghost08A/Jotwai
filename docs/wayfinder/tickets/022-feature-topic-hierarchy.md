---
id: 22
title: "สร้าง: โครงสร้างลำดับชั้นของ Topic (สร้าง/ซ้อนกัน, ความไม่ซ้ำกันของ sibling, หนึ่ง Note ต่อหนึ่ง Topic, การป้องกันการลบเมื่อยังไม่ว่าง)"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §4; GLOSSARY.md "Topic"; BR-003, BR-004) ตัดสินใจแล้วว่า: Topic มีลำดับชั้นแบบไม่จำกัดความลึก ชื่อของ sibling ต้องไม่ซ้ำกันแบบไม่สนตัวพิมพ์เล็ก-ใหญ่ภายใต้ parent เดียวกัน Note หนึ่งรายการอยู่ใน Topic ได้อย่างมากหนึ่งรายการ Topic ที่ยังมี Note หรือ Topic ลูกอยู่จะลบไม่ได้จนกว่าจะว่างเปล่า และการย้าย Note ไปยัง Topic อื่นต้องเป็นการกระทำที่ผู้ใช้สั่งอย่างชัดเจนเสมอ ยังไม่มีสิ่งใดอยู่ในโค้ด (Module Organization ยังไม่ถูกสร้างเลย — ดูการตรวจสอบ ADR-0001, ตั๋ว 011)

หมายเหตุ: ประเด็นที่ยังไม่ชัดเจน ("Not yet specified") บน map (ว่าการย้าย Note ไปยัง Topic ที่ยังไม่มีอยู่จะสามารถสร้าง Topic นั้นแบบ inline ได้หรือไม่) ควรถูกแก้ไขในฐานะตั๋วสำหรับ grilling แยกต่างหาก ก่อนหรือระหว่างการสร้างงานนี้ — ไม่ควรสันนิษฐานไปทางใดทางหนึ่งในที่นี้

## Finding

ยังไม่เริ่มดำเนินการ ไม่มีเอนทิตี Topic, DbContext หรือ UI อยู่ที่ใดในโปรเจกต์เลย

## Resolution

Build เสร็จและ verify ครบวงจรแล้ว (2026-09-24) — เฉพาะฝั่ง backend (client UI ยังไม่ทำ ไม่อยู่ใน scope ของ ticket นี้) ก่อนเริ่มมีการ grill 2 ประเด็นเพิ่ม (ดูคำถามที่ ticket นี้ระบุไว้เอง):

- **[ticket 036](036-topic-must-exist-before-note-move.md)**: การย้าย Note ไป Topic ต้องมี Topic อยู่ก่อนแล้วเท่านั้น ไม่รองรับสร้างแบบ inline
- **Scope**: เลือกทำเต็มขอบ รวม cross-module read model จริง (ตัวแรกของ repo นี้) แทนที่จะแยกส่วนผูก Note ออกเป็น ticket ต่างหาก

Implementation:

- `Jotwai.Domain`: `Models/Topic.cs` (`Id`, `UserId`, `ParentId` nullable self-FK ตาม decision 029, `Name`/`NormalizedName`, `NoteCount` — denormalized read model ของ Organization เอง), `Events/TopicCreated.cs`/`TopicRenamed.cs`/`TopicDeleted.cs`, `Events/NoteTopicAssigned.cs` (publish โดย Notes), `Models/KnownTopic.cs` (read model ของ Notes เอง), `Interfaces/ITopicsService.cs`, extend `INotesService` ด้วย `AssignTopicAsync`, request/view/result model ที่เกี่ยวข้อง
- **Cross-module read model แรกของ repo (ปิด gap หลักของ [ticket 013](013-audit-adr-0003.md) ไปด้วย)**:
  - Notes subscribe `TopicCreated`/`TopicDeleted` ของ Organization เพื่อรักษา `KnownTopics` (schema `notes`) — ใช้ validate ว่า `TopicId` ที่จะ assign มีอยู่จริงและเป็นของ user นั้น โดยไม่ query schema `organization` ตรงๆ
  - Organization subscribe `NoteTopicAssigned` ของ Notes เพื่อรักษา `Topics.NoteCount` (schema `organization`) — ใช้บังคับกฎ "Topic ที่มี Note จะลบไม่ได้" โดยไม่ query schema `notes` ตรงๆ
  - Handler อยู่ที่ `Jotwai.Service/EventHandlers/NotesKnownTopicsProjectionHandler.cs` และ `OrganizationNoteCountProjectionHandler.cs` ลงทะเบียนใน `NotesServiceRegistration`/`OrganizationServiceRegistration` ตามลำดับ
- `Jotwai.Service`: `OrganizationDbContext` (schema `organization`), migration, `TopicsService` (create/list/get/rename/delete ครบตามกฎ sibling-uniqueness แบบ case-insensitive ผ่าน partial unique index สองอัน — root กับ non-root แยกกัน เพราะ unique index ธรรมดาจะปล่อยให้ `ParentId IS NULL` ซ้ำกันได้เงียบๆ, delete guard เช็คทั้ง child topics และ `NoteCount`), `NotesService.AssignTopicAsync`
- `Web.Jotwai`: `TopicsController` (`POST/GET/PUT/DELETE /api/topics`), `NotesController` เพิ่ม `PUT /api/notes/{id}/topic`
- Integration test 13 เคสใหม่ (`TopicsTests.cs`) ครอบคลุม hierarchy, sibling uniqueness (รวมชื่อซ้ำข้าม parent ต่างกันที่อนุญาต), parent ไม่พบ, rename ชนชื่อ, delete guard ทั้งสองแบบ (child topics / notes), assign ข้าม user ไม่ได้, unauthorized — รวม test เดิมผ่านครบ **39/39**, `dotnet format --verify-no-changes` ผ่าน
- Verify end-to-end จริงผ่าน curl: nested topic, duplicate-sibling 409, cross-parent เดียวกันชื่อซ้ำได้, delete-with-children 409, assign note→topic, delete-with-notes 409, assign ไป topic ที่ไม่มี 400, clear แล้ว delete สำเร็จ — ครบทุก use case

**Client UI (2026-09-25)**: build เสร็จและ verify ครบวงจรจริงแล้วผ่าน browser ต่อ backend จริง —

- `modules/organization/` ใหม่ทั้งหมด (types, `services/topicsService.ts`, hooks `useTopics`/`useCreateTopic`/`useRenameTopic`/`useDeleteTopic`), `utils/buildTopicTree.ts` (flat list → tree แบบ pure function ไม่มี side effect, sort ตามชื่อ), `utils/flattenTopicsForSelect.ts` (tree → option list แบบ indent สำหรับ `<select>`)
- `components/TopicsPage.tsx` + `TopicNode.tsx` (แสดงเป็น tree แบบ recursive, indent ด้วย Tailwind step ที่ fix ไว้ล่วงหน้าแทน inline style ตามกฎ client/CLAUDE.md, ปุ่ม "+ Subtopic"/Rename/Delete ต่อ node) + `CreateTopicForm.tsx` (ใช้ร่วมกันทั้ง root และ subtopic ผ่าน `parentId` prop) เพิ่ม route `/topics` + nav link ใหม่ใน `PrivateLayout`
- ผูก Note↔Topic จาก `NoteCard.tsx`: เพิ่ม `assignNoteTopic` ใน `modules/notes/services/notesService.ts` + `useAssignNoteTopic` hook, การ์ดของ Note ทุกใบมี `<select>` เลือก Topic (แสดงลำดับชั้นด้วย indent จาก `flattenTopicsForSelect`) เปลี่ยนแล้ว save ทันที แยกจาก action แก้เนื้อหา ตรงตามที่ backend ออกแบบ `AssignTopicAsync` ไว้เป็นคนละ action
- Verify ผ่าน browser จริงกับ backend จริงครบ: สร้าง root topic, สร้าง subtopic ซ้อนใน UI เห็น indent ถูกต้อง, assign note ไปยัง subtopic แล้ว reload หน้าเว็บยืนยันว่า persist จริง (ไม่ใช่ optimistic state ค้าง), พยายามลบ Topic ที่ยังมี Note อยู่เห็น error message จริงจาก backend ("Topic still has Notes.") แสดงผลถูกต้อง, rename ทำงานถูกต้อง
- `npm run lint` + `npm run build` ผ่านสะอาด
- ยังไม่ทำ: drag-and-drop ย้าย Topic/reparent จาก UI (ไม่เคยอยู่ใน scope ของ product spec), การแสดง Note count ต่อ Topic ใน UI (backend ไม่ได้ expose `NoteCount` ใน `TopicResponse` — เป็น internal read model ของ Organization เอง ไม่ใช่ API field)
