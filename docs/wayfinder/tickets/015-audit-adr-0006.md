---
id: 15
title: "ตรวจสอบการ implement: ADR-0006 Postgres substring search, no external search engine"
type: wayfinder:research
status: closed
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/15
---

## Question

โค้ดเบสปัจจุบัน implement การตัดสินใจที่บันทึกไว้ใน `docs/adr/0006-search-postgres-substring.md` หรือไม่? ค้นหา module/controller ของ Search, การใช้งาน ILIKE, migration ของ pg_trgm/GIN index, และ entity ของ read-model สำหรับ search

## Finding (ยังไม่ได้ implement)

**Re-audit 2026-09-25** (commit c87243b): แม้ Notes/Organization/AI Categorization module จะ build ครบแล้ว (migration มีอยู่จริงใน `Migrations/Notes/`, `Migrations/Organization/`, `Migrations/AiCategorization/`) แต่ module Search ยังไม่ถูกแตะเลย — ไม่มี `SearchDbContext`, ไม่มี schema `search`, ไม่มี extension `pg_trgm`, ไม่มี GIN index, ไม่มีไฟล์ `SearchController`/`*Search*` อยู่ที่ไหนใน `backend/` เลย การ grep ทั่ว repo หาคำว่า `ILIKE`, `pg_trgm`, `GIN`, `Search` พบเพียงคอมเมนต์เดิมที่กล่าวถึง module ในอนาคตใน `AuthController.cs` เท่านั้น ไม่ใช่โค้ดจริง — สอดคล้องกับ finding เดิมของการ audit ครั้งก่อน (2026-09-22) และของ [ticket 026 (feature-search)](026-feature-search.md) ที่ยัง block อยู่

**ขาด**: entity read-model + DbContext ของ Search Module, migration สำหรับเปิดใช้งาน `pg_trgm`/GIN index, ตรรกะ query แบบ `ILIKE`, `SearchController`, และ event handler ที่ทำ projection การเปลี่ยนแปลงของ Note/Topic/Category เข้าสู่ read model

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่ block โดย [ticket 026](026-feature-search.md) (product spec ของ Search feature) ซึ่งเป็นงานที่ต้อง build จริง ไม่ใช่ audit เพิ่ม

## Resolution

ปิดแล้ว (2026-09-25) — build ครบผ่าน [ticket 026](026-feature-search.md): module `Search` (schema `search`) implement ตรงตาม decision ของ ADR-0006 เป๊ะ — `ILIKE` substring query บน read model ของตัวเอง หนุนด้วย `pg_trgm` GIN trigram index บน `SearchDocuments.Content`, ครอบคลุมเนื้อหา Note (รวม code block เพราะเป็นส่วนหนึ่งของ Content), ชื่อ Topic และ Category ของ Note, และ Topic filter รวม descendant ทั้งหมด ไม่มี external search engine/semantic search ตาม MVP scope client UI ยังไม่ทำ
