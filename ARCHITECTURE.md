# Jotwai Architecture

ดู `GLOSSARY.md` สำหรับคำศัพท์ domain ที่ใช้ด้านล่าง และ `docs/adr/` สำหรับเหตุผลเบื้องหลังการตัดสินใจแต่ละอย่าง

## Overview

Jotwai เป็นแอปจดโน้ตแบบ "จดก่อน จัดทีหลัง": React SPA client, ASP.NET Core backend, เก็บข้อมูลใน PostgreSQL Backend เป็น **Modular Monolith** — deploy เป็นชิ้นเดียว แต่แบ่งเป็น 5 Module ที่มีขอบเขตชัดเจนภายใน ยังไม่ใช่ true microservices แต่ละ Module ถูกออกแบบให้สามารถแยกออกเป็น service ของตัวเองในอนาคตได้โดยไม่ต้อง rewrite

## Repository layout

Monorepo เดียว:
- `client/` — React SPA
- `backend/` — ASP.NET Core modular monolith (`Jotwai.Domain`, `Jotwai.Service`, `Web.Jotwai`, `Jotwai.sln`)
- `docs/adr/` — architecture decision records
- `GLOSSARY.md`, `ARCHITECTURE.md`, `CLAUDE.md` — เอกสารระดับ project

## Backend modules

| Module | เป็นเจ้าของ | หน้าที่รับผิดชอบ |
|---|---|---|
| **Notes** | schema `notes` | Note CRUD, เนื้อหา Markdown, soft delete |
| **Organization** | schema `organization` | Topics (แบบลำดับชั้น), Categories (แบบแบน), กฎการตั้งชื่อ |
| **AI Categorization** | schema `ai_categorization` | สร้าง Suggestion แบบ background, สถานะของ suggestion, outbox |
| **Search** | schema `search` | Search แบบ cross-cutting ครอบคลุมเนื้อหา/Topic/Category ผ่าน read model ของตัวเอง |
| **Identity** | schema `identity` | Account, login, session |

### Backend project structure (global 3-tier)

ตามมาตรฐานของบริษัท (ตาม convention เดิมของ Loyalty-Merchant) backend ใช้ layering แบบ Domain → Service → Web **เดียวทั้งระบบ** (global) ไม่ใช่แยกชุดต่อ Module (ADR-0010 แทนที่ ADR-0009):

- **`Jotwai.Domain`** — ไม่ reference project อื่นเลย มี entity/model, enum, request model, view model, validation model และ interface ของ**ทุก** Module จัดเรียงตามโฟลเดอร์ระดับ technical layer (`Models/`, `Enums/`, `Interfaces/`, `RequestModels/`, `ValidateModel/`, `ViewModels/`) — ไม่ใช่แยกโฟลเดอร์ตาม Module
- **`Jotwai.Service`** — reference `Jotwai.Domain` เท่านั้น มี business logic (`ImplementServices/`), helper (`Helper/`) และ `DbContext` + EF Core configuration ของแต่ละ Module
- **`Web.Jotwai`** — reference ทั้ง `Jotwai.Service` และ `Jotwai.Domain` มี Controller, `Program.cs`, middleware/configuration, `ExceptionHandler.cs`

Module (Identity และต่อไปคือ Notes/Organization/AI Categorization/Search) อยู่ภายใน 3 project นี้ในฐานะ feature ภายใน ไม่ใช่คู่ project แยกกัน ขอบเขตของ Module ถูกบังคับด้วยวินัยเรื่องโฟลเดอร์, code review และ convention การสื่อสารผ่าน event ด้านล่าง — ไม่ใช่ด้วย compiler

### Module communication

Module ไม่เรียกโค้ดของกันหรือ query schema ของกันโดยตรง ตาม convention พวกมันสื่อสารกันผ่าน **Domain Events** ที่ publish แบบ in-process ผ่าน mediator (สไตล์ MediatR) เมื่อ state ของ Module เจ้าของเปลี่ยน — เช่น `NoteCreated`, `NoteUpdated`, `NoteDeleted`, `TopicRenamed`, `CategoryCreated`, `CategoryRenamed`, `CategoryDeleted` Event type และการ wire handler ทั้งคู่อยู่ใน `Jotwai.Domain`/`Jotwai.Service` ที่ใช้ร่วมกัน (ADR-0010) — ไม่มีปัญหาเรื่อง cross-project reference ต้องแก้เพราะเป็น project เดียว แต่ก็ไม่มีอะไรกันไม่ให้เรียกข้าม Module โดยตรงนอกจาก code review

### Cross-module reads: Read Model ไม่ใช่ join

ถ้า Module หนึ่งต้องการข้อมูลที่ Module อื่นเป็นเจ้าของ (Search ต้องการเนื้อหา Note และชื่อ Topic/Category; AI Categorization ต้องการ Category list ปัจจุบัน) มันจะเก็บ **Read Model ของตัวเองแบบ denormalized** ที่อัปเดตให้ทันสมัยด้วยการ subscribe Domain Events ที่เกี่ยวข้อง ไม่มี Module ไหน join SQL ข้าม schema ของ Module อื่น แม้ในทางกายภาพทุก Module จะแชร์ PostgreSQL database เดียวกันก็ตาม นี่คือสิ่งที่ทำให้ขอบเขตนี้เป็นของจริงและพร้อมสำหรับการแยกในอนาคต

### Database

PostgreSQL database เดียว แบ่งแบบ **schema-per-module** (`notes`, `organization`, `ai_categorization`, `search`, `identity`) แต่ละ Module มี EF Core `DbContext` ของตัวเองผูกกับ schema ของตัวเอง (เช่น `IdentityDbContext`); `DbContext` เหล่านี้ทั้งหมดอยู่ใน project `Jotwai.Service` ที่ใช้ร่วมกัน (ADR-0010) — ไม่มี `AppDbContext` ที่ใช้ร่วมกัน

### AI Categorization flow

1. Notes Module บันทึก Note ใหม่ แล้ว publish `NoteCreated`
2. AI Categorization Module เขียนแถว **outbox** แบบ transactional เพื่อตอบสนอง (durable — รอดจากการ restart)
3. `BackgroundService` poll outbox แล้วเรียก AI provider เพื่อสร้าง Suggestion แบบจัดอันดับ (Category ที่เป็นตัวเลือก อาจรวมถึงชื่อใหม่ที่ยังไม่เคยมี)
4. สถานะของ Suggestion แสดงให้ client เห็นเป็น indicator ในแอปแบบไม่ real-time — ผู้ใช้จะเห็นในครั้งถัดไปที่เปิดดู Note หรือเปิดแอป ไม่มี push/websocket infrastructure
5. ผู้ใช้ดำเนินการ (accept บางส่วน / แก้ไข / เพิกเฉย) — สิ่งนี้ resolve ชุด suggestion; candidate ที่ไม่ได้ถูก accept จะถูกทิ้ง ไม่ใช่ค้างไว้
6. ตามกฎ **User Ownership**: flow นี้ทำงานแค่ครั้งเดียวโดยอัตโนมัติ ตอน save ครั้งแรกที่มีเนื้อหา การแก้ไข Note จะไม่ trigger ซ้ำ ผู้ใช้สามารถเรียก action "Suggest Category" ด้วยตัวเองได้ทุกเมื่อ

**AI provider**: Google Gemini (tier 2.5 Flash-Lite) — งาน classification ที่เบาแบบนี้ไม่จำเป็นต้องใช้โมเดลใหญ่/แพง Provider อยู่หลัง interface ภายใน AI Categorization Module เพื่อให้สลับได้ผ่าน config

### Search implementation

ใช้ query แบบ Postgres `ILIKE` substring กับ read model ของ Search Module เอง หนุนหลังด้วย trigram GIN index จาก `pg_trgm` เพื่อ performance ไม่มี search engine ภายนอก ไม่มี semantic/vector search ตรงตาม MVP scope ที่ตัดสินใจไว้ชัดเจน การ filter ตาม Topic แม่จะรวม Topic ลูกทั้งหมดด้วย

### Auth

ASP.NET Core Identity (email/password สำหรับ MVP, OAuth เป็น fast-follow) ออก JWT access + refresh token SPA แนบ access token ไปกับการเรียก API

### Background jobs

`BackgroundService` แบบ in-process ที่ consume ตาราง outbox ที่อธิบายไว้ข้างบน ไม่มี external message broker (RabbitMQ/Azure Service Bus) สำหรับ MVP — นี่คือจุดที่จะใส่ message broker ถ้า AI Categorization ถูกแยกออกเป็น service ของตัวเองในอนาคต

## Frontend

- **React + TypeScript + Vite**
- **react-router-dom** สำหรับ routing
- **TanStack Query** สำหรับ server state (fetch/cache Notes, Topics, Categories)
- **axios** สำหรับเรียก HTTP ไปยัง API
- ไม่มี client-state library เฉพาะ — `useState`/Context จัดการ UI-only state (filter ที่ใช้งาน, draft ของ editor, สถานะ sidebar)
- **Tailwind CSS** + headless component สไตล์ shadcn/ui สำหรับ styling
- **Tiptap** (ProseMirror) เป็น editor ของ Note, serialize ไป/กลับเป็น Markdown — ให้ bullet/numbered list แบบ WYSIWYG และตัวเลือกภาษาของ code block ในขณะที่ที่เก็บข้อมูลจริงยังเป็น Markdown ธรรมดา

Navigation model: feed "All Notes" เป็นค่าเริ่มต้น (เรียงตามอัปเดตล่าสุด, infinite scroll) พร้อม Topic tree และตัวกรอง Category เป็น side navigation แบบถาวร; search เป็น layer เสริมด้านบน ไม่ใช่วิธีเดียวในการหา Note

## Deployment topology

ASP.NET Core backend เสิร์ฟ**เฉพาะ** JSON API React SPA ถูก build และ deploy เป็น **artifact แบบ static แยกต่างหาก** (static hosting/CDN) เรียก API ผ่าน HTTP ทั้งสองส่วน deploy และ scale ได้อย่างอิสระจากกัน

## Local development

`docker-compose.yml` ที่ root รัน **เฉพาะ PostgreSQL** API รันแบบ native ผ่าน `dotnet watch`; client รันแบบ native ผ่าน `vite dev` App process ไม่ถูก containerize สำหรับ dev ในเครื่อง — มีแค่ database ที่ถูก containerize เพื่อให้ inner loop เร็ว

## Explicit non-goals for MVP

- ไม่มี LLM semantic/vector search
- ไม่มี real-time push notification (websocket) สำหรับ AI suggestion
- ไม่มีการแชร์/ทำงานร่วมกันของ note ระหว่าง account
- ไม่มี version history ของ note
- ไม่มีการแนบไฟล์/รูปภาพใน Note
- ไม่มี true microservices deployment แยกจริง (เป็น extraction ที่เป็นไปได้ในอนาคต ไม่ใช่ MVP)
