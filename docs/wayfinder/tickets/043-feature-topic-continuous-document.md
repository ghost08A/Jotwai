---
id: 43
title: "สร้าง: 1 Topic = เอกสารต่อเนื่องเดียว (Notion-style autosave), เอา guard ลบ Topic ที่มี Note ออก"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ผู้ใช้ไม่ชอบหน้าตาเดิมของ Topic Detail (คอมโพสเซอร์ + ปุ่ม "Save note" + รายการ Note การ์ดแยกด้านล่าง ตาม ticket 039/042) อยากได้ประสบการณ์แบบ Notion: เปิด Topic มาพิมพ์ต่อได้ทันที เนื้อหาต่อเนื่องกันไปเรื่อยๆ ไม่แยกเป็น Note ย่อยหลายก้อน ไม่มีปุ่ม Save (autosave แทน) เปิดใหม่แล้วต้องเห็นเหมือนกำลังเขียนต่อ

Grill กันจนได้ scope ที่ยืนยันแล้วดังนี้ (2026-09-27):

### ทิศทาง UX (prototype ก่อน แล้วค่อย implement จริง)

ทำ throwaway prototype ก่อน (`?variant=notion` เทียบกับ `?variant=current`, ใช้ localStorage แทน backend) เพื่อพิสูจน์ความรู้สึกโดยไม่เสี่ยงกับ schema จริง — ผู้ใช้ยืนยันทิศทางที่ชอบ: layout กว้างเต็มจอแบบเดิม (`current`) + เอกสารเดียวต่อเนื่อง + ไม่มีปุ่ม Save + ปุ่ม AI (Content Polish, ✨) ล็อกตำแหน่งมุมล่างขวาจอถาวร (`fixed`, ไม่ใช่ `absolute` ที่เลื่อนตาม scroll) + ไม่มี section "Subtopics" แยกต่างหาก (ใช้ `/subtopic` slash command ที่มีอยู่แล้วจาก ticket 042 แทน)

### แนวทาง data model: เก็บ `Note` entity เดิม + unique constraint (ไม่ลบ `Note` ทิ้ง)

ประเมิน 2 ทาง: (A) เก็บ `Note` entity เดิมทั้งหมด แค่เพิ่มกฎ "1 Topic ผูกกับ Note ที่ยังไม่ลบได้อย่างมาก 1 ตัว" vs (B) ลบแนวคิด `Note` ทิ้งให้ `Topic` มี content ตรงๆ — เลือก **(A)** เพราะ Category Suggestion, AI Categorization outbox, Search read-model, Trash ทั้งหมด key ด้วย `NoteId` อยู่แล้ว กระทบโมดูลอื่นน้อยกว่ามาก

### นโยบายชนกัน (conflict policy)

ถ้ามีคนพยายามเอา Note ไปไว้ใน Topic ที่มีเอกสารอยู่แล้ว (manual assign ผ่าน `NoteCard` dropdown, หรือ accept Topic Suggestion แบบ Merge ที่ทั้งสอง Topic มีเอกสารอยู่แล้วทั้งคู่) — **ปฏิเสธด้วย 409 Conflict** ไม่ auto-merge เนื้อหาให้ ผู้ใช้ไปรวมเนื้อหาเองในเอกสารเดียว

### Follow-up: เอา guard "Topic มี Note ลบไม่ได้" ออก

ทดสอบจริงแล้วเจอว่ากด Delete Topic ไม่ได้เพราะ guard เดิม (GLOSSARY.md ก่อนหน้านี้: "Topic ที่มี Note หรือ Topic ลูกอยู่ภายในโดยตรงจะลบไม่ได้จนกว่าจะว่างก่อน") — ไม่สมเหตุสมผลอีกต่อไปเมื่อ Note คือเอกสารของ Topic นั้นเอง ไม่ใช่สิ่งแปลกปลอมที่ต้องเอาออกก่อน ผู้ใช้ยืนยัน: เอา guard ออก, ลบ Topic แล้ว Note ของมัน (ถ้ามี) soft-delete เข้า Trash ไปด้วย (กู้คืนได้) guard "มี Topic ลูกลบไม่ได้" ยังคงอยู่เหมือนเดิม ("ย้าย topic เป็น subtopic ของ topic อื่น" ที่ผู้ใช้ถามถึง คือ `TopicsService.MoveAsync`/Topic Suggestion Placement เดิม — ยืนยันว่าเป็นคนละส่วนกับ Note ทั้งหมด ไม่ถูกแตะเลย)

## Resolution

Build เสร็จและ verify ผ่าน browser จริงกับ backend จริงครบวงจร (2026-09-27):

**Backend**
- `NoteConfiguration.cs`: `Notes.TopicId` เปลี่ยนจาก non-unique index เป็น unique partial index (`WHERE "TopicId" IS NOT NULL AND "DeletedAt" IS NULL`) — migration `AddUniqueActiveNotePerTopic`
- `NotesService`: เพิ่ม `GetByTopicAsync`/`UpsertByTopicAsync` (สร้าง Note ครั้งแรกพร้อม `TopicId` ตรงๆ + publish `NoteTopicAssigned(null → topicId)` เพื่อความเข้ากันได้กับ event contract เดิม, ครั้งถัดไป overwrite เหมือน `UpdateAsync`) — endpoint ใหม่ `GET`/`PUT /api/notes/by-topic/{topicId}`
- `AssignTopicAsync`/`RestoreAsync`: เช็คชนกันแล้ว (409 Conflict / unfile อัตโนมัติเมื่อ topic เดิมถูกคนอื่นครองไปแล้วระหว่างอยู่ใน Trash)
- `TopicSuggestionsService.AcceptMergeAsync`: เช็คก่อน merge ว่าทั้งสอง Topic มี Note อยู่แล้วทั้งคู่ไหม ถ้าใช่ → 409 ไม่ merge เลย
- `TopicsService.DeleteAsync`: เอาเช็ค `NoteCount > 0` ออก, `NotesKnownTopicsProjectionHandler` (subscribe `TopicDeleted` อยู่แล้ว) เพิ่ม soft-delete + unfile Note ที่เหลือของ Topic นั้น (publish `NoteDeleted` ต่อ)
- เก็บกวาด dead code ที่ตามมา: ลบ `Topic.NoteCount` field, `OrganizationNoteCountProjectionHandler`, migration `RemoveTopicNoteCount`
- 7 integration test ใหม่ + แก้ 1 test เก่าที่ premise เปลี่ยน (`Assigning_a_note_to_a_topic_blocks_deleting...` → `Deleting_a_topic_with_a_note_succeeds_and_soft_deletes_the_note_into_trash`) — รวม **84/84 ผ่าน**, `dotnet format --verify-no-changes` ผ่าน

**Frontend**
- `TopicDetailPage.tsx`: ไม่มี multi-note list/composer/ปุ่ม Save/section Subtopics แยกอีกต่อไป — ใช้ `NoteEditor` เดี่ยวผูกกับ `useTopicNote`/`useUpsertTopicNote` (debounce ~600ms ผ่าน `useDebouncedValue` ที่มีอยู่แล้ว) พร้อมข้อความสถานะ "กำลังบันทึก…/บันทึกแล้ว"
- `ContentPolishButton.tsx`: เปลี่ยนตำแหน่งปุ่ม AI จาก `absolute` (เลื่อนตาม scroll) เป็น `fixed` (ล็อกมุมล่างขวาจอถาวร) — กระทบทุกที่ที่ใช้ `NoteEditor` ไม่ใช่แค่หน้านี้
- ลบไฟล์ prototype ทั้งหมด (`TopicDetailPage.notionPrototype.tsx`, `PrototypeSwitcher.tsx`) หลังยืนยันทิศทางแล้ว
- `npx tsc -b`, `npm run lint`, `npm run build` ผ่านหมด

**GLOSSARY.md**: อัปเดตนิยาม Note/Topic ให้ตรงกับกฎใหม่ (1 Topic ↔ อย่างมาก 1 Note ที่ยังไม่ลบ, guard ลบเหลือแค่เรื่อง Topic ลูก, ลบ Topic แล้ว Note ไปด้วยกัน)

**Verify จริงผ่าน browser + backend จริง (test account แยกบน localhost)**: พิมพ์ทันทีไม่ต้องกดปุ่ม → เห็นสถานะ autosave → reload → เนื้อหายังอยู่ (มาจาก `PUT`/`GET .../by-topic/...` จริง ไม่ใช่ localStorage) → กด Delete topic ที่มีเอกสารอยู่ → สำเร็จ กลับหน้า Home → เช็ค Trash เจอ note นั้นจริงพร้อมปุ่ม Restore
