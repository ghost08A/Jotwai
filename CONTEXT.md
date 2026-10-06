# Jotwai Domain Glossary

คำศัพท์ที่ใช้ร่วมกันสำหรับ product domain และ architecture ใช้คำเหล่านี้เป๊ะๆ ในโค้ด, comment, commit และ spec — ห้ามคิดคำพ้องความหมายขึ้นเอง (คำศัพท์เอง — ตัวหนา — ให้คงเป็นภาษาอังกฤษเสมอ เพราะตรงกับชื่อ class/identifier ในโค้ด)

เอกสารนี้เป็น **นิยามคำศัพท์เท่านั้น** — รายละเอียดพฤติกรรม ขั้นตอน เพดานตัวเลข และกฎของแต่ละฟีเจอร์อยู่ที่ `requirements-v2.md`; flow และโครงสร้างระบบอยู่ที่ `ARCHITECTURE.md`; เหตุผลของการตัดสินใจอยู่ที่ `docs/adr/`

> นิยามเป็นของรุ่น v2 — ส่วนที่ยังไม่ได้ implement โค้ดปัจจุบันยังใช้ชื่อเดิม (Topic แยกจาก Note, Topic Suggestion, Topic Reference) เมื่อเขียนโค้ดใหม่ ให้ใช้ชื่อในเอกสารนี้

## Product domain

**Note**
หน่วยพื้นฐานของทุกอย่างที่ผู้ใช้จดไว้ (รวม Topic เดิมเข้ามาแล้ว — คำว่า "Topic" เลิกใช้) Note มีชื่อ เนื้อหาที่เป็น Markdown และอยู่ในลำดับชั้นแม่/ลูกได้ลึกไม่จำกัด (Note ที่ไม่มีแม่คือ Note ระดับบนสุด) มี **Description** และ **Category** ได้ ลบได้แบบ soft delete (เข้าถังขยะ กู้คืนได้) เนื้อหาเป็น Markdown เป็น source of truth ไม่ใช่ editor ไม่มี version history

**Note Reference**
Block พิเศษในเนื้อหาของ Note ที่ชี้ไปยัง Note อื่น (ปกติคือ Note ลูก) แสดงเป็นแถวมีไอคอนแบบ nested page ไม่ใช่ลิงก์ข้อความธรรมดา (เดิมชื่อ Topic Reference)

**Description**
ข้อความธรรมดาสั้นๆ ของ Note ที่อธิบายว่า Note นั้นเกี่ยวกับอะไร แยกจากเนื้อหา มีไว้เป็น Context ให้ AI ไม่ต้องเดาจากชื่ออย่างเดียว อยู่ภายใต้กฎ **User Ownership**

**Category**
Label แบบแบน ไม่มีลำดับชั้น Note หนึ่งมีได้หลาย Category ชื่อ Category ต้อง unique ทั้งระบบ (ไม่สนตัวพิมพ์เล็ก-ใหญ่) ผู้ใช้สร้างเองได้อย่างอิสระ หรือถูกเสนอโดย AI Categorization

**AI Categorization**
กระบวนการ background ที่วิเคราะห์ Note ที่เพิ่งสร้างใหม่และเสนอ Category ที่เป็นตัวเลือก ทำงานอัตโนมัติแค่ครั้งเดียว ตอนที่ Note ถูกบันทึกครั้งแรกพร้อมเนื้อหาจริง ผู้ใช้ trigger ซ้ำเองได้แบบ on-demand เป็นหนึ่งในความสามารถของ module **Nerd Assistant**

**Suggestion (Category Suggestion)**
รายการ Category ที่เป็นตัวเลือก จัดอันดับโดย AI Categorization สำหรับ Note หนึ่งๆ อาจรวมถึงชื่อ Category ใหม่ที่ยังไม่มีในระบบ ผู้ใช้ accept บางส่วน แก้ไข หรือเพิกเฉยก็ได้ เมื่อดำเนินการแล้วถือว่า resolve และทิ้งส่วนที่เหลือ

**Note Suggestion**
คำแนะนำที่ Nerd Assistant เสนอให้กับ Note ที่เพิ่งสร้างใหม่ (เดิมชื่อ Topic Suggestion) มีอย่างมากหนึ่งผลลัพธ์ต่อ Note เป็นได้แค่หนึ่งในสองแบบ:
- **Merge** — Note ใหม่ซ้ำกับ Note ที่มีอยู่ ถ้าผู้ใช้ยืนยัน Note ที่มีอยู่เป็นผู้รอด และ Note ใหม่ถูกรวมเข้าไปแล้วลบถาวร (กู้คืนไม่ได้)
- **Placement** — Note ใหม่ควรเป็น Note ลูกของ Note ที่มีอยู่ ถ้าผู้ใช้ยืนยัน ย้ายไปเป็นลูกทันที
ถ้าเข้าเงื่อนไข Merge ให้ Merge ชนะเสมอ resolve ครั้งเดียวจบ ไม่เสนอซ้ำอัตโนมัติ

**User Ownership**
กฎที่ว่า เมื่อ Note อยู่ที่ใดหรือมี Category/Description อะไรแล้ว (โดยผู้ใช้เอง หรือโดยการ accept AI suggestion) ระบบจะไม่ย้าย จัดหมวดหมู่ใหม่ หรือเขียนทับให้อัตโนมัติอีกเลย มีแค่ action ที่ชัดเจนของผู้ใช้เท่านั้นที่เปลี่ยนมันได้ กฎเดียวกันครอบคลุม Note Suggestion และ Description ด้วย

**Content Polish**
Action แบบ on-demand ที่ผู้ใช้กดเรียกเอง (ไม่มี auto-trigger) ให้ Nerd Assistant ช่วยจัดเนื้อหาของ Note ทั้งฉบับให้อ่านง่ายขึ้น เป็น request/response ธรรมดา ไม่มี state ค้างในระบบ หลักการ: ห้ามเปลี่ยนความหมายหรือโครงสร้างที่ผู้ใช้ตั้งใจเขียนแบบเงียบๆ ผลลัพธ์ต้องให้ผู้ใช้ยืนยันก่อน overwrite เสมอ เพราะไม่มี version history

**Search**
การจับคู่ substring แบบไม่สนตัวพิมพ์เล็ก-ใหญ่ ครอบคลุมชื่อและเนื้อหาของ Note (รวมเนื้อหาใน code block), ภาษาที่ประกาศไว้ของ code block และ Category ของมัน

**Share Link**
ลิงก์ที่เจ้าของสร้างเพื่อแชร์ Note ให้คนอื่น **ดูอย่างเดียว** ผู้รับเห็น Note ที่แชร์และทุกอย่างใต้มัน มี 2 รูปแบบ: **Public** (ใครมีลิงก์ก็เปิดได้) และ **จำกัดอีเมล** (เฉพาะบัญชีที่อีเมลตรงกับรายการ) ไม่ใช่การทำงานร่วมกัน (collaboration)

**Attachment**
รูปภาพหรือไฟล์ที่แนบในเนื้อหาของ Note เป็นของ Note ใด Note หนึ่ง อ้างในเนื้อหาด้วยรหัสอ้างอิง (key) ไม่ฝังตัวไฟล์ไว้ใน Markdown

**Error Log**
บันทึกใน Database ของ Exception ฝั่ง Backend ที่ระบบดักจับได้ ใช้ตรวจสอบปัญหาย้อนหลังโดยไม่พึ่ง Console Log

## Architecture domain

**Module**
ส่วนหนึ่งของ backend modular monolith ที่มีขอบเขตชัดเจนและคิดแยกกันได้อย่างอิสระ ดู `ARCHITECTURE.md` สำหรับแผนผัง module แต่ละ Module เป็นเจ้าของ database schema ของตัวเอง ไม่มีโค้ดของ Module อื่นสามารถ query มันได้โดยตรง

**Nerd Assistant**
ชื่อ Module ที่รวมความสามารถทุกอย่างที่เรียกใช้ AI (Gemini) ในระบบ ได้แก่ AI Categorization, Note Suggestion, Content Polish และการสร้าง Description (เดิมชื่อ "AI Categorization" แต่แคบเกินไป) การเปลี่ยนชื่อนี้เป็นแค่ระดับเอกสาร — schema ในฐานข้อมูล (`ai_categorization`) และชื่อ class ในโค้ด (เช่น `AiCategorizationDbContext`) ยังไม่เปลี่ยนตาม

**Domain Event**
การแจ้งเตือนแบบ in-process (เช่น `NoteCreated`, `NoteRenamed`, `CategoryDeleted`) ที่ publish โดย Module เจ้าของการเปลี่ยนแปลงนั้น และถูก consume โดย Module อื่นที่ต้องการตอบสนอง — เป็นวิธีที่อนุญาตให้ Module คุยกัน (ข้อยกเว้น: Module อื่นเขียนลง Module **Logging** ได้โดยตรง)

**Read Model (Projection)**
สำเนาข้อมูลแบบ denormalized ที่ sync ผ่าน event ซึ่ง Module หนึ่งต้องการใช้แต่ไม่ได้เป็นเจ้าของ — เช่น สำเนาเนื้อหาและชื่อ Note กับชื่อ Category ของ Search Module ที่อัปเดตให้ทันสมัยผ่าน Domain Events แทนที่จะ join SQL ข้าม schema

**Outbox**
ตารางแบบ durable ที่หนุนหลัง background job ของ AI: แถวหนึ่งจะถูกเขียนแบบ transactional พร้อมกับ Note ที่ต้องประมวลผล แล้ว `BackgroundService` จะ poll และประมวลผลมัน ทำให้งานที่ค้างอยู่รอดจากการ restart ของแอปได้

**Modular Monolith**
สถาปัตยกรรมเริ่มต้นที่เลือกใช้: ASP.NET Core application ตัวเดียวที่ deploy เป็นชิ้นเดียว แต่แบ่งภายในเป็น Module ที่มีขอบเขตเข้มงวด (มี schema ของตัวเอง สื่อสารกันผ่าน Domain Events เป็นหลัก) ออกแบบมาให้ Module ใดๆ สามารถถูกแยกออกเป็น service ของตัวเองในอนาคตได้โดยไม่ต้อง rewrite
