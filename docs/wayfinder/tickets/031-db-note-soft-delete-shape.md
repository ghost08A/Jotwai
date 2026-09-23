---
id: 31
title: ตัดสินใจรูปร่างคอลัมน์ soft-delete ของ Note
type: wayfinder:grilling
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ticket 008 ตัดสินใจแล้วว่า Note ต้องมี Trash/Bin ที่ restore ได้ แต่ไม่ได้บอกว่าคอลัมน์หน้าตาเป็นยังไง

ตัวเลือก:
1. **`DeletedAt` (timestamp, nullable)** — `null` = ยังไม่ลบ, มีค่า = ลบแล้วตอนไหน (ได้ audit trail เรื่องเวลาฟรีๆ)
2. **`IsDeleted` (bool) + `DeletedAt` แยกกัน** — ซ้ำซ้อนกว่า

**คำแนะนำ**: ตัวเลือก 1 (`DeletedAt` เดี่ยวๆ พอ) — ไม่ต้องมี `IsDeleted` คู่กันเพราะเช็ค `DeletedAt IS NULL` ได้อยู่แล้ว ลด field ที่ต้อง sync กันเอง
