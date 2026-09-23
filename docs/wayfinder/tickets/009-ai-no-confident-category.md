---
id: 9
title: ตัดสินใจพฤติกรรมของ AI เมื่อไม่มี Category ใดมั่นใจเพียงพอ
type: wayfinder:grilling
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/9
---

## Question

ขั้นตอนการทำงานของ AI Categorization ใน ARCHITECTURE.md ระบุว่าจะ "ผลิต Suggestion ที่จัดลำดับไว้เสมอ" แต่ไม่ได้ครอบคลุมกรณีที่ Note สั้นเกินไปหรือกำกวมเกินกว่าที่ candidate Category ใดจะควรค่าแก่การแสดง ระบบควร (a) แสดง suggestion แบบ best-effort เสมอไม่ว่าความมั่นใจจะเท่าใด (b) แสดงชุด suggestion ที่ว่างเปล่าอย่างชัดเจน หรือ (c) ใช้ค่า threshold ความมั่นใจที่หากต่ำกว่านั้นจะไม่แสดงอะไรเลย

## Resolution

**การตัดสินใจ**: (c) — ใช้ค่า threshold ความมั่นใจ หากความมั่นใจของ candidate ที่ดีที่สุดจาก AI Categorization ต่ำกว่าค่า threshold ที่ตั้งค่าได้ ระบบจะไม่แสดง Suggestion ให้ผู้ใช้เห็นเลย (ไม่ต้องสร้าง UI สำหรับสถานะชุดว่างเปล่า)

เหตุผล: ช่วยหลีกเลี่ยงการแสดง suggestion ที่มีคุณค่าต่ำจนรก UI โดยไม่ต้องสร้าง UI ใหม่สำหรับ "ไม่มี suggestion"
