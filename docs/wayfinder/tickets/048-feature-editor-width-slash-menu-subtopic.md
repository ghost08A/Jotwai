---
id: 48
title: "ขยายหน้าเขียน ~944px, เมนู / พลิกขึ้นบนเมื่ออยู่ต่ำ, /subtopic แบบ Notion"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ผู้ใช้ขอ (2026-09-30) หลังรีวิว ticket 046: (1) หน้าเขียนกว้างกว่า 768px เป็นราว 900 กว่าๆ ถ้าจอเล็กกว่าให้พอดีจอ (2) เมนู `/` หายลงล่างจอเมื่อเคอร์เซอร์อยู่ต่ำ — ให้ย้ายขึ้นบนได้ (3) `/subtopic` ไม่สวยแบบ Notion

## Resolution

Build เสร็จและ verify ผ่าน browser จริง (2026-09-30) ฝั่ง client:

- `TopicDetailPage.tsx`: `max-w-3xl` → `max-w-[59rem]` (944px; จอแคบกว่านั้นยังเต็มความกว้างเพราะ `w-full`)
- `slashCommand.ts` + `SlashCommandMenu.tsx`: เมนูพลิกขึ้นเหนือเคอร์เซอร์เมื่อที่ว่างด้านล่าง < 300px และด้านบนมีมากกว่า, กันเมนูล้นขอบขวา, จำกัดความสูง `max-h-72` พร้อม scroll และเลื่อนรายการที่เลือกด้วยลูกศรให้เห็นเสมอ
- `/subtopic`: **เปลี่ยนพฤติกรรม** จาก dialog ถามชื่อ เป็นสร้าง subtopic ชื่อ "Untitled" (กันชื่อซ้ำด้วย `nextUntitledTopicName`) แล้วแทรกลิงก์ทันทีแบบ Notion `/page` — ตั้งชื่อทีหลังโดยเปิดเข้าไป (สอดคล้อง "จดก่อน จัดทีหลัง", ticket 044); ลบ `CreateSubtopicDialog.tsx` (dead code); ล็อก editor ระหว่างรอ API กัน insert ตำแหน่งเลื่อน
- `TopicReferenceView.tsx`: จากการ์ดมีกรอบ เป็นแถวเรียบ (ไอคอน + ชื่อขีดเส้นใต้จาง, hover เป็นพื้น) แบบลิงก์หน้าใน Notion

Verify: `npx tsc -b`/`npm run lint`/`npm run build` ผ่าน; browser จริง (จอสูง 500px) — main กว้าง 944px, เมนูพลิกขึ้นและอยู่ใน viewport ครบ, เลือก Subtopic แล้วได้แถว "📄 Untitled" โดยไม่มี dialog และ editor กลับมาแก้ไขได้ ยังไม่ได้ดูสายตาผ่าน screenshot (ใช้ไม่ได้ใน pane นี้), ไม่ได้เช็ก light/mobile และไม่ได้ทดสอบ reload เพื่อยืนยัน autosave ของ reference
