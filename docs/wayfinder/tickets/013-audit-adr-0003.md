---
id: 13
title: "ตรวจสอบการ implement: ADR-0003 PostgreSQL, schema-per-module, and event-synced read models"
type: wayfinder:research
status: closed
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

## Update (2026-09-23)

`NotesDbContext` ถูกสร้างแล้วพร้อม ticket 020 — `HasDefaultSchema("notes")`, migration ของตัวเอง, ตาม pattern เดียวกับ `IdentityDbContext` เป๊ะ (พิสูจน์ pattern ซ้ำเป็นครั้งที่สอง) ยังคง**เปิดไว้**: `OrganizationDbContext`/`AiCategorizationDbContext`/`SearchDbContext` ยังไม่มี และรูปแบบ read-model สำหรับอ่านข้ามโมดูล (ส่วนสำคัญของ ADR-0003) ยังไม่เคยถูกพิสูจน์เลยแม้แต่ครั้งเดียว เพราะยังไม่มี 2 module ที่ต้องอ่านข้อมูลของกันและกันจริงๆ — จะพิสูจน์ได้ครั้งแรกตอน Search (ticket 026) หรือ AI Categorization (ticket 024) ถูก build

## Resolution

ปิดแล้ว (2026-09-24) — [ticket 022](022-feature-topic-hierarchy.md) สร้าง `OrganizationDbContext` (schema `organization`) เป็น schema ที่ 3 ตาม pattern เดียวกัน และพิสูจน์รูปแบบ read-model ข้าม module เป็นครั้งแรกจริงของ repo นี้: Notes เก็บ `KnownTopic` read model ของ Topic ที่มีอยู่ (sync จาก `TopicCreated`/`TopicDeleted`) และ Organization เก็บ `Topic.NoteCount` read model ของจำนวน Note ต่อ Topic (sync จาก `NoteTopicAssigned`) ทั้งสองทิศทางไม่มีการ query ข้าม schema เลย ยืนยันด้วย integration test และ curl end-to-end ยังเหลือ `AiCategorizationDbContext`/`SearchDbContext` ที่ยังไม่ build — ติดตามที่ ticket 024/026 ต่อไป ไม่ใช่ ticket audit นี้อีกต่อไป เพราะ pattern หลักของ ADR-0003 (schema-per-module + read-model) พิสูจน์ครบทั้งสองด้านแล้ว
