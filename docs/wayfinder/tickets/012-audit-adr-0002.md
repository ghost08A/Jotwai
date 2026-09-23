---
id: 12
title: "ตรวจสอบการ implement: ADR-0002 In-process event/mediator communication between modules"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/12
---

## Question

โค้ดเบสปัจจุบัน implement การตัดสินใจที่บันทึกไว้ใน `docs/adr/0002-in-process-event-communication.md` หรือไม่? ค้นหา dependency ของ mediator/MediatR หรือสิ่งที่เขียนขึ้นเองในลักษณะเดียวกัน, ชนิด/คลาสของ domain event ใดๆ, event handler ใดๆ, และการเชื่อมต่อ pub/sub ใน Program.cs หรือ DI setup

## Finding (ยังไม่ได้ implement)

ไม่มี package ของ mediator (MediatR หรืออื่นใด) ถูกอ้างอิงใน `.csproj` ไฟล์ใดเลย — `Jotwai.Domain.csproj` ไม่มีการอ้างอิง package ใดๆ, `Jotwai.Service.csproj` อ้างอิงเฉพาะ package ของ EF Core/Npgsql/Identity/JWT เท่านั้น การ grep หา `MediatR`, `IEventHandler`, `IDomainEvent`, `INotification`, `DomainEvent`, และชื่อ event เฉพาะจาก ARCHITECTURE.md (`NoteCreated`, `TopicRenamed`, `CategoryCreated`/`Renamed`/`Deleted`) ทั่ว `backend/` ไม่พบผลลัพธ์ใดๆ

**ขาด**: ทุกอย่าง — dependency ของ mediator, ชนิด Domain Event เอง, event handler, และการลงทะเบียน DI/pub-sub ใน `Program.cs` ยังไม่มีโครงสร้างพื้นฐานใดๆ อยู่เลย ต้องสร้างขึ้นใหม่ทั้งหมดเมื่อ module Notes เสร็จสมบูรณ์

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่
