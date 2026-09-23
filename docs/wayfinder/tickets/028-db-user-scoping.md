---
id: 28
title: ตัดสินใจว่า Topic/Category ผูกกับ User หรือไม่ (multi-tenancy ของ schema organization/notes)
type: wayfinder:grilling
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

GLOSSARY.md เขียนไว้ว่า "Category names are globally unique" ถ้าตีตรงตัวคือ unique ข้ามทุก User ในระบบทั้งหมด (คนละคนสร้าง Category ชื่อ "Work" ซ้ำกันไม่ได้เลย) ซึ่งดูขัดกับกฎ User Ownership (ticket 025) ที่บอกว่าข้อมูลถูก scope ต่อเจ้าของ คำว่า "globally" ในที่นี้น่าจะหมายถึง "unique ในผัง flat ทั้งหมดของ User คนนั้น" (เทียบกับ Topic ที่ unique แค่ในกลุ่มพี่น้อง) ไม่ใช่ unique ข้าม User จริงๆ — แต่ยังไม่มีเอกสารไหนยืนยันชัดเจน

คำถามนี้กำหนดว่า `Notes`, `Topics`, `Categories` ทุกตารางต้องมีคอลัมน์ `UserId` ไหม และ unique constraint ต้องเป็น composite (`UserId + Name`) หรือ single column — เป็น decision ที่กระทบทุกตารางของ 3 module ที่ยังไม่ได้ build (Notes, Organization, และโดยอ้อมคือ AI Categorization/Search)

ตัวเลือก:
1. ผูกกับ User ทั้งหมด (ทุกตารางมี `UserId`, unique constraint เป็น `(UserId, Name)`)
2. Global จริงๆ ข้าม User ทั้งระบบ (unique constraint เป็น column เดียว ไม่มี `UserId` ร่วม)

**คำแนะนำ**: ตัวเลือก 1 — แอปนี้เป็นสมุดโน้ตส่วนตัว ไม่มีที่ไหนพูดถึงการแชร์ข้อมูลข้าม User เลย (เป็น explicit non-goal ด้วยซ้ำ) การให้ User คนอื่นชนชื่อ Category กันจะแปลกมาก
