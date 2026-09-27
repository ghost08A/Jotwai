---
id: 44
title: "สร้าง: '+ New topic' เข้าหน้าจดทันที, Topic Suggestion ใช้ชื่อ+เนื้อหา พร้อม trigger เอง/อัตโนมัติ"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

หลัง ticket 043 (1 Topic = เอกสารต่อเนื่อง) ผู้ใช้สังเกตว่ากด "+ New topic" ยังต้องพิมพ์ชื่อในฟอร์มเล็กๆ ก่อนถึงจะสร้าง Topic ได้ (เป็น root เสมอ) — ขัดกับ philosophy "จดก่อน จัดทีหลัง" อยากให้กดแล้วเข้าหน้าจดต่อเนื่องได้ทันที ไม่ต้องคิดชื่อ/ที่อยู่ก่อน แล้วให้ระบบตัดสินใจภายหลังว่า Topic นี้ควรรวมกับอันไหน ควรเป็น subtopic ของใคร หรือแยกเป็น root ใหม่

Grill กันจนตกผลึก (2026-09-27):

- สร้าง Topic ด้วยชื่อ placeholder ("Untitled", กันชนกับ sibling-uniqueness ที่มีอยู่แล้วด้วย "Untitled 2"/"Untitled 3"/...) ทันที แล้วพาไปหน้าจดเลย ไม่มี popup ถามชื่อก่อน
- การตัดสินใจ "ควรรวม/ควรย้าย/แยก root" ใช้ฟีเจอร์ **Topic Suggestion** ที่มีอยู่แล้ว (ticket 040) ไม่สร้างระบบใหม่ — แต่ต้องขยาย scope ให้วิเคราะห์จากทั้ง **ชื่อ** และ **เนื้อหา** (เดิมดูแค่ชื่อ ซึ่งตอนสร้างยังเป็น "Untitled" ไม่มีความหมาย)
- มี**ปุ่มกดเอง**ในหน้าจด ให้สั่งวิเคราะห์ใหม่ได้ทุกเมื่อ (ดูชื่อ+เนื้อหาปัจจุบัน ณ ตอนกด)
- ถ้าไม่กดแล้วออกจากหน้าไปที่อื่น ให้วิเคราะห์ให้เองเงียบๆ เบื้องหลัง (ผลโผล่เป็น suggestion ทีหลังผ่าน badge/panel ที่มีอยู่แล้ว) — เฉพาะกรณีมีการแก้ไขเนื้อหาจริงในรอบที่เปิดหน้านั้น กันวิเคราะห์ซ้ำเปล่าประโยชน์เวลาผู้ใช้แค่เปิดดูเฉยๆ

ระหว่างสำรวจโค้ดพบบั๊กแฝงที่ต้องแก้ไปพร้อมกัน: `TopicSuggestionOutboxWorker` เพิ่มแถว `TopicSuggestion` ใหม่ทุกครั้งโดยไม่ resolve แถวเก่าก่อน — เดิมไม่มีปัญหาเพราะวิเคราะห์ครั้งเดียว/Topic แต่พอเปิดให้ trigger ซ้ำได้ จะมี 2 แถว unresolved พร้อมกันแล้ว `SingleOrDefaultAsync` ที่ใช้อยู่จะ throw runtime exception

## Resolution

Build เสร็จและ verify ผ่าน browser จริงกับ backend จริงครบวงจร (2026-09-27):

**Backend**
- `TopicSuggestionOutboxEntry`: เพิ่ม `Content` (nullable) — migration `AddTopicSuggestionOutboxContent`
- `IAiProvider.AnalyzeTopicAsync`/`GeminiAiProvider`/`FakeAiProvider`: เพิ่ม parameter `newTopicContent` — prompt ของ Gemini ใช้ทั้งชื่อและเนื้อหาตัดสินใจแล้ว
- `TopicSuggestionOutboxWorker.ProcessEntryAsync`: resolve `TopicSuggestion` เก่าที่ยัง unresolved ของ Topic เดียวกันก่อนเพิ่มแถวใหม่เสมอ (แก้บั๊กแฝง)
- `ITopicSuggestionsService`/`TopicSuggestionsService`: เพิ่ม `TriggerAsync(userId, topicId, content)` — หาชื่อปัจจุบันจาก `TopicProjections` (sync ผ่าน `TopicRenamed` อยู่แล้ว จึงเป็นชื่อล่าสุดเสมอ) เขียน outbox entry ใหม่พร้อม content ที่รับมา
- `TopicSuggestionsController`: endpoint ใหม่ `POST /api/topics/{topicId}/suggestion/trigger`
- Integration test ใหม่ 3 เคส (content มีผลจริงต่อผลลัพธ์, trigger ซ้ำไม่ throw/resolve แถวเก่าถูกต้อง, trigger topic ที่ไม่มีอยู่จริง → 404)

**Frontend**
- ลบ `CreateTopicForm.tsx` (dead code หลัง flow เปลี่ยน)
- `nextUntitledTopicName.ts` ใหม่ — กันชนชื่อ "Untitled" ซ้ำ
- `TopicBrowserPage.tsx`: "+ New topic" สร้าง Topic ชื่อ Untitled แล้ว navigate เข้าหน้า Topic Detail ทันที ไม่มี popup ถามชื่ออีกต่อไป
- `TopicSuggestionPanel.tsx`: เพิ่มปุ่ม "Suggest placement" (mirror `NoteAiSuggestionPanel`'s poll pattern) เมื่อยังไม่มี suggestion active
- `TopicDetailPage.tsx`: ต่อ content ปัจจุบัน (ผ่าน ref ไม่ใช่ state กัน re-render ทั้งหน้าทุกตัวอักษร) เข้ากับปุ่ม trigger + เพิ่ม auto-trigger ตอน unmount (ออกจากหน้า) เฉพาะเมื่อมีการแก้ไขจริงในรอบนั้น

Verify ผ่าน browser จริง: กด "+ New topic" → เข้าหน้าจดทันที → พิมพ์เนื้อหา → กดปุ่ม "Suggest placement" เห็นผลจริง → ทดสอบ auto-trigger-on-leave (พิมพ์แล้วออกไม่กดปุ่ม) เห็น suggestion โผล่มาทีหลังผ่าน badge จริง `dotnet build`/`dotnet test`/`dotnet format --verify-no-changes` และ `npx tsc -b`/`npm run lint`/`npm run build` ผ่านหมด
