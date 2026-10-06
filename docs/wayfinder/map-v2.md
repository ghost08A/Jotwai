---
title: Jotwai Requirements v2 — แผนผังการตัดสินใจ
label: wayfinder:map
---

## Destination

Requirements v2 ฉบับ final: ทุกข้อที่ `requirements-v2.md` (Working Notes) ระบุว่า "ยังไม่ตัดสิน/ต้อง Grill ต่อ" ถูกตัดสินแล้วโดยเจ้าของโปรเจกต์ พร้อมส่งต่อให้ขั้นวิเคราะห์ Architecture และวาง Implementation Plan — **เป็นการตัดสินใจ ไม่ใช่การ implement**

## Notes

- ต้นทาง: `requirements-v2.md` ที่ root เป็น Working Notes **ไม่ใช่ implementation specification** ห้าม assume ข้อที่ยังไม่ตัดสิน ข้อที่เจ้าของโปรเจกต์ยังไม่ได้บอก ต้องถามเสมอ
- เอกสาร domain ที่ต้อง consult: ARCHITECTURE.md, GLOSSARY.md, `docs/adr/`
- ทุก ticket ที่ไม่ใช่ research เป็น HITL (grilling) — เจ้าของโปรเจกต์ตอบเอง ห้ามตอบแทน
- Research ticket (AFK) ให้ผลเป็น **ข้อเท็จจริงล้วน** ไม่ใช่ข้อเสนอที่ถูกเลือก เก็บเป็นไฟล์ `docs/wayfinder/research/<name>.md` (ยังไม่ commit — กฎ repo ห้าม commit เองโดยไม่ได้รับอนุญาต จึงไม่สร้าง throwaway branch ตามที่ skill แนะนำ)
- **ยกเลิก non-goal เดิม (ยืนยัน 2026-10-05)**: v2 ยกเลิกข้อห้าม "การแชร์" (เฉพาะ read-only link ไม่ใช่ collaboration) และ "การแนบไฟล์/รูปภาพ" ใน ARCHITECTURE.md/CLAUDE.md ส่วน real-time push, semantic search, version history **ยังเป็น non-goal** (ถ้า Concurrent Editing เฉียดเข้าใกล้ version history ให้ถามเจ้าของโปรเจกต์)
- ยังไม่แก้ ARCHITECTURE.md/CLAUDE.md/GLOSSARY.md ในแผนผังนี้ (ดู Not yet specified)
- **Tracker**: local-markdown (`docs/wayfinder/`) แยกจาก `map.md` ของ v0.1; เลข ticket ต่อเนื่องจากเดิม (เริ่ม 049)

## Decisions so far

- [Topic และ Note เป็นสิ่งเดียวกัน](tickets/049-topic-note-merge.md): เจ้าของโปรเจกต์ตัดสินแล้วว่ารวมเป็น Entity เดียว (ผลกระทบต่อของเดิมยังไม่ตัดสิน — ดู [ผลกระทบของการรวม](tickets/053-topic-note-merge-consequences.md))
- [ผลกระทบของการรวม Topic+Note](tickets/053-topic-note-merge-consequences.md): ตัดสินครบแล้ว — Entity ชื่อ **Note** (เลิกคำว่า Topic ทุกที่), ชื่อเป็นช่องแยก ซ้ำพี่น้องได้พร้อมคำเตือน, ลบ/กู้คืนทั้งต้น, Category/Search/AI trigger เหมือนเดิม, Merge ให้ผู้ใช้เลือกเนื้อหาตอนยืนยัน, Module Notes เป็นเจ้าของทั้งก้อน (Organization เหลือ Category), ลบข้อมูลเดิมได้ ดูรายการศัพท์ GLOSSARY ที่ต้องเปลี่ยนใน ticket
- [AI Content Formatting](tickets/054-ai-content-formatting.md): ตัดสินครบแล้ว — AI ห้ามเปลี่ยนชนิดโครงสร้างที่ผู้ใช้เขียน (รายการเลข/จุด/checkbox/หัวข้อ/quote), ในรายการรวม/แยก/สลับ/เพิ่ม/ลบข้อซ้ำเป๊ะได้ (checkbox ห้าม), เพิ่มเส้นคั่น+ตัวหนาได้, ดักด้วยคำเตือนใน preview เทียบเฉพาะชนิดโครงสร้าง, กฎ Polish เดิมที่ไม่ใช่โครงสร้างคงไว้ทั้งหมด
- [Error Logging](tickets/055-error-logging-details.md): ตัดสินครบแล้ว — เก็บ Error ของ API (500) / งานเบื้องหลัง / การเรียก AI ที่ล้ม ไม่เก็บ 4xx และ Error ฝั่งหน้าเว็บ, ห้ามเก็บเนื้อหา Note/body/อีเมล/query string, ลบเมื่อเกิน 6 เดือน (รันเดือนละครั้ง), ดูผ่านฐานข้อมูลโดยตรง, Module ใหม่ Logging, เก็บ Error ซ้ำทุกครั้ง, ถ้าเขียน DB ไม่ได้ให้ลง console
- [Concurrent Editing](tickets/056-concurrent-editing.md): ตัดสินครบแล้ว — ตรวจการชนตอนบันทึกเนื้อหา แท็บเก่าถูกหยุด autosave แล้วให้ผู้ใช้เลือก (โหลดล่าสุด / เขียนทับ / คัดลอกเก็บก่อน) ไม่รวมข้อความอัตโนมัติ ไม่ใช้ Yjs, ป้องกันเฉพาะเนื้อหา, Note ถูกลบมีทางกู้, แท็บกลับมาโฟกัสโหลดใหม่เองถ้าไม่มีข้อความค้าง, Polish/Merge ยืนยันต้องตรวจเนื้อหาไม่เปลี่ยน, ไม่มีประวัติเวอร์ชันฝั่งเซิร์ฟเวอร์
- [Description ของ Note](tickets/057-topic-description.md): ตัดสินครบแล้ว — ข้อความธรรมดาช่องเดียวแยกจากเนื้อหา ไม่เกิน 200 ตัวอักษร, AI สร้างครั้งเดียวตอนบันทึกครั้งแรกที่มีเนื้อหา แล้วสร้างใหม่ได้เมื่อผู้ใช้กดปุ่ม, ที่ผู้ใช้เขียนเองห้ามระบบทับ (ลบจนว่าง = คืนสิทธิ์ให้ AI), ใช้กับ Topic Suggestion (ส่งชื่อ+Description ของ Note เดิมทุกตัว) และ AI Categorization แต่ไม่ใช้กับ Content Polish, ไม่ค้นหาจาก Description
- [Note Sharing](tickets/058-topic-sharing-details.md): ตัดสินครบแล้ว — Public ไม่ต้องมีบัญชี, แบบจำกัดอีเมลต้องล็อกอินด้วยบัญชีที่อีเมลตรง + เพิ่มการยืนยันอีเมลตอนสมัคร (ticket 061), หลายลิงก์ต่อ Note, แก้/ปิดลิงก์ได้ วันหมดอายุเป็นตัวเลือก, ผู้รับเห็นทุกอย่างใต้ Note แม่ ณ ตอนเปิดดู (ชื่อ เนื้อหา ต้นไม้ Category Description วันที่) ไม่เห็นการค้นหา/ข้อมูลเจ้าของ/เส้นทางเหนือแม่, หมดอายุบอกแยก ส่วนอื่นข้อความเดียวกัน, ไม่ติดตามผู้เปิดดู
- [Image/File Attachments](tickets/059-attachments-architecture.md): ตัดสินครบแล้ว — allowlist (รูป+pdf/txt/md/csv/Office) ไม่สแกนไวรัส, 10 MB/ไฟล์ 500 MB/ผู้ใช้, เก็บที่ S3-compatible แต่ API เป็นคนส่งไฟล์ทุกครั้ง (ปิดลิงก์แชร์แล้วหยุดทันที), คัดลอกข้ามหน้าระบบคัดลอกไฟล์ให้, เก็บกวาดไฟล์ที่ไม่ถูกอ้างถึงเกิน 7 วัน ไม่มีลบ Note ถาวร, วางรูป/ลากไฟล์/พิมพ์ / เพิ่มไฟล์, ข้อมูลไฟล์อยู่ Module Notes, อ้างด้วย attachment:KEY ใน Markdown
- [รายละเอียดการรวม Note (Merge)](tickets/060-merge-details-after-note-merge.md): ตัดสินครบแล้ว — ผู้รอดคือ A, Category ของ B ย้ายมา A, Description ใช้ของ A (ถ้าไม่มีใช้ของ B), ไฟล์แนบย้ายเจ้าของมา A, ลิงก์แชร์ของ B ถูกยกเลิก, การ์ดลิงก์ที่ชี้ B ชี้ไป A อัตโนมัติ (ห้ามแก้เนื้อหา Note อื่นเงียบๆ), B ถูกลบถาวร, dialog ยืนยันมี preview ผลลัพธ์เต็ม ตัวเลือกตั้งต้นต่อ B ท้าย A, AI ตัดสินเสนอรวมเหมือนเดิม ไม่นับ Note ในถังขยะ ไม่เสนอเมื่อ A เป็นลูกหลานของ B
- [การยืนยันอีเมลตอนสมัคร](tickets/061-email-verification-at-registration.md): ตัดสินแล้ว — สมัครแล้วยังไม่ล็อกอิน ต้องกดลิงก์ยืนยันก่อนใช้แอป, ลิงก์หมดอายุ 24 ชม., ส่งซ้ำได้ (พัก 60 วินาที ไม่เกิน 5 ครั้ง/ชม./บัญชี) และจำกัดต่อ IP, ลบบัญชีเดิม, ไม่เพิ่มการเปลี่ยนอีเมล, ใช้บริการส่งอีเมลภายนอก, ส่งไม่สำเร็จสมัครยังสำเร็จ+ส่งซ้ำได้+บันทึก Error Log, dev ใช้ตัวรับอีเมลจำลองใน docker-compose; **ยังไม่มีโดเมนสำหรับที่อยู่ผู้ส่ง ต้องจัดหาก่อนส่งอีเมลจริง**
- [Research: ตัวเลือกการส่งอีเมลยืนยัน](tickets/062-research-email-sending-options.md): รวบรวมข้อเท็จจริงใน `research/email-sending-options.md` (token ยืนยัน, SMTP/MailKit, โควตาของบริการส่งเมล, เครื่องมือ dev) ไม่ได้เลือกวิธีใด
- [Research: แนวทางป้องกัน stale write](tickets/050-research-concurrency-patterns.md): รวบรวมตัวเลือก (EF concurrency token, ETag/If-Match, merge/CRDT) เป็นข้อเท็จจริงล้วนใน `research/concurrency-patterns.md` ไม่ได้เลือกวิธีใด
- [Research: วิธีกัน LLM เปลี่ยนโครงสร้าง Markdown](tickets/052-research-ai-markdown-preservation.md): รวบรวมข้อเท็จจริงใน `research/ai-markdown-preservation.md` (prompt ปัจจุบัน, structured output, เกณฑ์เทียบโครงสร้าง) ไม่ได้เลือกวิธีใด
- [Research: ตัวเลือกที่เก็บรูปภาพ/ไฟล์แนบ](tickets/051-research-attachment-storage.md): รวบรวมข้อเท็จจริง 4 ตัวเลือกใน `research/attachment-storage.md` (รวมข้อจำกัดเรื่องการแสดงรูปแบบมีสิทธิ์) ไม่ได้เลือกวิธีใด

## Not yet specified

- **เอกสารปิดท้าย**: หลังตัดสินใจครบ ต้องรวมเป็น requirements-v2 ฉบับ final, บันทึก ADR ยกเลิก non-goal (แชร์/ไฟล์แนบ) และ ADR ของ Topic+Note, อัปเดต ARCHITECTURE.md/GLOSSARY.md/CLAUDE.md ให้ตรง — ยังไม่ระบุว่าแบ่งงานอย่างไร
- **เรื่องที่อาจโผล่หลัง ticket Topic+Note/Share/Attachment resolve**: เช่น สิทธิ์ของผู้รับ Share ต่อไฟล์แนบ, การลบ/Trash เมื่อเนื้อหาเป็นของ Entity เดียวกัน — ยังไม่คมพอที่จะเป็น ticket
- **ชื่อใหม่ของฟีเจอร์ที่เคยมีคำว่า Topic**: Topic Suggestion, Topic Reference, Topic Browser/Detail, Topic Sharing, Topic Description — ต้องตั้งชื่อใหม่เมื่อเลิกใช้คำว่า Topic (ticket 053) ยังไม่ได้ตัดสินชื่อ ผูกกับ ticket Sharing/Description/Merge
- **ADR ที่ต้องเขียน**: การรวม Topic+Note และ Module Notes เป็นเจ้าของ (กระทบ ADR-0001 ถึง 0003), การยกเลิก non-goal แชร์/ไฟล์แนบ — ยังไม่ได้เขียน (อยู่ในขั้นเอกสารปิดท้าย)
- **ADR/เอกสารเรื่องการยืนยันอีเมล**: แก้หรือเขียนใหม่ต่อ ADR-0011 (สถานะยืนยันอีเมล + token ยืนยันที่เขียนเอง), ARCHITECTURE.md หัวข้อ Auth และ Local development, ขยายรายการ Error Log ของ ticket 055 (บริการภายนอกที่ล้มรวมการส่งอีเมล) — อยู่ในขั้นเอกสารปิดท้าย
- **งานที่ต้องทำก่อน build ส่วนส่งอีเมลจริง**: จัดหาโดเมนที่แก้ DNS ได้ (ยังไม่มี) เพื่อตั้ง SPF/DKIM และที่อยู่ผู้ส่ง — เป็นงานเตรียมการ ไม่ใช่การตัดสินใจ
- **Module เจ้าของ Share**: การแชร์ (ลิงก์, อีเมลที่อนุญาต, วันหมดอายุ, การตรวจสิทธิ์ผู้รับที่ไม่ล็อกอิน) ต้องมี Module เจ้าของ ยังไม่ได้ตัดสินว่าเป็น Module ใหม่หรืออยู่ใน Notes (ticket 059 พึ่งการตรวจขอบเขตแชร์ผ่าน Module Notes)
- **การค้นหาชื่อไฟล์แนบ**: ยังไม่ได้ตัดสินว่า Search ค้นชื่อไฟล์ได้หรือไม่ (ticket 059)

## Out of scope

- Database schema ใหม่, Entity model ละเอียด, API contract, UI specification, การเลือก storage provider จริง, migration plan, ticket/task breakdown, implementation order — เอกสารต้นทางระบุเองว่าเป็นขั้นหลัง Requirements v2 final
