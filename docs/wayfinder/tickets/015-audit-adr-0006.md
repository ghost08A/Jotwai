---
id: 15
title: "ตรวจสอบการ implement: ADR-0006 Postgres substring search, no external search engine"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/15
---

## Question

โค้ดเบสปัจจุบัน implement การตัดสินใจที่บันทึกไว้ใน `docs/adr/0006-search-postgres-substring.md` หรือไม่? ค้นหา module/controller ของ Search, การใช้งาน ILIKE, migration ของ pg_trgm/GIN index, และ entity ของ read-model สำหรับ search

## Finding (ยังไม่ได้ implement)

migration ที่มีอยู่มีเพียง `InitialCreate` ของ Identity เท่านั้น — ไม่มี extension `pg_trgm`, ไม่มี GIN index, ไม่มีตาราง read-model ของ Search ไม่มีไฟล์ `SearchController` หรือ `*Search*` อยู่ที่ไหนใน `backend/` เลย การ grep ทั่ว repo หาคำว่า `ILIKE`, `pg_trgm`, `GIN`, `Search` พบเพียงคอมเมนต์เดิมที่กล่าวถึง module ในอนาคตใน `AuthController.cs` เท่านั้น ไม่ใช่โค้ดจริง

**ขาด**: entity read-model + DbContext ของ Search Module, migration สำหรับเปิดใช้งาน `pg_trgm`/GIN index, ตรรกะ query แบบ `ILIKE`, `SearchController`, และ event handler ที่ทำ projection การเปลี่ยนแปลงของ Note/Topic/Category เข้าสู่ read model

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่
