---
id: 14
title: "ตรวจสอบการ implement: ADR-0004 Outbox + background worker for AI Categorization, using Gemini"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/14
---

## Question

โค้ดเบสปัจจุบัน implement การตัดสินใจที่บันทึกไว้ใน `docs/adr/0004-ai-categorization-background-job.md` หรือไม่? ค้นหาตาราง/entity ของ outbox, การ implement `BackgroundService`/`IHostedService`, client หรือ interface ของ Gemini/AI provider, และโค้ด module Notes หรือ AI Categorization ใดๆ ทั้งหมด

## Finding (ยังไม่ได้ implement)

การค้นหาทั่วทั้ง repo หาคำว่า `Outbox`, `Gemini`, `BackgroundService`, `IHostedService`, `Categoriz`, `Suggestion`, `Note`, `NoteCreated` พบเพียงไฟล์เดียว: `backend/Web.Jotwai/Controllers/AuthController.cs` และพบเพียงในรูปแบบคอมเมนต์ที่กล่าวถึง module ในอนาคตเท่านั้น ไม่ใช่การ implement จริงแต่อย่างใด migration ที่มีอยู่มีเพียง `InitialCreate` ของ Identity เท่านั้น

**ขาด**: module Notes (entity `Note`, domain event `NoteCreated`), ตาราง outbox พร้อมการเขียนข้อมูลแบบธุรกรรมเดียวกันตอนสร้าง note, ตัว poller แบบ `BackgroundService`/`IHostedService`, interface ของ AI provider พร้อม client ของ Gemini (2.5 Flash-Lite), domain model และการเก็บข้อมูลของ Category/Suggestion, การเชื่อมต่อ hosted service ใน `Program.cs`

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่
