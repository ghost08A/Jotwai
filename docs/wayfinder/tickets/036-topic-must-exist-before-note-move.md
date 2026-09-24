---
id: 36
title: ตัดสินใจว่าการย้าย Note ไปยัง Topic ที่ยังไม่มีอยู่จะสร้าง Topic นั้นแบบ inline ได้ไหม
type: wayfinder:grilling
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ticket 022 (Topic hierarchy) ระบุไว้เองว่าประเด็นนี้ต้องถูก grill แยกต่างหากก่อนหรือระหว่างสร้างงาน: การย้าย Note ไปยัง Topic ผ่าน endpoint ยอมให้ระบุชื่อ Topic ที่ยังไม่มีอยู่จริงแล้วสร้างให้อัตโนมัติในคำขอเดียวกันได้ไหม หรือ Topic ต้องถูกสร้างไว้ก่อนเสมอผ่าน endpoint สร้าง Topic แยกต่างหาก

ตัวเลือก:
1. **ต้องมีอยู่ก่อนเท่านั้น** — endpoint ย้าย Note รับแค่ `TopicId` ที่มีอยู่แล้ว
2. **สร้างแบบ inline ได้** — endpoint รับได้ทั้ง `TopicId` หรือชื่อ Topic ใหม่

## Resolution

เลือกตัวเลือก 1 — endpoint ย้าย Note (`PUT /api/notes/{id}/topic`) รับเฉพาะ `TopicId` ที่มีอยู่แล้วเท่านั้น ต้องสร้าง Topic ผ่าน `POST /api/topics` ก่อนเสมอ สร้างและย้ายในขั้นตอนเดียวกันสามารถเพิ่มทีหลังได้ถ้าต้องการลด friction จริง แต่ไม่ใช่ตอนนี้
