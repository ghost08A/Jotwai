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
กระบวนการ background ที่วิเคราะห์ Note ที่เพิ่งสร้างใหม่และเสนอ Category ที่เป็นตัวเลือก ทำงานแค่ครั้งเดียว ตอนที่ Note ถูกบันทึกครั้งแรกพร้อมเนื้อหาจริง จะไม่ถูก trigger ซ้ำโดยอัตโนมัติจากการแก้ไข (ดูกฎ "User Ownership" ด้านล่าง) ผู้ใช้ trigger ซ้ำเองได้ผ่าน action แบบ on-demand เป็นหนึ่งในสามความสามารถของ module **Nerd Assistant** (ดูด้านล่าง) — คู่กับ **Topic Suggestion** และ **Content Polish**

**Suggestion (Category Suggestion)**
รายการ Category ที่เป็นตัวเลือก จัดอันดับโดย AI Categorization สำหรับ Note หนึ่งๆ อาจรวมถึงชื่อ Category ใหม่ที่ยังไม่มีในระบบด้วย Note หนึ่งมี suggestion set ที่ active ได้อย่างมากหนึ่งชุดในแต่ละครั้ง ผู้ใช้ accept ได้บางส่วน, แก้ไข หรือเพิกเฉยก็ได้; เมื่อดำเนินการกับชุดนั้นแล้วถือว่า resolve และทิ้งส่วนที่เหลือ

**Topic Suggestion**
คำแนะนำที่ Nerd Assistant เสนอให้กับ Topic ที่เพิ่งสร้างใหม่ วิเคราะห์แบบ background (ไม่บล็อก UI) ทันทีที่ Topic ถูกสร้าง โดยเทียบชื่อ Topic ใหม่กับชื่อ Topic ที่มีอยู่แล้วทั้งหมดของ user (ทุก branch/ทุกระดับชั้น) มีอย่างมากหนึ่งผลลัพธ์ต่อ Topic ในแต่ละครั้ง เป็นได้แค่หนึ่งในสองแบบ (ไม่ใช่ทั้งคู่พร้อมกัน — ถ้าเข้าเงื่อนไข Merge ให้ Merge ชนะเสมอ ไม่เสนอ Placement ซ้อน):
- **Merge** — Topic ใหม่นี้ซ้ำกับ Topic ที่มีอยู่แล้ว ถ้าผู้ใช้ยืนยัน (ต้องผ่าน dialog ยืนยันซ้ำเพราะเป็น action ที่ทำลายข้อมูล กู้คืนไม่ได้): Topic เดิมเป็นผู้รอดเสมอ, Note ทั้งหมดที่อยู่ใต้ Topic ใหม่ย้ายไปอยู่ใต้ Topic เดิม, subtopic ลูกของ Topic ใหม่ (ถ้ามี) ย้ายไปเป็นลูกของ Topic เดิมด้วย แล้ว Topic ใหม่ถูกลบทิ้ง
- **Placement** — Topic ใหม่นี้ควรเป็น subtopic ของ Topic ที่มีอยู่ ถ้าผู้ใช้ยืนยัน Topic ใหม่ย้ายไปเป็นลูกของ Topic ที่แนะนำทันที (ไม่ต้องยืนยันซ้ำ เพราะไม่มีข้อมูลสูญหาย)

ต่างจาก Suggestion (Category Suggestion) ตรงที่เสนอแค่ตัวเลือกเดียวเสมอ (ไม่ใช่ ranked list — Topic มีแม่ได้แค่หนึ่งเดียว) และมี confidence threshold ของตัวเองแยกจาก Category Suggestion (เกณฑ์สูงกว่า เพราะ Merge เป็น action ที่ทำลายข้อมูลจริง) แต่เหมือนกันตรงที่ resolve ครั้งเดียวจบ ตอบแล้ว (accept/decline) ไม่เสนอซ้ำอัตโนมัติอีก แม้จะ rename Topic ทีหลัง (ตามกฎ "User Ownership" ด้านล่าง)

**User Ownership**
กฎที่ว่า เมื่อ Topic/Category ของ Note ถูกกำหนดแล้ว (โดยผู้ใช้เอง หรือโดยการ accept AI suggestion) ระบบจะไม่ย้ายหรือจัดหมวดหมู่ใหม่ให้อัตโนมัติอีกเลย มีแค่ action ของผู้ใช้ที่ชัดเจนเท่านั้นที่เปลี่ยนมันได้ กฎเดียวกันนี้ครอบคลุมถึง Topic เองด้วย (ผ่าน Topic Suggestion ด้านบน)

**Content Polish**
Action แบบ on-demand ที่ผู้ใช้กดปุ่มเรียกเอง (ไม่มี auto-trigger) ให้ Nerd Assistant ช่วยจัดเนื้อหาของ Note ทั้งฉบับ (รวม heading แรกสุดที่ทำหน้าที่ Title ด้วย) ให้อ่านง่ายขึ้น — แก้คำผิด, จัดย่อหน้ายาวเป็นข้อย่อย, ขยายคำอธิบาย เป็น request/response ธรรมดา ไม่มี state ค้างอยู่ในระบบเลย (ต่างจาก Suggestion ที่ต้องรอ resolve) ไม่แตะเนื้อหาใน code block เด็ดขาด

กฎการแก้ไขที่ต้องยึดตาม: แก้ตัวสะกดผิด/ไวยากรณ์/จัดรูปแบบใหม่ได้ตรงๆ โดยไม่ต้องเก็บของเดิมไว้ (ไม่ได้เปลี่ยนความหมายที่ผู้ใช้ตั้งใจสื่อ แค่เปลี่ยนรูปแบบการเขียน) แต่ถ้าเป็นการเปลี่ยนหรือเติมเนื้อหาที่อาจกระทบความหมายเดิม (ตัวเลข, ข้อเท็จจริง, ประโยคใหม่ที่ AI เดาเติมเข้ามาเองที่ไม่มีในต้นฉบับ) ต้องเก็บข้อความเดิมกำกับไว้ในวงเล็บให้ผู้ใช้เห็นทั้งคู่เสมอ ห้ามเปลี่ยนความหมายแบบเงียบๆ เด็ดขาด ผลลัพธ์ที่ได้ต้องโชว์เป็น preview ให้ผู้ใช้กด "ยืนยัน" (overwrite เนื้อหาจริงทั้งฉบับทีเดียว) หรือ "ยกเลิก" (ไม่มีอะไรเปลี่ยน) ก่อนเสมอ เพราะ Note ไม่มี version history กู้คืนไม่ได้ถ้า apply ไปแล้ว

**Search**
การจับคู่ substring แบบไม่สนตัวพิมพ์เล็ก-ใหญ่ ครอบคลุมเนื้อหาของ Note (รวมถึงเนื้อหาใน code block), ภาษาที่ประกาศไว้ของ code block, Topic และ Category ของมัน

## Architecture domain

**Module**
ส่วนหนึ่งของ backend modular monolith ที่มีขอบเขตชัดเจนและคิดแยกกันได้อย่างอิสระ ดู `ARCHITECTURE.md` สำหรับแผนผัง module แต่ละ Module เป็นเจ้าของ database schema ของตัวเอง ไม่มีโค้ดของ Module อื่นสามารถ query มันได้โดยตรง

**Nerd Assistant**
ชื่อ Module ที่รวมความสามารถทุกอย่างที่เรียกใช้ AI (Gemini) ในระบบ — เดิมชื่อ "AI Categorization" แต่ชื่อนั้นแคบเกินไปแล้วเพราะตอนนี้ทำ 3 เรื่อง: AI Categorization (เดิม), Topic Suggestion และ Content Polish (ทั้งสองใหม่ ดูด้านบน) การเปลี่ยนชื่อนี้เป็นแค่ระดับเอกสาร (ชื่อที่คนอ่านเห็น) — schema ในฐานข้อมูล (`ai_categorization`) และชื่อ class ในโค้ด (เช่น `AiCategorizationDbContext`) ยังไม่เปลี่ยนตาม จะทำเป็น ticket แยกทีหลัง

**Domain Event**
การแจ้งเตือนแบบ in-process (เช่น `NoteCreated`, `TopicRenamed`, `CategoryDeleted`) ที่ publish โดย Module เจ้าของการเปลี่ยนแปลงนั้น และถูก consume โดย Module อื่นที่ต้องการตอบสนอง — เป็นวิธีเดียวที่อนุญาตให้ Module คุยกัน

**Read Model (Projection)**
สำเนาข้อมูลแบบ denormalized ที่ sync ผ่าน event ซึ่ง Module หนึ่งต้องการใช้แต่ไม่ได้เป็นเจ้าของ — เช่น สำเนาเนื้อหา Note และชื่อ Topic/Category ของ Search Module ที่อัปเดตให้ทันสมัยผ่าน Domain Events แทนที่จะ join SQL ข้าม schema

**Outbox**
ตารางแบบ durable ที่หนุนหลัง background job ของ AI Categorization: แถวหนึ่งจะถูกเขียนแบบ transactional พร้อมกับ Note ที่ต้องประมวลผล แล้ว `BackgroundService` จะ poll และประมวลผลมัน ทำให้งานที่ค้างอยู่รอดจากการ restart ของแอปได้

**Modular Monolith**
สถาปัตยกรรมเริ่มต้นที่เลือกใช้: ASP.NET Core application ตัวเดียวที่ deploy เป็นชิ้นเดียว แต่แบ่งภายในเป็น Module ที่มีขอบเขตเข้มงวด (มี schema ของตัวเอง, สื่อสารกันผ่าน Domain Events เท่านั้น) ออกแบบมาให้ Module ใดๆ สามารถถูกแยกออกเป็น service ของตัวเองในอนาคตได้โดยไม่ต้อง rewrite
