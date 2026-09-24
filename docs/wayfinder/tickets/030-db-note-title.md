---
id: 30
title: ตัดสินใจว่า Note มี Title แยกเป็นคอลัมน์หรือไม่
type: wayfinder:grilling
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ไม่มีเอกสารไหนพูดถึง field "Title" ของ Note เลย (GLOSSARY พูดแค่ "freeform content... stored as Markdown") ตอนนี้ยังไม่รู้ว่า list "All Notes" ในหน้าแรกจะโชว์อะไรเป็นหัวเรื่องให้ผู้ใช้เห็น

ตัวเลือก:
1. **มี `Title` แยกเป็นคอลัมน์** — ผู้ใช้กรอกเอง หรือ derive จากบรรทัดแรก
2. **ไม่มี Title เลย** — list แสดงตัวอย่างข้อความต้นๆ ของ `Content` แทน (เหมือน Apple Notes/Google Keep)

**คำแนะนำ**: ตัวเลือก 2 (ไม่มี Title แยก ใช้ preview ของ content แทน) — "capture first" คือหัวใจของ product นี้ การบังคับ/แนะนำให้ตั้งชื่อจะขัดกับ flow "จดเร็วๆ" แต่นี่เป็นการเดาจาก product philosophy เท่านั้น ไม่ใช่สิ่งที่มีคนตัดสินใจไว้แล้ว

## Resolution

เลือกตัวเลือก 2 — Note ไม่มีคอลัมน์ `Title` แยก list "All Notes" แสดง preview ของ `Content` (Markdown) แทน
