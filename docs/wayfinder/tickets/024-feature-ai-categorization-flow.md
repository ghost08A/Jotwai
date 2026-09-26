---
id: 24
title: "สร้าง: กระบวนการเสนอแนะของ AI Categorization (ทริกเกอร์เบื้องหลัง, เกณฑ์ความเชื่อมั่น, ยอมรับ/แก้ไข/เพิกเฉย, การยอมรับแบบ bulk แบบ atomic)"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §6–§8; GLOSSARY.md "AI Categorization"/"Suggestion"; BR-007 ถึง BR-012) รวมถึงการตัดสินใจของ map นี้เองล้วนตกลงกันหมดแล้ว:

- ทำงานเบื้องหลังเมื่อบันทึกครั้งแรกที่มีเนื้อหาจริงเท่านั้น; ไม่เคยบล็อกการสร้าง Note; ไม่ทริกเกอร์ซ้ำโดยอัตโนมัติเมื่อมีการแก้ไข
- สร้าง Suggestion แบบจัดอันดับ (อาจรวมถึงชื่อ Category ใหม่เอี่ยม)
- หากต่ำกว่าเกณฑ์ความเชื่อมั่น จะไม่แสดงผลใด ๆ (ตั๋ว 009)
- ผู้ใช้สามารถยอมรับบางส่วน / แก้ไข / เพิกเฉยได้; การยอมรับเป็นธุรกรรมแบบ atomic เดียว (ตั๋ว 010)
- เมื่อได้รับการแก้ไขแล้ว (ยอมรับ/เพิกเฉย) จะไม่ย้าย Note โดยอัตโนมัติอีกต่อไป (User Ownership, ตั๋ว 025)

ยังไม่มีสิ่งใดอยู่ในโค้ดเลย — ยืนยันแล้วจากการตรวจสอบ ADR-0004 (ตั๋ว 014): ไม่มี outbox, ไม่มี BackgroundService, ไม่มี Gemini client ถูก block อยู่จนกว่า Module Notes (ตั๋ว 020 สำหรับ `NoteCreated`) และกลไก Domain Event (ตั๋ว 012) จะมีอยู่จริง

## Finding

ยังไม่เริ่มดำเนินการ

## Resolution

Build เสร็จและ verify ครบวงจรแล้ว (2026-09-24) — เฉพาะฝั่ง backend (client UI ยังไม่ทำ) นี่คือ ticket ที่ซับซ้อนที่สุดเท่าที่ทำมา เพราะมีทั้ง background worker, external AI provider, และ atomic transaction ข้าม 3 schema เป็นครั้งแรกของ repo

**Trigger (BR-008/010, ticket 033)**: `Notes.AiCategorizationTriggeredAt` (nullable) ตัดสินใจโดย `NotesService` เอง — trigger ตอน `CreateAsync`/`UpdateAsync` ที่ Content ไม่ว่างเปล่าเป็นครั้งแรกเท่านั้น (รองรับกรณีสร้าง Note ว่างแล้วมาเติมเนื้อหาทีหลัง) publish `NoteReadyForCategorization` แค่ครั้งเดียวต่อ Note ตลอดชีวิต — เป็นการตัดสินใจ cross-cutting เดียวที่ครอบ BR ทั้งหมดเรื่อง "ทำงานครั้งเดียว"

**Outbox + BackgroundService (ADR-0004)**: module `AiCategorization` (schema `ai_categorization`) ใหม่ — `AiCategorizationOutboxWriterHandler` subscribe `NoteReadyForCategorization` เขียนแถว outbox แบบ transactional; `AiCategorizationOutboxWorker` (`BackgroundService`) poll ทุก 5 วินาที (ปรับได้ผ่าน config, test ใช้ 1 วินาที) เรียก `IAiProvider`, retry สูงสุด 5 ครั้งต่อแถวก่อนเลิกพยายาม

**AI provider (Gemini)**: `GeminiAiProvider` เรียก REST `generateContent` ของ Gemini 2.5 Flash-Lite แบบ structured JSON output (`responseSchema`) อยู่หลัง `IAiProvider` ตาม ADR-0004 — เปลี่ยน provider ทีหลังคือแค่เปลี่ยน DI registration ApiKey ไม่ hardcode ในซอร์ส: อ่านผ่าน `dotnet user-secrets` ตอน dev (`appsettings.Development.json`/`appsettings.json` ถูก commit เข้า git จริง จึงห้ามใส่ key จริงไว้ที่นั่น) เพิ่ม `appsettings.Example.json` ใหม่เป็น template ที่ยอมรับได้ตาม security rule ของ repo

**Confidence threshold (ticket 009, 035)**: ตาราง `ai_categorization.Settings` แถวเดียว seed ค่าเริ่มต้น 0.6 ผ่าน migration — ถ้า candidate อันดับ 1 ต่ำกว่านี้ ไม่สร้าง Suggestion เลย (ไม่ใช่สร้างแบบว่างเปล่า)

**Suggestion matching**: candidate ชื่อที่ตรงกับ Category เดิมของ user (case-insensitive) ถูก map เป็น `CategoryId`; ที่ไม่ตรงเก็บเป็น `NewCategoryName` แบบ string ดิบ (ticket 034) ผ่าน read model `CategoryProjection` ของ module นี้เอง (sync จาก `CategoryCreated`/`CategoryRenamed`/`CategoryDeleted` — เป็น cross-module read model ตัวที่ 3 ของ repo ต่อจาก Topic/Category)

**Bulk-accept แบบ atomic ข้าม 3 schema (ticket 010)** — จุดสถาปัตยกรรมที่สำคัญที่สุดของ ticket นี้: `SuggestionsService.AcceptAsync` เปิด `NpgsqlConnection` ของตัวเองแยกจาก DI แล้วสร้าง `NotesDbContext`/`OrganizationDbContext`/`AiCategorizationDbContext` ชุดใหม่ผูกกับ connection เดียวกัน แชร์ transaction เดียวกันจริง (`UseTransactionAsync`) เพื่อให้ "สร้าง Category ใหม่ + แนบเข้า Note + resolve Suggestion" สำเร็จหรือ rollback พร้อมกันทั้งหมด — จุดนี้เรียก logic ของ Notes/Organization โดยตรงข้าม module (ไม่ผ่าน event) ซึ่งขัดกับ convention ปกติของ ADR-0002 โดยตั้งใจ เพราะ ADR เองยอมรับว่าขอบเขตนี้ "บังคับใช้ด้วย code review ไม่ใช่ compiler" และ synchronous atomic transaction ไม่ใช่ use case ที่ fire-and-forget event ออกแบบมารองรับ — publish `CategoryCreated`/`NoteCategoryAttached` **หลัง** commit สำเร็จเท่านั้น เพื่อไม่ให้ event handler ที่ต่อ connection แยกเขียนข้อมูลไปก่อนที่เรารู้ว่า transaction จะ commit จริงหรือไม่

**Testing**: `FakeAiProvider` แทน Gemini จริงใน test (deterministic, ไม่มี network call จริง), poll interval ลดเหลือ 1 วินาทีเฉพาะ test host เพิ่ม integration test 8 เคสใหม่ (`AiCategorizationTests.cs`) ครอบคลุม suggestion เกิดขึ้นจริง, confidence ต่ำไม่แสดงอะไรเลย, แก้ไข Note ไม่ trigger ซ้ำ, accept แบบ atomic สร้าง Category+แนบสำเร็จ, accept ที่ตรงกับ Category เดิมนำกลับมาใช้ซ้ำ, accept ด้วย candidate id ปลอมทำให้ทั้ง request ล้มเหลว (ไม่แนบอะไรเลย), ignore resolve โดยไม่แนบอะไร, unauthorized — รวม test เดิมผ่านครบ **58/58**, `dotnet format --verify-no-changes` ผ่าน

**ไม่ได้ทำใน scope นี้ (ตั้งใจ)**: endpoint แก้ไข confidence threshold (ticket 035 ระบุไว้แล้วว่ายังไม่ต้องมี UI), client UI ทั้งหมด

**Update (2026-09-24, ระหว่าง audit ticket 025)**: พบว่าพลาด endpoint on-demand re-trigger ที่ GLOSSARY.md/ARCHITECTURE.md ระบุไว้ ("ผู้ใช้สามารถเรียก action 'Suggest Category' ด้วยตัวเองได้ทุกเมื่อ") — เพิ่ม `POST /api/notes/{id}/suggestion/trigger` แล้ว (`INotesService.TriggerCategorizationAsync`, ไม่แตะ `AiCategorizationTriggeredAt` เพราะเป็นคนละกลไกกับ auto-trigger ครั้งเดียว) `AiCategorizationOutboxWorker` แก้ให้ resolve suggestion ที่ active อยู่ก่อนสร้างอันใหม่ (รองรับ "trigger ซ้ำได้" โดยไม่ชน unique constraint "หนึ่ง Note มี suggestion active ได้อย่างมากหนึ่งชุด") เพิ่ม test ยืนยันแล้ว รวม test ผ่านครบ **59/59**

**Client UI (2026-09-25)**: build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง client — module ใหม่ `client/src/modules/ai-categorization/` (types/services/hooks/components ตาม pattern เดิมของ notes/organization) `NoteAiSuggestionPanel` ถูก mount เข้าไปใน `NoteCard` แสดงผลตามสถานะจริงของ suggestion ต่อ Note หนึ่งๆ: ไม่มี suggestion active → ปุ่ม "Suggest category" (เรียก on-demand trigger endpoint); หลัง trigger (`202 Accepted`) → poll `GET .../suggestion` ทุก 3 วินาทีจนกว่าจะเจอ suggestion หรือครบ timeout 30 วินาที (เพราะ background worker เป็น async และกรณี confidence ต่ำกว่าเกณฑ์จะไม่มี suggestion โผล่มาเลยตลอดไป ต้องมี timeout ไม่ใช่ poll ค้าง); มี suggestion active → แสดง candidate เป็น toggle chip (preselect ทั้งหมด, โชว์ % confidence, ต่อท้าย "(new)" ถ้าเป็น `NewCategoryName`) พร้อมปุ่ม Accept selected (bulk-accept แบบ partial ตาม ticket 010) และ Ignore แยกกัน verify ผ่าน browser จริงกับ backend จริงครบวงจร: trigger → poll → (Gemini key ในเครื่อง dev ใช้งานไม่ได้จริง เจอ `Gemini request failed with status NotFound` ในทุกแถวของ `ai_categorization.OutboxEntries` — ไม่เกี่ยวกับโค้ด client, เป็นปัญหาเดียวกับ known issue เรื่อง Gemini API key ที่ hardcode ไว้ผิดที่) seed แถว `Suggestions`/`SuggestionCandidates` ตรงในฐานข้อมูลเพื่อพิสูจน์ path การแสดงผล+accept(partial)+ignore แทน แล้ว verify ว่า accept แนบเฉพาะ candidate ที่เลือกจริง (Category ที่ไม่ได้เลือกไม่ถูกสร้าง), ignore resolve โดยไม่แนบอะไรเลย, และ suggestion กลับสู่สถานะ idle ("Suggest category") ถูกต้องหลังทั้งสอง action

**Update (2026-09-26)**: แก้ known issue เรื่อง Gemini API key ที่ไม่ทำงาน — ไม่ใช่ปัญหา key เลย (key valid, `generateContent` ยิงตรงด้วย `curl` สำเร็จ) แต่ model `gemini-2.5-flash-lite` ถูก Google deprecate ไปแล้วสำหรับ key ใหม่ ("no longer available to new users") เปลี่ยน `Gemini:Model` เป็น `gemini-3.5-flash-lite` ใน `appsettings.json`/`appsettings.Development.json`/`appsettings.Example.json` ทั้งหมด แล้ว verify กับ Gemini จริงผ่าน browser สำเร็จ ได้ suggestion จริงกลับมา (เช่น note เรื่อง auth middleware bug ได้ `Engineering (new) 95%`, `Bug Fix (new) 85%`, `test (existing) 30-40%`)

ระหว่าง verify รอบนี้เจอ **bug จริงของ client UI** (ไม่เกี่ยวกับ Gemini): auto-trigger (ตอน save Note ครั้งแรก) กับ on-demand trigger (กดปุ่ม "Suggest category" ทันทีหลัง save) แข่งกัน — on-demand trigger resolve suggestion เดิมแล้วสร้างอันใหม่ (ตาม design เดิมของ update 2026-09-24) แต่ `NoteAiSuggestionPanel` ยัง render candidate ids ของ suggestion เก่าที่ resolve ไปแล้วค้างอยู่ (รอบ poll ถัดไปยังไม่ทัน refetch) กด Accept แล้วส่ง id เก่าไป ได้ error "One or more candidate IDs are invalid." ถูกต้องจาก backend แต่ client ไม่ refetch ให้เห็น suggestion ใหม่ที่มีอยู่แล้วจริง ค้างเป็น error เฉยๆ แก้โดยให้ `useAcceptSuggestion` ([useAcceptSuggestion.ts](../../../client/src/modules/ai-categorization/hooks/useAcceptSuggestion.ts)) invalidate suggestion query ใน `onSettled` (ทั้ง success และ fail) แทนที่จะทำแค่ตอน success — verify ซ้ำแล้วว่า reload/refetch แสดง suggestion ล่าสุดถูกต้อง และ accept แบบ partial สำเร็จจริง (สร้าง Category ใหม่ 2 ตัว + แนบเข้า Note ถูกต้อง ตัวที่ deselect ไม่ถูกแนบ)

**Update (2026-09-26, security)**: ย้าย Gemini `ApiKey` ออกจาก `appsettings.Development.json` แล้ว (เหลือแค่ `Model`) ตามที่ comment ใน `GeminiOptions.cs` ระบุไว้แต่แรกว่าห้าม hardcode ในไฟล์ที่ commit เข้า git จริง — ผู้ใช้ตั้งค่าใหม่ผ่าน `dotnet user-secrets set "Gemini:ApiKey" "<key>" --project backend/Web.Jotwai` แทน (`UserSecretsId` มีอยู่แล้วใน `Web.Jotwai.csproj`) key เก่ายังอยู่ใน git history จากหลาย commit ก่อนหน้า ควร rotate/revoke ที่ Google AI Studio ด้วย เพราะการลบออกจากไฟล์ปัจจุบันไม่ได้ลบออกจาก history
