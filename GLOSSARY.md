# Jotwai Domain Glossary

คำศัพท์ที่ใช้ร่วมกันสำหรับ product domain และ architecture ใช้คำเหล่านี้เป๊ะๆ ในโค้ด, comment, commit และ spec — ห้ามคิดคำพ้องความหมายขึ้นเอง (คำศัพท์เอง — ตัวหนา — ให้คงเป็นภาษาอังกฤษเสมอ เพราะตรงกับชื่อ class/identifier ในโค้ด)

## Product domain

**Note**
หน่วยพื้นฐานของการจดบันทึก เนื้อหาอิสระ (plain text, heading ระดับ 1-3, blockquote, bulleted list, numbered list, code block, horizontal rule, พร้อม inline mark ตัวหนา/เอียง/ขีดทับ/inline code) เก็บเป็น Markdown สามารถมีอยู่ได้โดยไม่มี Topic และไม่มี Category Soft-deletable ไม่มี version history — การแก้ไขจะ overwrite ทับของเดิม ไม่มี Title แยกเป็น field ของตัวเอง (ตั๋ว 030) แต่ block แรกสุดของเนื้อหาถูกบังคับให้เป็น heading ระดับ 1 เสมอ (ตั๋ว 037) ทำหน้าที่เป็น "หัวเรื่อง" ทางสายตาโดยไม่ต้องมี field แยก

**Topic**
การจัดหมวดหมู่แบบลำดับชั้นที่ Note สามารถเลือกอยู่ในนั้นได้ Note หนึ่งอยู่ได้อย่างมากหนึ่ง Topic Topic มี Topic ลูกได้ (ลึกไม่จำกัด) ชื่อ Topic พี่น้องกันต้อง unique (ไม่สนตัวพิมพ์เล็ก-ใหญ่) ภายใต้ parent เดียวกัน Topic ที่มี Note หรือ Topic ลูกอยู่ภายในโดยตรงจะลบไม่ได้จนกว่าจะว่างก่อน

**Category**
Label แบบแบน ไม่มีลำดับชั้น Note หนึ่งมีได้หลาย Category ชื่อ Category ต้อง unique ทั้งระบบ (ไม่สนตัวพิมพ์เล็ก-ใหญ่) ผู้ใช้สร้างเองได้อย่างอิสระ หรือถูกเสนอโดย AI Categorization

**AI Categorization**
กระบวนการ background ที่วิเคราะห์ Note ที่เพิ่งสร้างใหม่และเสนอ Category ที่เป็นตัวเลือก ทำงานแค่ครั้งเดียว ตอนที่ Note ถูกบันทึกครั้งแรกพร้อมเนื้อหาจริง จะไม่ถูก trigger ซ้ำโดยอัตโนมัติจากการแก้ไข (ดูกฎ "User Ownership" ด้านล่าง) ผู้ใช้ trigger ซ้ำเองได้ผ่าน action แบบ on-demand

**Suggestion (Category Suggestion)**
รายการ Category ที่เป็นตัวเลือก จัดอันดับโดย AI Categorization สำหรับ Note หนึ่งๆ อาจรวมถึงชื่อ Category ใหม่ที่ยังไม่มีในระบบด้วย Note หนึ่งมี suggestion set ที่ active ได้อย่างมากหนึ่งชุดในแต่ละครั้ง ผู้ใช้ accept ได้บางส่วน, แก้ไข หรือเพิกเฉยก็ได้; เมื่อดำเนินการกับชุดนั้นแล้วถือว่า resolve และทิ้งส่วนที่เหลือ

**User Ownership**
กฎที่ว่า เมื่อ Topic/Category ของ Note ถูกกำหนดแล้ว (โดยผู้ใช้เอง หรือโดยการ accept AI suggestion) ระบบจะไม่ย้ายหรือจัดหมวดหมู่ใหม่ให้อัตโนมัติอีกเลย มีแค่ action ของผู้ใช้ที่ชัดเจนเท่านั้นที่เปลี่ยนมันได้

**Search**
การจับคู่ substring แบบไม่สนตัวพิมพ์เล็ก-ใหญ่ ครอบคลุมเนื้อหาของ Note (รวมถึงเนื้อหาใน code block), ภาษาที่ประกาศไว้ของ code block, Topic และ Category ของมัน

## Architecture domain

**Module**
ส่วนหนึ่งของ backend modular monolith ที่มีขอบเขตชัดเจนและคิดแยกกันได้อย่างอิสระ ดู `ARCHITECTURE.md` สำหรับแผนผัง module แต่ละ Module เป็นเจ้าของ database schema ของตัวเอง ไม่มีโค้ดของ Module อื่นสามารถ query มันได้โดยตรง

**Domain Event**
การแจ้งเตือนแบบ in-process (เช่น `NoteCreated`, `TopicRenamed`, `CategoryDeleted`) ที่ publish โดย Module เจ้าของการเปลี่ยนแปลงนั้น และถูก consume โดย Module อื่นที่ต้องการตอบสนอง — เป็นวิธีเดียวที่อนุญาตให้ Module คุยกัน

**Read Model (Projection)**
สำเนาข้อมูลแบบ denormalized ที่ sync ผ่าน event ซึ่ง Module หนึ่งต้องการใช้แต่ไม่ได้เป็นเจ้าของ — เช่น สำเนาเนื้อหา Note และชื่อ Topic/Category ของ Search Module ที่อัปเดตให้ทันสมัยผ่าน Domain Events แทนที่จะ join SQL ข้าม schema

**Outbox**
ตารางแบบ durable ที่หนุนหลัง background job ของ AI Categorization: แถวหนึ่งจะถูกเขียนแบบ transactional พร้อมกับ Note ที่ต้องประมวลผล แล้ว `BackgroundService` จะ poll และประมวลผลมัน ทำให้งานที่ค้างอยู่รอดจากการ restart ของแอปได้

**Modular Monolith**
สถาปัตยกรรมเริ่มต้นที่เลือกใช้: ASP.NET Core application ตัวเดียวที่ deploy เป็นชิ้นเดียว แต่แบ่งภายในเป็น Module ที่มีขอบเขตเข้มงวด (มี schema ของตัวเอง, สื่อสารกันผ่าน Domain Events เท่านั้น) ออกแบบมาให้ Module ใดๆ สามารถถูกแยกออกเป็น service ของตัวเองในอนาคตได้โดยไม่ต้อง rewrite
