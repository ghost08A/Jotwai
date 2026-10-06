---
id: 51
title: "Research: ตัวเลือกที่เก็บรูปภาพ/ไฟล์แนบ"
type: wayfinder:research
status: closed
assignee: null
parent: ../map-v2.md
blocked_by: []
---

## Question

ตัวเลือกการเก็บไฟล์แนบสำหรับแอปแบบ ASP.NET Core + PostgreSQL ที่ deploy แยก static SPA (local disk, S3-compatible object storage, เก็บใน Postgres ฯลฯ) ข้อเท็จจริงเรื่อง key/URL อ้างอิง, การแสดง inline, การ download, การลบ, การจำกัดขนาด/ชนิด, การเข้าถึงแบบมีสิทธิ์ (รวมกรณีผู้รับ Share) ข้อดีข้อเสีย — **รายงานข้อเท็จจริงเท่านั้น ห้ามเลือกแทนเจ้าของโปรเจกต์** เก็บที่ `docs/wayfinder/research/attachment-storage.md`

## Finding

รวบรวมเสร็จ (2026-10-05) ที่ [research/attachment-storage.md](../research/attachment-storage.md) — ข้อเท็จจริงล้วน ไม่จัดอันดับ ไม่เลือกแทนเจ้าของโปรเจกต์ เปรียบเทียบ 4 ตัวเลือก (local disk, Postgres `bytea`, Postgres Large Object, S3-compatible) ประเด็นที่ตัดสินเรื่องอื่นต่อ: `<img src>` ใส่ Authorization header ไม่ได้ จึงต้องใช้ cookie / token ใน URL (เช่น presigned URL, อายุสูงสุด 7 วัน และเป็น bearer token) / endpoint สาธารณะ; static file middleware ไม่ตรวจสิทธิ์เอง; ขีดจำกัดขนาดต่างกันตามตัวเลือก (Kestrel default ~30 MB, `bytea` 1 GB)

ข้อจำกัดของ research: ข้อมูลดึงผ่านเครื่องมือสรุปหน้าเว็บ ไม่ใช่อ่านต้นฉบับทีละบรรทัด; ช่องว่างที่ยังไม่ยืนยัน (หัวข้อ 9 ของไฟล์): Large Object, EF Core กับ `bytea`, ราคา AWS S3, การสแกนไวรัส, ลักษณะ deploy จริงของโปรเจกต์
