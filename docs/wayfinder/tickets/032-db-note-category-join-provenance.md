---
id: 32
title: ตัดสินใจว่าตารางเชื่อม Note-Category ต้องเก็บที่มาไหม
type: wayfinder:grilling
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Category ผูกกับ Note แบบ many-to-many แน่นอน (ต้องมีตารางเชื่อม) แต่คำถามคือตารางนี้ควรรู้ไหมว่า Category ตัวนี้ถูกติดเข้า Note "ด้วยมือผู้ใช้" หรือ "มาจาก AI suggestion ที่ผู้ใช้ accept"

ตัวเลือก:
1. **เก็บที่มา** — เพิ่มคอลัมน์เช่น `Source` (Manual/AiSuggested) ใน join table — เปิดทางให้ทำ analytics/UI แยกแสดงว่าอันไหนมาจาก AI ในอนาคต
2. **ไม่เก็บ** — join table มีแค่ `NoteId + CategoryId` เพียวๆ

**คำแนะนำ**: ตัวเลือก 2 (ไม่เก็บตอนนี้) — ไม่มี requirement ไหนขอ feature "แสดงที่มาของ Category" เลย เพิ่มทีหลังได้ถ้ามีคนต้องการจริงๆ (ตรงกับกฎ "อย่า over-engineer" ใน `docs/agents/backend.md`)

## Resolution

เลือกตัวเลือก 2 — join table `NoteCategories` มีแค่ `NoteId + CategoryId` ไม่เก็บ `Source`
