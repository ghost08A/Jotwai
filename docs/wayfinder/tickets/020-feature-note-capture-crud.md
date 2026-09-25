---
id: 20
title: "สร้าง: การบันทึกและ CRUD ของ Note (สร้างได้โดยไม่ต้องมี Topic/Category, แก้ไขแบบเขียนทับ, ลบแบบ soft delete + ถังขยะ)"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §2, §9; GLOSSARY.md "Note"; การตัดสินใจเรื่องถังขยะในตั๋ว 008) ตัดสินใจครบถ้วนแล้วสำหรับการบันทึก Note ยังไม่มีสิ่งใดอยู่ในโค้ดเลย (ยืนยันแล้ว: ไม่มี model/controller ของ `Note` อยู่ที่ใดใน `backend/`, ไม่มี UI ของ Note ใน `client/src`) สิ่งที่ต้องสร้าง:

- การสร้าง Note โดยไม่ต้องมี Topic/Category บังคับ บันทึกทันที (BR-001, BR-002)
- เนื้อหาจัดเก็บเป็น Markdown; การแก้ไขจะเขียนทับ (ไม่มีประวัติเวอร์ชัน)
- Soft delete + มุมมองถังขยะ/Bin ที่ตัดสินใจไว้ (รายการ + ปุ่มกู้คืน, ตั๋ว 008)

นี่คืองานสร้าง (build task) ไม่ใช่การตัดสินใจที่ค้างอยู่ — ถูกติดตามที่นี่ในฐานะ backlog ตามธรรมเนียมของ map นี้เรื่องหนี้เชิงการนำไปใช้จริง ขึ้นอยู่กับการมี Module/schema ของ Notes อยู่ก่อน (ดูการตรวจสอบ ADR-0001/0003, ตั๋ว 011/013) และกลไก Domain Event (ตั๋ว 012) เพื่อให้ `NoteCreated` สามารถสั่งให้ AI Categorization ทำงาน (ตั๋ว 024)

## Finding

ยังไม่เริ่มดำเนินการ ไม่มีเอนทิตี `Note`, DbContext, controller หรือ UI ฝั่ง client สำหรับ Note อยู่ที่ใดในโปรเจกต์เลย

## Resolution

Build เสร็จและ verify ครบวงจรแล้ว (2026-09-23) — เฉพาะฝั่ง backend (client UI ยังไม่ทำ ไม่อยู่ใน scope ของ ticket นี้):

- `Jotwai.Domain`: `Models/Note.cs` (`Id`, `UserId`, `Content`, `CreatedAt`, `UpdatedAt`, `DeletedAt` — ตาม decision ของ ticket 028/030/031: ไม่มี `Title`, `DeletedAt` เดี่ยวๆ), `Events/NoteCreated.cs`/`NoteUpdated.cs`/`NoteDeleted.cs`, `Interfaces/INotesService.cs`, request/view model
- กลไก Domain Event ของ ADR-0002 ถูกสร้างขึ้นครั้งแรกพร้อมกันนี้ (`IDomainEvent`, `IEventHandler<T>`, `IEventPublisher`, `InProcessEventPublisher`) — ยังไม่มี handler subscribe เลย (ตามคาด เพราะ AI Categorization module ยังไม่ถูก build) แต่ `NotesService` publish `NoteCreated`/`NoteUpdated`/`NoteDeleted` แล้วจริง
- `Jotwai.Service`: `NotesDbContext` (schema `notes`, ตาม ADR-0003), migration `InitialCreate`, `NotesService` (create/list/get/update overwrite/soft-delete/restore/list-trash, scope ด้วย `UserId` ตาม decision 028), `NotesServiceRegistration`
- `Web.Jotwai`: `NotesController` (`POST/GET/PUT/DELETE /api/notes`, `GET /api/notes/trash`, `POST /api/notes/{id}/restore`) ทั้งหมด `[Authorize]`
- Integration test 8 เคสใหม่ใน `Jotwai.IntegrationTests/NotesTests.cs` (create, list scoped ต่อ user, update overwrite, update ของคนอื่นได้ 404, delete เข้า trash, restore, delete ซ้ำได้ 404, endpoint ปฏิเสธ request ไม่มี token) — รวม test เดิมผ่านครบ 27/27, `dotnet format --verify-no-changes` ผ่าน (ยกเว้นไฟล์ migration ที่ generate เองซึ่งมีปัญหา CRLF/BOM เหมือนกับ migration เดิมของ Identity ที่ commit ไว้แล้ว — เป็น artifact ของ `dotnet ef` tooling เอง ไม่ใช่โค้ดที่แก้)
- ทำให้ ticket 011/012/013 (audit ADR-0001/0002/0003) ปิดตามไปด้วยในฐานะผลพลอยได้ — ดู Resolution ของแต่ละ ticket

**ไม่ได้ทำใน scope นี้ (ตั้งใจ)**: `AiCategorizationTriggeredAt` บน `Notes` (decision ของ ticket 033 — จะเพิ่มตอน build ticket 024 จริง), Topic/Category FK บน Note (ticket 022/023 ยังไม่ build), client UI

**Client UI (2026-09-25)**: build เสร็จและ verify ครบวงจรจริงแล้วผ่าน browser ต่อ backend จริง (register → capture → save → edit (overwrite) → delete → trash → restore → กลับมาอยู่ใน active list) —

- `modules/notes/types`, `services/notesService.ts` (เรียก `/api/notes`, `/api/notes/trash`, `/api/notes/{id}`, `/api/notes/{id}/restore` ตรงตาม `NotesController` ที่มีอยู่แล้ว), `hooks/` (`useNotes`/`useTrash`/`useCreateNote`/`useUpdateNote`/`useDeleteNote`/`useRestoreNote` ห่อ TanStack Query, invalidate query key ที่เกี่ยวข้องหลัง mutation)
- `components/NotesPage.tsx` (composer แบบ capture-first ด้านบนด้วย `NoteEditor` เดียวกับ base editor ticket 017 + list ของ Note ที่ยัง active), `NoteCard.tsx` (คลิกเพื่อขยายเป็นแก้ไข ใช้ `NoteEditor` เดียวกัน แก้แบบเขียนทับ), `TrashPage.tsx` (list ถังขยะ read-only + ปุ่มกู้คืนต่อรายการ ตาม decision ตั๋ว 008), `NoteCardSkeleton.tsx` (skeleton ระหว่าง fetch ตามกฎ Loading Indicator ของ client/CLAUDE.md)
- เพิ่ม `core/layouts/PrivateLayout.tsx` (header + nav Notes/Trash + สถานะผู้ใช้ + logout ใช้ร่วมกันทุกหน้า protected) แทนที่ placeholder `core/pages/Home.tsx` เดิม (ลบทิ้ง) — route `/` = Notes list, `/trash` = Trash ใหม่ (`ROUTES.trash`)
- ใช้ component จาก headless library ที่เพิ่งตั้ง (`Button`) และ design token (`bg-card`, `text-muted-foreground`, `text-destructive` ฯลฯ) ตาม ADR-0008 audit ticket 017 รอบล่าสุด ไม่ได้เพิ่ม component ใหม่ (ใช้ `Button` เดิมพอ)
- `npm run lint` + `npm run build` ผ่านสะอาด verify ผ่าน browser จริงกับ backend ที่รันจริง (ไม่ใช่ mock) รวมถึงยืนยันว่า restore ทำให้ note กลับมาแสดงถูกต้อง (พิสูจน์ทางอ้อมว่า `NoteRestored` event ที่เพิ่มระหว่าง build Search module ทำงานถูกต้องแล้ว)
- ยังไม่ทำ: การกำหนด Topic/Category ให้ Note จาก UI (ticket 022/023 ฝั่ง client), Code Block ในบริบทของ Note จริง (ticket 021 — editor รองรับแล้วแต่ยังไม่มี test เจาะจงจาก flow นี้), AI Categorization suggestion UI (ticket 024 ฝั่ง client)
