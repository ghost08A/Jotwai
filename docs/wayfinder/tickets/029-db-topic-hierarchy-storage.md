---
id: 29
title: ตัดสินใจกลยุทธ์เก็บ Topic hierarchy ใน schema organization
type: wayfinder:grilling
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Topic ซ้อนชั้นได้ไม่จำกัด และต้อง query "Topic นี้กับลูกหลานทั้งหมด" ได้ (ต้องใช้ตอน filter search ตาม ticket 026 ด้วย) — แต่ไม่มีเอกสารไหนตัดสินใจว่าจะเก็บโครงสร้างนี้ในฐานข้อมูลยังไง

ตัวเลือก:
1. **Adjacency list** (`ParentId` self-FK ธรรมดา) — เขียนง่ายที่สุด แต่ query "เอาลูกหลานทั้งหมด" ต้องใช้ recursive CTE ทุกครั้ง
2. **Closure table** (ตารางแยกเก็บทุกคู่ ancestor-descendant) — query ลูกหลานเร็วมาก (join ตรงๆ) แต่เขียน/อัปเดตซับซ้อนกว่าตอนย้าย/ลบ node และมีต้นทุนพื้นที่เก็บ
3. **Materialized path / Postgres `ltree`** — query เร็ว, Postgres รองรับ extension `ltree` ให้ในตัว แต่ผูกกับ Postgres-specific feature และแก้ path ทั้งสายเวลาย้าย node ที่มีลูกหลาน

**คำแนะนำ**: ตัวเลือก 1 (Adjacency list) — scope ตอนนี้ยังไม่มี requirement เรื่อง performance ที่ต้อง optimize ขนาดนั้น (โน้ตส่วนตัว ไม่ใช่ enterprise-scale) และ `ParentId` เขียน/แก้ง่ายที่สุดตอนย้าย Topic — เพิ่ม closure table ทีหลังได้ถ้าจำเป็นจริงๆ

**หมายเหตุ**: คำตอบของ ticket นี้กำหนดว่า module Search จะ implement การ filter ตาม Topic-ลูกหลานยังไง — ต้อง resolve ticket นี้ก่อนถึงจะ grill รายละเอียด schema ของ Search ต่อได้

## Resolution

เลือกตัวเลือก 1 — Adjacency list `Topics.ParentId` เป็น self-FK nullable (root topic มี `ParentId = null`) การ query "ลูกหลานทั้งหมด" ใช้ recursive CTE ตอน implement module Search/filter ไม่ต้องมี closure table หรือ `ltree` เพิ่มความซับซ้อนตอนนี้
