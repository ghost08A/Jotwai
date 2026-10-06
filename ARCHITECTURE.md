# Jotwai Architecture

ดู `CONTEXT.md` สำหรับคำศัพท์ domain ที่ใช้ด้านล่าง และ `docs/adr/` สำหรับเหตุผลเบื้องหลังการตัดสินใจแต่ละอย่าง

> **สถานะ: เอกสารนี้อธิบายสถาปัตยกรรมเป้าหมายของรุ่น v2** (ตัดสินใจใน `docs/wayfinder/map-v2.md`, ADR-0013 ถึง 0018) ส่วนที่ยัง **ไม่ได้ implement** ได้แก่ การรวม Topic+Note, Module Logging, การแชร์, ไฟล์แนบ, การตรวจการชนตอนบันทึก, Description, การยืนยันอีเมล และกฎโครงสร้างใหม่ของ Content Polish — โค้ดปัจจุบันยังเป็นแบบเดิม (Topic อยู่ใน Module Organization แยกจาก Note)

## Overview

Jotwai เป็นแอปจดโน้ตแบบ "จดก่อน จัดทีหลัง": React SPA client, ASP.NET Core backend, เก็บข้อมูลใน PostgreSQL Backend เป็น **Modular Monolith** — deploy เป็นชิ้นเดียว แต่แบ่งเป็น 6 Module ที่มีขอบเขตชัดเจนภายใน ยังไม่ใช่ true microservices แต่ละ Module ถูกออกแบบให้สามารถแยกออกเป็น service ของตัวเองในอนาคตได้โดยไม่ต้อง rewrite

## Repository layout

Monorepo เดียว:
- `client/` — React SPA
- `backend/` — ASP.NET Core modular monolith (`Jotwai.Domain`, `Jotwai.Service`, `Web.Jotwai`, `Jotwai.sln`)
- `docs/adr/` — architecture decision records
- `CONTEXT.md`, `ARCHITECTURE.md`, `CLAUDE.md`, `requirements-v2.md` — เอกสารระดับ project

## Backend modules

| Module | เป็นเจ้าของ | หน้าที่รับผิดชอบ |
|---|---|---|
| **Notes** | schema `notes` | Note (ชื่อ เนื้อหา Markdown ลำดับชั้นแม่/ลูก ถังขยะ Description) รวมการติด Category (join), ไฟล์แนบ (ข้อมูลของไฟล์ ไม่ใช่ตัวไบต์), Share Link และการตรวจการชนตอนบันทึก (ADR-0013/0014/0015/0018) |
| **Organization** | schema `organization` | **Category** (แบบแบน) และกฎการตั้งชื่อ Category เท่านั้น (ลำดับชั้นย้ายไปอยู่ใน Notes แล้ว) |
| **Nerd Assistant** (เดิมชื่อ "AI Categorization" — เปลี่ยนแค่ชื่อในเอกสาร, schema/class ในโค้ดยังไม่เปลี่ยน ดู CONTEXT.md) | schema `ai_categorization` | เรียก AI (Gemini) ทำ 4 เรื่อง: AI Categorization (เสนอ Category ให้ Note แบบ background), Note Suggestion (เสนอ Merge/Placement แบบ background), Description (สร้างให้ครั้งแรก/เมื่อกดปุ่ม), Content Polish (แต่งเนื้อหา Note แบบ on-demand) |
| **Search** | schema `search` | Search แบบ cross-cutting ครอบคลุมชื่อ/เนื้อหา/Category ผ่าน read model ของตัวเอง |
| **Identity** | schema `identity` | Account, login, session, การยืนยันอีเมลตอนสมัคร (ADR-0017) |
| **Logging** | schema `logging` | Error Log (ADR-0016) — Module อื่นเขียนเข้าได้ตรง |

### Backend project structure (global 3-tier)

ตามมาตรฐานของบริษัท (ตาม convention เดิมของ Loyalty-Merchant) backend ใช้ layering แบบ Domain → Service → Web **เดียวทั้งระบบ** (global) ไม่ใช่แยกชุดต่อ Module (ADR-0010 แทนที่ ADR-0009):

- **`Jotwai.Domain`** — ไม่ reference project อื่นเลย มี entity/model, enum, request model, view model, validation model และ interface ของ**ทุก** Module จัดเรียงตามโฟลเดอร์ระดับ technical layer (`Models/`, `Enums/`, `Interfaces/`, `RequestModels/`, `ValidateModel/`, `ViewModels/`) — ไม่ใช่แยกโฟลเดอร์ตาม Module
- **`Jotwai.Service`** — reference `Jotwai.Domain` เท่านั้น มี business logic (`ImplementServices/`), helper (`Helper/`) และ `DbContext` + EF Core configuration ของแต่ละ Module
- **`Web.Jotwai`** — reference ทั้ง `Jotwai.Service` และ `Jotwai.Domain` มี Controller, `Program.cs`, middleware/configuration (ตัวดักข้อผิดพลาดกลางที่เขียนลง Error Log ยังไม่มีในโค้ด — เอกสารเดิมเคยอ้างถึง `ExceptionHandler.cs` แต่ไฟล์นี้ไม่มีอยู่จริง ดู ADR-0016)

Module (Identity และต่อไปคือ Notes/Organization/Nerd Assistant/Search/Logging) อยู่ภายใน 3 project นี้ในฐานะ feature ภายใน ไม่ใช่คู่ project แยกกัน ขอบเขตของ Module ถูกบังคับด้วยวินัยเรื่องโฟลเดอร์, code review และ convention การสื่อสารผ่าน event ด้านล่าง — ไม่ใช่ด้วย compiler

### Module communication

Module ไม่เรียกโค้ดของกันหรือ query schema ของกันโดยตรง ตาม convention พวกมันสื่อสารกันผ่าน **Domain Events** ที่ publish แบบ in-process ผ่าน mediator (สไตล์ MediatR) เมื่อ state ของ Module เจ้าของเปลี่ยน — เช่น `NoteCreated`, `NoteUpdated`, `NoteDeleted`, `NoteRenamed`, `CategoryCreated`, `CategoryRenamed`, `CategoryDeleted` Event type และการ wire handler ทั้งคู่อยู่ใน `Jotwai.Domain`/`Jotwai.Service` ที่ใช้ร่วมกัน (ADR-0010) — ไม่มีปัญหาเรื่อง cross-project reference ต้องแก้เพราะเป็น project เดียว แต่ก็ไม่มีอะไรกันไม่ให้เรียกข้าม Module โดยตรงนอกจาก code review **ข้อยกเว้นเดียว**: Module อื่นเขียนลง Module **Logging** ได้โดยตรง เพราะ Error Log ต้องเขียนได้แม้ระบบกำลังมีปัญหา (ADR-0016)

### Cross-module reads: Read Model ไม่ใช่ join

ถ้า Module หนึ่งต้องการข้อมูลที่ Module อื่นเป็นเจ้าของ (Search ต้องการเนื้อหา Note และชื่อ Category; Nerd Assistant ต้องการ Category list ปัจจุบันและ list ของ Note ทั้งหมด (Id/ชื่อ/Description/แม่); Notes ต้องการรายการ Category ที่มีอยู่เพื่อตรวจก่อนติด) มันจะเก็บ **Read Model ของตัวเองแบบ denormalized** ที่อัปเดตให้ทันสมัยด้วยการ subscribe Domain Events ที่เกี่ยวข้อง ไม่มี Module ไหน join SQL ข้าม schema ของ Module อื่น แม้ในทางกายภาพทุก Module จะแชร์ PostgreSQL database เดียวกันก็ตาม นี่คือสิ่งที่ทำให้ขอบเขตนี้เป็นของจริงและพร้อมสำหรับการแยกในอนาคต เมื่อรวม Topic เข้ากับ Note (ADR-0013) read model ระหว่าง Notes ↔ Organization ที่เคยมีเพราะ Topic กับ Note แยกกัน (เช่น `KnownTopic`) หายไป

### Database

PostgreSQL database เดียว แบ่งแบบ **schema-per-module** (`notes`, `organization`, `ai_categorization`, `search`, `identity`, `logging`) แต่ละ Module มี EF Core `DbContext` ของตัวเองผูกกับ schema ของตัวเอง (เช่น `IdentityDbContext`); `DbContext` เหล่านี้ทั้งหมดอยู่ใน project `Jotwai.Service` ที่ใช้ร่วมกัน (ADR-0010) — ไม่มี `AppDbContext` ที่ใช้ร่วมกัน ไฟล์ตัวไบต์ของ Attachment ไม่อยู่ในฐานข้อมูล แต่อยู่ใน S3-compatible storage

### AI Categorization flow (เสนอ Category ให้ Note)

1. Notes Module บันทึก Note ใหม่ แล้ว publish `NoteCreated`
2. Nerd Assistant Module เขียนแถว **outbox** แบบ transactional เพื่อตอบสนอง (durable — รอดจากการ restart)
3. `BackgroundService` poll outbox แล้วเรียก AI provider (ส่งเนื้อหา Note + Description ของ Note นั้นถ้ามี + ชื่อ Category ที่มีอยู่) เพื่อสร้าง Suggestion แบบจัดอันดับ (Category ที่เป็นตัวเลือก อาจรวมถึงชื่อใหม่ที่ยังไม่เคยมี)
4. สถานะของ Suggestion แสดงให้ client เห็นเป็น indicator ในแอปแบบไม่ real-time — ผู้ใช้จะเห็นในครั้งถัดไปที่เปิดดู Note หรือเปิดแอป ไม่มี push/websocket infrastructure
5. ผู้ใช้ดำเนินการ (accept บางส่วน / แก้ไข / เพิกเฉย) — สิ่งนี้ resolve ชุด suggestion; candidate ที่ไม่ได้ถูก accept จะถูกทิ้ง ไม่ใช่ค้างไว้
6. ตามกฎ **User Ownership**: flow นี้ทำงานแค่ครั้งเดียวโดยอัตโนมัติ ตอน save ครั้งแรกที่มีเนื้อหา การแก้ไข Note จะไม่ trigger ซ้ำ ผู้ใช้สามารถเรียก action "Suggest Category" ด้วยตัวเองได้ทุกเมื่อ

### Description flow (สร้างข้อความอธิบาย Note)

1. ตอนบันทึกครั้งแรกที่มีเนื้อหาจริง (เงื่อนไขเดียวกับ AI Categorization) Nerd Assistant สร้าง Description ให้ครั้งเดียว (ไม่เกิน 200 ตัวอักษร) ผ่านงานเบื้องหลัง
2. หลังจากนั้นสร้างใหม่ได้เฉพาะตอนผู้ใช้กดปุ่ม "สร้างใหม่" ถ้า Description เคยถูกผู้ใช้เขียนหรือแก้เอง ต้องผ่านขั้นยืนยันก่อนเขียนทับ ถ้าผู้ใช้ลบจนว่างคือคืนสิทธิ์ให้ AI สร้างใหม่ได้
3. Description ถูกใช้เป็นข้อมูลของ AI Categorization และ Note Suggestion (ไม่ใช้กับ Content Polish) ไม่ถูก Search ค้น และถูกแสดงให้ผู้รับ Share Link เห็น

### Note Suggestion flow (เสนอ Merge/Placement) — เดิมชื่อ Topic Suggestion

1. Notes Module สร้าง Note ใหม่ แล้ว publish `NoteCreated` (event ที่มีอยู่แล้วสำหรับ read model อื่นๆ)
2. Nerd Assistant Module เขียนแถว outbox แบบเดียวกับ AI Categorization flow (durable, ไม่บล็อก UI)
3. Nerd Assistant มี read model ของตัวเอง (Id/ชื่อ/Description/ParentId ของทุก Note ของ user ที่ไม่ได้อยู่ในถังขยะ) sync ผ่าน `NoteCreated`/`NoteRenamed`/`NoteDeleted`/event ที่เกี่ยวข้อง — ไม่ query schema `notes` ตรงๆ
4. `BackgroundService` เรียก AI provider ครั้งเดียว เทียบชื่อ+Description+เนื้อหาของ Note ใหม่กับชื่อ+Description ของ Note ที่มีอยู่ทั้งหมด (ทุก branch/ทุกระดับ) ให้ตอบกลับมาเป็นอย่างใดอย่างหนึ่ง: **Merge**, **Placement** หรือไม่เข้าเงื่อนไขไหนเลย (ถ้าเข้าเงื่อนไข Merge ให้ Merge ชนะเสมอ) — ใช้ confidence threshold ของตัวเอง แยกจาก AI Categorization flow (เกณฑ์สูงกว่า เพราะ Merge ลบข้อมูลจริง) ไม่เสนอเมื่อ Note เดิมเป็นลูกหลานของ Note ใหม่
5. Note Suggestion แสดงเป็น indicator ที่รายการ Note และรายละเอียด/ปุ่มยืนยันอยู่ที่หน้า Note (แบบไม่ real-time เหมือน AI Categorization flow)
6. ผู้ใช้ยืนยัน **Placement**: ย้าย Note ใหม่ไปเป็นลูกของ Note ที่แนะนำทันที ไม่ต้องยืนยันซ้ำ
7. ผู้ใช้ยืนยัน **Merge** (ต้องผ่าน dialog ยืนยันซ้ำที่มี preview เนื้อหาหลังรวม ตามตัวเลือก: เก็บของเดิม/ของใหม่/ต่อใหม่ท้ายเดิม/ต่อเดิมท้ายใหม่): ตรวจก่อนว่าเนื้อหาของทั้งสอง Note ไม่เปลี่ยนหลัง AI คำนวณ (ADR-0014) แล้วทำใน **ธุรกรรมเดียวข้ามสอง schema** (`notes` และ `ai_categorization` — ผูก DbContext เข้าด้วยกันแบบเดียวกับตอน accept Category Suggestion): ย้ายลูกและ Category ของ Note ใหม่ไปที่ Note เดิม, ย้ายเจ้าของไฟล์แนบที่เนื้อหายังอยู่, ยกเลิกลิงก์แชร์ของ Note ใหม่, ตั้งการเปลี่ยนเส้นทางของ Note Reference ที่ชี้ Note ใหม่ไป Note เดิม, ลบ Note ใหม่ถาวร (กู้คืนไม่ได้) และ resolve suggestion
8. ตามกฎ **User Ownership**: resolve ครั้งเดียวจบ ไม่ trigger ซ้ำอัตโนมัติอีก แม้จะเปลี่ยนชื่อ Note ทีหลัง

### Content Polish flow (แต่งเนื้อหา Note)

1. ผู้ใช้กดปุ่ม (floating button มุมขวาล่างของ editor) — เรียก Nerd Assistant แบบ **sync** ตรงๆ ไม่ผ่าน outbox/background (ผู้ใช้รอผลสดๆ หน้าจอ) ไม่ส่ง Description ให้ AI
2. AI จัดรูปแบบเนื้อหาทั้ง Note ให้อ่านง่ายขึ้น — แก้คำผิด, จัดย่อหน้าเป็นข้อย่อย, ขยายคำอธิบาย — ไม่แตะเนื้อหาใน code block เด็ดขาด และต้องรักษาชนิดของโครงสร้างที่ผู้ใช้เขียนไว้ตามกฎใน requirements-v2.md §2.6 (ห้ามเปลี่ยนชนิดรายการ/ระดับหัวข้อ/quote; to-do list ห้ามรวม/แยก/สลับ/เพิ่ม/ลบข้อ; ห้ามแตะ code block, สถานะ checkbox, เลขในรายการเลข, ลิงก์ภายนอก, mark ที่มีอยู่, เส้นคั่นที่มีอยู่, Note Reference และอ้างอิงไฟล์แนบ `attachment:KEY`)
3. ส่วนที่เป็นการแก้ตรงๆ (คำผิด/รูปแบบ) มาตรงๆ ในผลลัพธ์ ส่วนที่อาจกระทบความหมายเดิม (ตัวเลข/ข้อเท็จจริง/ประโยคที่ AI เดาเติมใหม่) จะมีข้อความเดิมกำกับไว้ในวงเล็บให้เห็นทั้งคู่
4. หลัง AI ตอบ ระบบเทียบโครงสร้างก่อน-หลังด้วยโปรแกรมธรรมดา (ไม่ใช้ AI) เฉพาะ "ชนิดของโครงสร้าง" ไม่เทียบจำนวน/ลำดับข้อ แล้วสร้าง **คำเตือนใน preview** (ผู้ใช้ยังกดยืนยันได้) เมื่อ: ชนิดรายการเปลี่ยน (เลข/จุด/checkbox), ระดับหัวข้อเปลี่ยนหรือหัวข้อหาย/เพิ่ม, สถานะ checkbox เปลี่ยน, quote หายหรือถูกสร้าง, code block เปลี่ยน, Note Reference หาย/ชี้เปลี่ยน, ลิงก์ภายนอกหาย/ที่อยู่เปลี่ยน, อ้างอิงไฟล์แนบหาย/เปลี่ยน — ไม่เตือนเรื่องจำนวน/ลำดับข้อ, เส้นคั่นหรือตัวหนาที่เพิ่ม และย่อหน้าที่ถูกจัดเป็นรายการจุด (ยังไม่ได้พิสูจน์ว่าการเทียบนี้จับอาการ `- 1.` ได้จริง ต้องทดสอบตอน implement)
5. Client โชว์ผลลัพธ์เป็น popup preview พร้อมคำเตือน — ผู้ใช้กด "ยืนยัน" (overwrite เนื้อหาจริงทั้งฉบับทีเดียว หลังตรวจว่าเนื้อหาไม่เปลี่ยนหลังที่ AI คำนวณ ตาม ADR-0014) หรือ "ยกเลิก" (ไม่มีอะไรเปลี่ยน) ไม่มี state ค้างในระบบเลยถ้าไม่กดยืนยัน (ไม่มี entity/table เก็บผลลัพธ์นี้)

**AI provider**: Google Gemini (tier 2.5 Flash-Lite) — งาน classification ที่เบาแบบนี้ไม่จำเป็นต้องใช้โมเดลใหญ่/แพง เริ่มต้นใช้ tier เดียวกันนี้ทั้ง 4 flow ข้างบน Provider อยู่หลัง interface ภายใน Nerd Assistant Module เพื่อให้สลับ tier/provider ได้ผ่าน config โดยไม่ต้องแก้โค้ด

### Search implementation

ใช้ query แบบ Postgres `ILIKE` substring กับ read model ของ Search Module เอง หนุนหลังด้วย trigram GIN index จาก `pg_trgm` เพื่อ performance ไม่มี search engine ภายนอก ไม่มี semantic/vector search ตรงตาม MVP scope ที่ตัดสินใจไว้ชัดเจน ค้นจาก ชื่อ Note, เนื้อหา, ภาษา code block และชื่อ Category (ไม่ค้น Description, ชื่อไฟล์แนบ หรือเนื้อหาไฟล์แนบ) การ filter ตาม Note แม่จะรวม Note ลูกทั้งหมดด้วย และไม่นับ Note ในถังขยะ

### Auth

JWT access + refresh token SPA แนบ access token ไปกับการเรียก API (Identity เขียนเอง ใช้เฉพาะตัวแฮชรหัสผ่านและ password policy ของ ASP.NET Core Identity ตาม ADR-0011; OAuth เป็น fast-follow) **การสมัครต้องยืนยันอีเมลก่อนล็อกอิน**: สมัครเสร็จยังไม่ได้ token ต้องกดลิงก์ในอีเมลที่หมดอายุใน 24 ชั่วโมงก่อน อีเมลส่งผ่านบริการส่งอีเมลภายนอก (ADR-0017) ส่งไม่สำเร็จ → สมัครยังสำเร็จ มีปุ่มส่งซ้ำ และบันทึก Error Log **ข้อยกเว้นจากกฎ "ทุก endpoint ต้องมี token"**: public read path ของ Share Link แบบ Public (ไม่ต้องล็อกอิน) ตาม ADR-0018

### Sharing (Share Link)

อยู่ใน Module Notes ผู้รับเห็นเฉพาะ Note ที่แชร์และทุกอย่างใต้มัน ณ ตอนเปิดดู แบบอ่านอย่างเดียว (Public ไม่ต้องมีบัญชี / จำกัดอีเมลต้องล็อกอินด้วยบัญชีที่อีเมลตรงและยืนยันแล้ว) รายละเอียดอยู่ใน requirements-v2.md §2.3 และ ADR-0018 ต้องมีรหัสลิงก์ที่เดาไม่ได้และ rate limit บน public read path

### Attachments

ข้อมูลของไฟล์ (ชื่อ ขนาด ชนิด Note ที่สังกัด key) อยู่ใน Module Notes ตัวไบต์อยู่ใน **S3-compatible storage** **API เป็นคนดึงไฟล์แล้วส่งต่อให้ผู้ใช้/ผู้รับ Share Link ทุกครั้ง** พร้อมตรวจสิทธิ์ทุกครั้ง (ไม่แจก presigned URL) เพื่อให้ปิด/หมดอายุลิงก์แล้วเปิดไฟล์ไม่ได้ทันที รายละเอียด allowlist เพดาน การคัดลอกข้าม Note และการเก็บกวาดอยู่ใน requirements-v2.md §2.7 และ ADR-0015

### Concurrent editing (stale write detection)

บันทึกเนื้อหาด้วยข้อมูลที่ไม่ใช่เวอร์ชันล่าสุด → ไม่เขียนทับ ให้ผู้ใช้เลือก (ADR-0014) แท็บที่กลับมาโฟกัสถามเซิร์ฟเวอร์เองว่ามีเวอร์ชันใหม่ไหม (ไม่ใช่ push) ไม่ใช้ CRDT ไม่รวมข้อความอัตโนมัติ ไม่เก็บ version history

### Error logging

Module Logging เก็บ Error Log ใน schema `logging` ผ่านตัวดักข้อผิดพลาดกลางของ `Web.Jotwai` และจุดดัก Error ของงานเบื้องหลัง/การเรียกบริการภายนอก ไม่เก็บ 4xx/Error ฝั่งหน้าเว็บ ห้ามเก็บเนื้อหา Note/body/อีเมล/query string/Authorization ลบเมื่อเก่ากว่า 6 เดือน (ADR-0016)

### Background jobs

`BackgroundService` แบบ in-process ที่ consume ตาราง outbox ที่อธิบายไว้ข้างบน (รวมการสร้าง Description) ไม่มี external message broker (RabbitMQ/Azure Service Bus) สำหรับ MVP — นี่คือจุดที่จะใส่ message broker ถ้า Nerd Assistant ถูกแยกออกเป็น service ของตัวเองในอนาคต งานเก็บกวาดไฟล์แนบ (ไม่ถูกอ้างถึงเกิน 7 วัน) และงานลบ Error Log เกิน 6 เดือน (รันเดือนละครั้ง) ก็เป็นงานเบื้องหลังในลักษณะเดียวกัน

## Frontend

- **React + TypeScript + Vite**
- **react-router-dom** สำหรับ routing
- **TanStack Query** สำหรับ server state (fetch/cache Notes, Categories)
- **axios** สำหรับเรียก HTTP ไปยัง API
- ไม่มี client-state library เฉพาะ — `useState`/Context จัดการ UI-only state (filter ที่ใช้งาน, draft ของ editor, สถานะ sidebar)
- **Tailwind CSS** + headless component สไตล์ shadcn/ui สำหรับ styling
- **Tiptap** (ProseMirror) เป็น editor ของ Note, serialize ไป/กลับเป็น Markdown — ให้ bullet/numbered list แบบ WYSIWYG และตัวเลือกภาษาของ code block ในขณะที่ที่เก็บข้อมูลจริงยังเป็น Markdown ธรรมดา (v2 เพิ่ม: รูป/ไฟล์แนบผ่าน `attachment:KEY`, วางรูป/ลากไฟล์/slash command, แถบเตือนตอนบันทึกชน, หน้าอ่านอย่างเดียวของ Share Link)

Navigation model: feed "All Notes" เป็นค่าเริ่มต้น (เรียงตามอัปเดตล่าสุด, infinite scroll) พร้อมต้นไม้ Note (ลำดับชั้นแม่/ลูก) และตัวกรอง Category เป็น side navigation แบบถาวร; search เป็น layer เสริมด้านบน ไม่ใช่วิธีเดียวในการหา Note

## Deployment topology

ASP.NET Core backend เสิร์ฟ**เฉพาะ** JSON API (และไฟล์แนบผ่าน API) React SPA ถูก build และ deploy เป็น **artifact แบบ static แยกต่างหาก** (static hosting/CDN) เรียก API ผ่าน HTTP ทั้งสองส่วน deploy และ scale ได้อย่างอิสระจากกัน ต้องมี S3-compatible storage (ยังไม่เลือกผู้ให้บริการ) และบริการส่งอีเมลภายนอก (ยังไม่เลือกเจ้า; ต้องมีโดเมนที่แก้ DNS ได้สำหรับที่อยู่ผู้ส่ง — **ยังไม่มี**)

## Local development

`docker-compose.yml` ที่ root รัน PostgreSQL และ (เมื่อ v2 implement) **ตัวรับอีเมลจำลอง** (เช่น Mailpit/smtp4dev — ยังไม่เลือกตัวใดตัวหนึ่ง) และตัวจำลอง S3 สำหรับ dev (ยังไม่ตัดสิน) API รันแบบ native ผ่าน `dotnet watch`; client รันแบบ native ผ่าน `vite dev` App process ไม่ถูก containerize สำหรับ dev ในเครื่อง — containerize เฉพาะบริการภายนอกเพื่อให้ inner loop เร็ว

## Explicit non-goals

- ไม่มี LLM semantic/vector search
- ไม่มี real-time push notification (websocket) สำหรับ AI suggestion
- ไม่มีการทำงานร่วมกัน (collaboration) ของ note ระหว่าง account — **การแชร์แบบอ่านอย่างเดียวผ่านลิงก์อยู่ใน scope ของ v2 แล้ว** (ADR-0018)
- ไม่มี version history ของ note
- ไม่มีการลบ Note ถาวร และไม่สแกนไวรัสไฟล์แนบใน v2
- ไม่มี true microservices deployment แยกจริง (เป็น extraction ที่เป็นไปได้ในอนาคต ไม่ใช่ MVP)

(เดิมมีข้อ "ไม่มีการแนบไฟล์/รูปภาพใน Note" — ถูกยกเลิกแล้วโดย ADR-0015)
