---
id: 10
title: ตัดสินใจความหมายเชิงพฤติกรรมของการยอมรับ Category suggestion แบบกลุ่ม
type: wayfinder:grilling
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/10
---

## Question

GLOSSARY.md ระบุว่าผู้ใช้ "สามารถยอมรับส่วนใดส่วนหนึ่ง" ของ candidate Category ใน Suggestion ได้ เมื่อผู้ใช้ยอมรับหลายรายการพร้อมกัน — บางส่วนเป็น Category ที่มีอยู่แล้ว บางส่วนเป็นชื่อใหม่ที่ AI เสนอขึ้นมา — นี่ควรเป็นธุรกรรมแบบ atomic เดียว (สร้างและแนบทั้งหมด สำเร็จหรือล้มเหลวพร้อมกัน) หรือแต่ละ Category สามารถถูกยอมรับ/แนบแยกจากกันได้

## Resolution

**การตัดสินใจ**: เป็นธุรกรรมแบบ atomic เดียว การยอมรับส่วนใดส่วนหนึ่งของ candidate Category ใน Suggestion — ไม่ว่าจะเป็น Category ที่มีอยู่แล้วหรือชื่อใหม่ที่ AI เสนอ — จะสร้าง Category ใหม่ที่จำเป็นและแนบ Category ที่ยอมรับทั้งหมดเข้ากับ Note ในธุรกรรมเดียว: สำเร็จหรือล้มเหลวพร้อมกันทั้งหมด ไม่มีสถานะกึ่งสำเร็จ

เหตุผล: สอดคล้องกับแนวทาง "บันทึก Note ทันที" ที่ถือว่าการกระทำของผู้ใช้เป็นขั้นตอน atomic เดียว
