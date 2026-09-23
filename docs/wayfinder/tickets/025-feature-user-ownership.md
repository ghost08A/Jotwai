---
id: 25
title: "สร้าง: การบังคับใช้ User Ownership (ไม่มีการจัดหมวดหมู่หรือย้ายซ้ำโดยอัตโนมัติหลังจากจัดระเบียบครั้งแรก)"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: [020-feature-note-capture-crud.md, 022-feature-topic-hierarchy.md, 023-feature-category-flat.md, 024-feature-ai-categorization-flow.md]
---

## Question

Product spec (Requirements v0.1 §8–§9; GLOSSARY.md "User Ownership") ตัดสินใจแล้วว่า: เมื่อ Topic/Category ของ Note ถูกกำหนดแล้ว (โดยผู้ใช้หรือโดยการยอมรับ Suggestion ของ AI) ระบบจะไม่ย้ายหรือจัดหมวดหมู่ใหม่โดยอัตโนมัติอีกเลย — การแก้ไขเนื้อหาจะไม่ทริกเกอร์ AI ซ้ำ และมีเพียงการกระทำที่ผู้ใช้สั่งอย่างชัดเจนเท่านั้นที่จะเปลี่ยน Topic/Category ได้ นี่ไม่ใช่ subsystem แยกต่างหากที่ต้องสร้างขึ้น แต่เป็น invariant ที่ Notes/Topic/Category/AI Categorization ทั้งหมดต้องปฏิบัติตาม — ถูกติดตามเป็นตั๋วของตัวเองเพื่อไม่ให้ถูกมองข้ามอย่างเงียบ ๆ ในขณะที่ส่วนเหล่านั้นถูกสร้างขึ้นทีละส่วน (เช่น อย่าเชื่อมต่อ "เมื่อมีการอัปเดต Note" ให้รัน AI Categorization ซ้ำโดยไม่ตั้งใจ)

## Finding

ยังไม่เริ่มดำเนินการ — ยังไม่มีโค้ดสำหรับส่วนใด ๆ ที่ invariant นี้กำกับควบคุมอยู่เลย
