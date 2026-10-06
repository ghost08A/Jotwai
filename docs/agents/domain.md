# Domain Docs

วิธีที่ engineering skill ควรใช้เอกสาร domain ของ repo นี้เมื่อสำรวจโค้ดเบส

## ก่อนสำรวจ ให้อ่านสิ่งเหล่านี้ก่อน

- **`ARCHITECTURE.md`** ที่ root ของ repo — แผนผัง module, กลยุทธ์ข้อมูล, flow ของ AI categorization, frontend stack, deployment
- **`CONTEXT.md`** ที่ root ของ repo — คำศัพท์เฉพาะของ domain (Note, Topic, Category, Suggestion, Module, Read Model, ...) ใช้คำเหล่านี้ตามที่กำหนด ห้ามคิดคำพ้องความหมายขึ้นเอง
- **`docs/adr/`** — อ่าน ADR ที่เกี่ยวข้องกับส่วนที่คุณกำลังจะแก้ไข

Repo นี้เป็นแบบ single-context: มี ARCHITECTURE.md/CONTEXT.md คู่เดียวและ docs/adr/ ชุดเดียวที่ root (ไม่มีการแบ่ง CONTEXT-MAP.md / per-context แยก)

## ใช้คำศัพท์จาก glossary

เมื่อผลงานของคุณอ้างถึง domain concept (ใน issue title, ข้อเสนอ refactor, สมมติฐาน, ชื่อ test) ให้ใช้คำตามที่นิยามไว้ใน CONTEXT.md อย่าเบี่ยงไปใช้คำพ้องความหมายที่ glossary จงใจหลีกเลี่ยง

ถ้า concept ที่คุณต้องการยังไม่มีใน glossary นั่นคือสัญญาณ: อาจเป็นเพราะคุณกำลังคิดคำที่ project ไม่ได้ใช้ขึ้นเอง (ควรทบทวนใหม่) หรือมี gap จริงที่ควรหยิบยกขึ้นมา

## แจ้งเมื่อขัดแย้งกับ ADR

ถ้าผลงานของคุณขัดแย้งกับ ADR ที่มีอยู่แล้ว ให้แจ้งอย่างชัดเจนแทนที่จะ override เงียบๆ:

> _ขัดแย้งกับ ADR-0002 (in-process event communication) แต่ควรค่าแก่การหยิบยกมาทบทวนใหม่เพราะ…_
