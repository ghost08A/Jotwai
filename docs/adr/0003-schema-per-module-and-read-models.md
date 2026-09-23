# ADR 0003: PostgreSQL, schema-per-module และ read model ที่ซิงค์ผ่าน event สำหรับการอ่านข้ามโมดูล

## สถานะ
ยอมรับแล้ว

## บริบท
Module ต่าง ๆ ต้องเก็บข้อมูลของตนเอง และบาง Module ก็ต้องการข้อมูลที่เป็นของ Module อื่น: Search ต้องการเนื้อหาของ Note รวมถึงชื่อ Topic/Category; AI Categorization ต้องการรายการ Category ปัจจุบัน การใช้ฐานข้อมูลเดียวร่วมกันจริง ๆ จะทำให้การเขียน SQL join ข้าม schema เป็นเรื่องง่าย — แต่การทำเช่นนั้นจะลบขอบเขตของ Module ที่ ADR 0001 และ 0002 วางไว้ไปอย่างเงียบ ๆ และทำให้การแยก Module ในอนาคตยากขึ้นมาก

## การตัดสินใจ
- ใช้ฐานข้อมูล **PostgreSQL** เดียว (ฟรี, ข้ามแพลตฟอร์ม, รองรับ EF Core อย่างเต็มที่, ทำงานได้ดีใน Docker สำหรับการพัฒนาในเครื่อง)
- ให้แต่ละ Module มี **schema** ของตัวเอง (`notes`, `organization`, `ai_categorization`, `search`, `identity`) เข้าถึงได้ผ่าน EF Core `DbContext` ของ Module นั้นเท่านั้น (เช่น `IdentityDbContext`) `DbContext` ต่อ Module เหล่านี้ทั้งหมดอยู่ใน project `Jotwai.Service` เดียว (ADR-0010) ไม่ได้แยกเป็น project ต่อ Module — การแบ่งแยก schema/`DbContext` นี้ถูกรักษาไว้โดยตั้งใจให้เป็นขอบเขตที่แข็งแรงเพียงหนึ่งเดียว แม้หลังจากยกเลิกการแบ่งแยกในระดับ project แล้ว
- Module ใดที่ต้องการข้อมูลของ Module อื่น จะดูแล **read model แบบ denormalized ของตัวเอง** ซึ่งอัปเดตให้ทันสมัยอยู่เสมอโดยการ subscribe Domain Events ของ Module นั้น (ADR 0002) — เช่น read model ของ Search จะถูกอัปเดตเมื่อมี `NoteCreated`/`NoteUpdated`/`TopicRenamed`/`CategoryRenamed` ไม่มี Module ใดทำ SQL join ข้าม schema ของ Module อื่น

## ผลที่ตามมา
- Search และ AI Categorization มีข้อมูลซ้ำซ้อนอยู่บ้าง (สำเนาข้อมูล Note/Topic/Category ของตัวเอง) และมี eventual consistency (ความล่าช้าเล็กน้อยระหว่างการเขียนข้อมูลกับการที่ read model สะท้อนข้อมูลนั้น) — ยอมรับได้สำหรับโดเมนนี้ เนื่องจากผลการค้นหาที่เก่าไปเล็กน้อยไม่ใช่ปัญหาด้านความถูกต้อง
- หาก Module ถูกแยกออกเป็นเซอร์วิสของตัวเองในอนาคต รูปแบบ read-model-via-events ของ Module นั้นจะไม่เปลี่ยนแปลง — เปลี่ยนแค่ transport (in-process → message broker) เท่านั้น
- schema-per-module ถูกบังคับใช้ด้วยแนวปฏิบัติ/การตั้งค่า DbContext ไม่ใช่ขอบเขตระดับฐานข้อมูลที่แข็งแกร่ง (schema ทั้งหมดยังอยู่ในฐานข้อมูลจริงเดียวกัน) — code review คือกลไกบังคับใช้ที่แท้จริง
