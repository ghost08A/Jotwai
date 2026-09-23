---
id: 11
title: "ตรวจสอบการ implement: ADR-0001 Modular monolith with extractable module boundaries"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/11
---

## Question

โค้ดเบสปัจจุบัน implement การตัดสินใจที่บันทึกไว้ใน `docs/adr/0001-modular-monolith.md` (ADR-0001: Modular monolith with extractable module boundaries) หรือไม่? อ่าน ADR ให้ครบถ้วน จากนั้นตรวจสอบ `backend/` (และ `client/` ในส่วนที่เกี่ยวข้อง) ว่าตรงกันหรือไม่ รายงานผล: implemented / partially implemented / not implemented พร้อมอ้างอิงไฟล์ และหากเป็น partial/not ให้ระบุให้ชัดเจนว่าขาดอะไรไปบ้าง

## Finding (implement บางส่วนแล้ว)

มีเพียง Identity module เท่านั้นที่มีอยู่ในโค้ด (แบ่งโปรเจกต์แบบ global 3-tier เดียว ตรงกับ ADR-0010) มี Domain models (`Jotwai.Domain/Models/User.cs`, `RefreshToken.cs`, `RefreshTokenIssuance.cs`), service (`AuthService.cs`, `TokenService.cs`), `IdentityDbContext` (จำกัด schema เป็น `identity`), migration, และ `AuthController` ครบถ้วน ไม่มีโครงสร้าง `DomainEvent`/`IEventHandler`/MediatR อยู่ที่ไหนใน `backend/` เลย — กลไก Domain Event แบบ in-process ที่ ADR นี้อาศัยเพื่อกำหนดขอบเขตของ module ยังไม่ถูกสร้างขึ้น และยังไม่มี module ที่สองให้ทดสอบขอบเขตนี้ด้วย

**ขาด**: module Notes, Organization, AI Categorization, และ Search (ไม่มี models/DbContext/service/controller สำหรับ module ใดเลย); กลไก Domain Event เอง (ติดตามแยกต่างหากในการตรวจสอบของ ADR-0002 ticket 012)

ยังคงเปิดไว้ — นี่คือหนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่ กลับมาดูอีกครั้งเมื่อ module Notes เสร็จสมบูรณ์
