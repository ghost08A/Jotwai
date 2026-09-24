---
id: 35
title: ตัดสินใจว่า confidence threshold ของ AI Categorization เก็บที่ไหน
type: wayfinder:grilling
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ticket 009 บอกว่ามี confidence threshold ที่ "ตั้งค่าได้" แต่ไม่บอกว่าใครตั้ง/เก็บที่ไหน

ตัวเลือก:
1. **App config** (`appsettings.json`) — ค่าเดียวทั้งระบบ, แก้ต้อง deploy ใหม่
2. **เก็บในฐานข้อมูล แบบ global setting** — แก้ได้โดยไม่ deploy แต่ต้องมีตาราง settings ใหม่
3. **เก็บต่อ User** — User ปรับความไวของตัวเองได้ ต้องมีคอลัมน์ผูกกับ Users

**คำแนะนำ**: ตัวเลือก 1 (App config) — ไม่มี requirement ไหนพูดถึง per-user tuning หรือ admin UI สำหรับปรับค่านี้เลย ทำเป็น DB-backed setting ตอนนี้คือ over-engineer ล่วงหน้า

## Resolution

เลือกตัวเลือก 2 (ขัดกับคำแนะนำ) — เก็บเป็น global setting ในฐานข้อมูล (ตาราง settings ของ schema `ai_categorization` เช่น คอลัมน์เดียวหรือ key-value แถวเดียว) ไม่ใช่ per-user และไม่ใช่ app config เพื่อให้แก้ค่าได้โดยไม่ต้อง deploy ใหม่ — ยังไม่มี UI สำหรับแก้ค่านี้ในตอนนี้ (ทำ endpoint/UI แยกทีหลังถ้าต้องการจริง)
