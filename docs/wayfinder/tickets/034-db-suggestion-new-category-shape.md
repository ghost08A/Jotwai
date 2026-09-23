---
id: 34
title: ยืนยันว่า Suggestion เก็บ Category ที่ยังไม่มีอยู่จริงเป็นชื่อ string ดิบๆ
type: wayfinder:grilling
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ticket 010 บอกว่า bulk-accept "สร้าง + attach Category ทั้งหมดพร้อมกัน" ซึ่งดูเหมือนจะบอกเป็นนัยว่า Category ใหม่จาก AI ยังไม่ถูกสร้างเป็นแถวจริงจนกว่าจะถูก accept (เก็บแค่ชื่อ string ไว้ใน Suggestion ก่อน) แต่ไม่มีเอกสารไหนยืนยันรูปร่าง Suggestion table ชัดเจน

ต้องการให้ยืนยัน: candidate ที่เป็น Category ใหม่ให้เก็บเป็น**ชื่อ string ดิบๆ** ใน Suggestion table (ไม่สร้างแถว Category จริงจนกว่าจะ accept) ใช่ไหม — ถ้าใช่ แปลว่า Suggestion table ต้องมีทั้ง nullable FK ไปยัง Category ที่มีอยู่แล้ว **และ** field string สำหรับชื่อใหม่ที่ยังไม่มีอยู่จริง (สองอย่างนี้ exclusive กัน: มีค่าใดค่าหนึ่งต่อ candidate หนึ่งแถว)

**คำแนะนำ**: ยืนยันตามที่อ่านได้จาก ticket 010 — ไม่สร้าง Category ล่วงหน้า ป้องกันไม่ให้ชื่อที่ AI คิดขึ้นเองแต่ผู้ใช้ไม่เคย accept ไปปรากฏใน namespace ของ Category จริง (ซึ่งจะผิดกฎ uniqueness ด้วย)
