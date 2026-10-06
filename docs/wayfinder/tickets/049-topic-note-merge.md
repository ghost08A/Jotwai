---
id: 49
title: "Topic และ Note เป็น Entity เดียวกันหรือไม่"
type: wayfinder:grilling
status: closed
assignee: voramatep
parent: ../map-v2.md
blocked_by: []
---

## Question

ปัจจุบัน Topic 1 : 1 Note และ UX มอง Topic/Note เป็นหน้าเดียวกัน ควรรวมเป็น Entity เดียวหรือไม่ (`requirements-v2.md` §4)

## Resolution

เจ้าของโปรเจกต์ตัดสิน (2026-10-05): **หัวข้อ (Topic) กับโน้ต (Note) คือสิ่งเดียวกัน — รวมเป็น Entity เดียว**

ตัดสินแค่นี้เท่านั้น ผลกระทบต่อของเดิม (Category, Note ที่ไม่มี Topic, Trash/soft delete, Search, flow ของ AI, การเป็นเจ้าของของ Module ฯลฯ) **ยังไม่ตัดสิน** — แยกเป็น ticket ผลกระทบ
