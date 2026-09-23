---
id: 26
title: "สร้าง: Search (เนื้อหารวมถึง code block, Topic, Category — ค้นหาแบบ substring ไม่สนตัวพิมพ์เล็ก-ใหญ่)"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §10–§11; GLOSSARY.md "Search"; BR-014 ถึง BR-016) รวมถึง ADR-0006 ตัดสินใจแล้วว่า: การจับคู่แบบ substring ที่ไม่สนตัวพิมพ์เล็ก-ใหญ่ ครอบคลุมเนื้อหาของ Note (รวมถึงเนื้อหาภายใน code block), ภาษาที่ระบุของ code block, Topic และ Category ผ่านทาง Postgres `ILIKE` + ดัชนี `pg_trgm` GIN บน read model ของ Search Module เอง การกรองตาม Topic ต้นทางจะรวม Topic ลูกหลานด้วย ไม่มี semantic/vector search สำหรับ MVP (เป็น non-goal อย่างชัดเจน) ยังไม่มีสิ่งใดอยู่ในโค้ด — ยืนยันแล้วจากการตรวจสอบ ADR-0006 (ตั๋ว 015) ถูก block อยู่จนกว่า Module Notes/Topic/Category จะมีอยู่จริง (read model ต้องมีสิ่งที่จะ project มาจาก) และกลไก Domain Event (ตั๋ว 012)

## Finding

ยังไม่เริ่มดำเนินการ
