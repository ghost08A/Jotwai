---
id: 13
title: "ตรวจสอบการ implement: ADR-0003 PostgreSQL, schema-per-module, and event-synced read models"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/13
---

## Question

โค้ดเบสปัจจุบัน implement การตัดสินใจที่บันทึกไว้ใน `docs/adr/0003-schema-per-module-and-read-models.md` หรือไม่? ตรวจสอบ DbContext, การตั้งค่า EF Core, migration, และ docker-compose.yml ว่ามีรูปแบบ schema-per-module และ read-model หรือไม่

## Finding (implement บางส่วนแล้ว)

มีเพียง `IdentityDbContext` (`backend/Jotwai.Service/IdentityDbContext.cs`) ที่ใช้ `HasDefaultSchema("identity")` อย่างถูกต้อง โดย migration ของมันกำหนด schema ให้ทุกตารางครบถ้วน `docker-compose.yml` จัดเตรียม Postgres 16 เพียงอินสแตนซ์เดียว สอดคล้องกับการตัดสินใจให้ใช้ฐานข้อมูลเดียว ยังไม่มี `NotesDbContext`/`OrganizationDbContext`/`AiCategorizationDbContext`/`SearchDbContext` และยังไม่มีโค้ด read-model/event-sync สำหรับการอ่านข้ามโมดูล

**ขาด**: DbContext, schema, และ migration สำหรับ `notes`, `organization`, `ai_categorization`, `search`; รูปแบบ read-model สำหรับการอ่านข้ามโมดูลยังไม่ได้รับการพิสูจน์นอกเหนือจากกรณี schema เดียว ตัวรูปแบบเอง (DbContext ของตัวเอง + `HasDefaultSchema`) ถูกวางไว้อย่างถูกต้องแล้วสำหรับ Identity และควรนำไปใช้ต่อได้โดยตรง

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่
