---
id: 50
title: "Research: แนวทางป้องกัน stale write / แก้ข้อมูลพร้อมกัน"
type: wayfinder:research
status: closed
assignee: null
parent: ../map-v2.md
blocked_by: []
---

## Question

แนวทางมาตรฐานที่ใช้กันตรวจจับการบันทึกทับข้อมูลที่ไม่ใช่ล่าสุด (เปิดหลาย Tab/อุปกรณ์) ในบริบท ASP.NET Core + EF Core + PostgreSQL + SPA editor (Tiptap) มีอะไรบ้าง (เช่น version/ETag/If-Match, EF concurrency token, timestamp comparison, conflict UX, merge) ข้อเท็จจริงของแต่ละแบบ ข้อดีข้อเสีย และผลต่อ UX — **รายงานข้อเท็จจริงเท่านั้น ห้ามเลือกแทนเจ้าของโปรเจกต์** เก็บที่ `docs/wayfinder/research/concurrency-patterns.md`

## Finding

รวบรวมเสร็จ (2026-10-05) ที่ [research/concurrency-patterns.md](../research/concurrency-patterns.md) — ข้อเท็จจริงล้วน ไม่เลือกแทนเจ้าของโปรเจกต์ ครอบคลุม EF Core concurrency token (DB-generated เช่น `xmin` บน Npgsql / app-managed), `DbUpdateConcurrencyException` และการ resolve, isolation level, HTTP ETag/If-Match/412/428, If-Unmodified-Since, และ merge/CRDT (Yjs, Tiptap Collaboration)

ข้อจำกัดของ research: ข้อมูลบางส่วนอ่านผ่านสรุปย่อ ไม่ใช่ข้อความเต็ม; ยังไม่ได้ตรวจ ETag/If-Match ที่ built-in ใน ASP.NET Core .NET 10, ขีดจำกัดของ fetch keepalive และไลเซนส์ Tiptap Collaboration; ข้อควรรู้: `sendBeacon` ตั้ง header (เช่น If-Match) ไม่ได้และจำกัด 64 KiB
