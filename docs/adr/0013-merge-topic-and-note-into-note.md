# ADR 0013: รวม Topic และ Note เป็น Entity เดียวชื่อ Note (Module Notes เป็นเจ้าของทั้งก้อน)

## สถานะ
ยอมรับแล้ว (ตัดสินใจใน wayfinder map-v2, ticket 049/053/060 — ยังไม่ได้ implement)

## บริบท
ปัจจุบัน Topic อยู่ใน Module Organization (schema `organization`) และ Note อยู่ใน Module Notes (schema `notes`) ความสัมพันธ์เป็น Topic 1 : 1 Note ซึ่ง UX ปัจจุบันมองเป็นหน้าเดียวกันอยู่แล้ว (หน้า Topic Detail พิมพ์ต่อเนื่องลง Note ตัวเดียวของ Topic) การแชร์ระดับ subtree ของ requirements v2 ก็สอดคล้องกับ "หน้าที่เป็นทั้งเนื้อหาและโครงสร้าง" มากกว่าสอง Entity ที่ต้องส่ง event หากันตลอด

## การตัดสินใจ
- Topic และ Note เป็น **Entity เดียวกัน เรียกว่า Note** คำว่า "Topic" เลิกใช้ทุกที่ (UI/โค้ด/เอกสาร)
- Note เก็บ: **ชื่อ (ช่องแยกจากเนื้อหา)**, เนื้อหา Markdown, Note แม่ (ไม่มีแม่ = Note ระดับบนสุด) และมี Note ลูกได้ลึกไม่จำกัด ไม่มีแนวคิด "Note ที่ยังไม่ได้จัดเข้า Topic" แยกอีก
- **Module Notes เป็นเจ้าของ Note ทั้งก้อน** (ชื่อ เนื้อหา ลำดับชั้น ถังขยะ) Module Organization เหลือ **Category อย่างเดียว** ข่าว/read model ระหว่าง Notes ↔ Organization ที่เคยมีเพราะ Topic กับ Note แยกกัน (เช่น `KnownTopic`) หายไป
- ชื่อพี่น้องใต้แม่เดียวกัน **ซ้ำได้พร้อมคำเตือน** (แทนกฎห้ามซ้ำแบบ unique เดิม) ลบ Note ที่มีลูก → ลูกทุกระดับเข้าถังขยะทั้งต้น กู้แม่ = ลูกกลับครบ กู้ลูกขณะแม่ยังอยู่ในถังขยะไม่ได้
- ข้อมูลเดิมลบทิ้งได้ ไม่ migrate
- รายละเอียดของ Merge (รวม Note ซ้ำที่ AI เสนอ) อยู่ใน requirements-v2.md §2.1 (นิยามคำอยู่ใน CONTEXT.md หัวข้อ Note Suggestion) และ wayfinder ticket 060

## ผลที่ตามมา
- ตาราง Module ใน ARCHITECTURE.md เปลี่ยน: Notes รับ Note+ลำดับชั้น, Organization เหลือ Category ADR-0001 ถึง 0003 ยังใช้ได้ (รูปแบบ schema-per-module, event, read model) แต่รายชื่อ event/read model ที่อ้างถึง Topic ต้องอ่านตามนิยามใหม่นี้
- ขั้นตอน Merge ที่เคยเป็นธุรกรรมข้าม 3 schema (Organization/Notes/Nerd Assistant) เหลือน้อยลง เพราะ Note กับลำดับชั้นอยู่ schema เดียวกันแล้ว
- ชื่อฟีเจอร์ที่เคยมีคำว่า Topic ตั้งชื่อใหม่แล้ว (ดู CONTEXT.md: Note Suggestion, Note Reference)
