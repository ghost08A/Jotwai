---
id: 20
title: "สร้าง: การบันทึกและ CRUD ของ Note (สร้างได้โดยไม่ต้องมี Topic/Category, แก้ไขแบบเขียนทับ, ลบแบบ soft delete + ถังขยะ)"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §2, §9; GLOSSARY.md "Note"; การตัดสินใจเรื่องถังขยะในตั๋ว 008) ตัดสินใจครบถ้วนแล้วสำหรับการบันทึก Note ยังไม่มีสิ่งใดอยู่ในโค้ดเลย (ยืนยันแล้ว: ไม่มี model/controller ของ `Note` อยู่ที่ใดใน `backend/`, ไม่มี UI ของ Note ใน `client/src`) สิ่งที่ต้องสร้าง:

- การสร้าง Note โดยไม่ต้องมี Topic/Category บังคับ บันทึกทันที (BR-001, BR-002)
- เนื้อหาจัดเก็บเป็น Markdown; การแก้ไขจะเขียนทับ (ไม่มีประวัติเวอร์ชัน)
- Soft delete + มุมมองถังขยะ/Bin ที่ตัดสินใจไว้ (รายการ + ปุ่มกู้คืน, ตั๋ว 008)

นี่คืองานสร้าง (build task) ไม่ใช่การตัดสินใจที่ค้างอยู่ — ถูกติดตามที่นี่ในฐานะ backlog ตามธรรมเนียมของ map นี้เรื่องหนี้เชิงการนำไปใช้จริง ขึ้นอยู่กับการมี Module/schema ของ Notes อยู่ก่อน (ดูการตรวจสอบ ADR-0001/0003, ตั๋ว 011/013) และกลไก Domain Event (ตั๋ว 012) เพื่อให้ `NoteCreated` สามารถสั่งให้ AI Categorization ทำงาน (ตั๋ว 024)

## Finding

ยังไม่เริ่มดำเนินการ ไม่มีเอนทิตี `Note`, DbContext, controller หรือ UI ฝั่ง client สำหรับ Note อยู่ที่ใดในโปรเจกต์เลย
