---
id: 18
title: "ตรวจสอบการนำไปใช้จริง: ADR-0010 โครงสร้างโปรเจกต์แบบสามชั้นระดับ Global ให้สอดคล้องกับธรรมเนียมของบริษัท"
type: wayfinder:research
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/18
---

## Question

โค้ดเบสปัจจุบันนำการตัดสินใจที่บันทึกไว้ใน `docs/adr/0010-global-three-tier-layering.md` ไปใช้จริงหรือไม่? ให้ตรวจสอบ ProjectReferences ในไฟล์ .csproj ทั้งสามไฟล์ และแสดงรายการโครงสร้างโฟลเดอร์จริงภายใต้ backend/Jotwai.Domain, backend/Jotwai.Service, backend/Web.Jotwai แต่ละโปรเจกต์

## Finding (ทำตามแล้ว)

กราฟการอ้างอิงโปรเจกต์ตรงกันทุกประการ: `Jotwai.Domain` ไม่มีการอ้างอิงโปรเจกต์อื่นเลย; `Jotwai.Service` อ้างอิงเฉพาะ `Jotwai.Domain`; `Web.Jotwai` อ้างอิงทั้งสองโปรเจกต์ โครงสร้างโฟลเดอร์ในแต่ละโปรเจกต์ตรงกับธรรมเนียมการแบ่งชั้นทางเทคนิคตามที่ ADR กำหนด (`Interfaces/`, `Models/`, `RequestModels/`, `ValidateModel/`, `ViewModels/` ใน Domain; `ImplementServices/`, `Helper/`, `Configurations/`, `Migrations/` ใน Service; `Controllers/` ใน Web) ไม่มีการแบ่งโฟลเดอร์ย่อยตาม Module ซึ่งถูกต้องตาม ADR นี้ โฟลเดอร์ `Enums/` ไม่มีอยู่เพียงเพราะยังไม่มี enum ใด ๆ เกิดขึ้น ไม่ใช่ความคลาดเคลื่อนจากแผน

ยังไม่มี Module อื่นเพิ่มเติมที่จะพิสูจน์ว่ารูปแบบนี้ใช้ได้ในระดับใหญ่ขึ้น แต่สำหรับทุกอย่างที่สร้างขึ้นแล้วในปัจจุบัน (Identity) โครงสร้างตรงกับที่ตัดสินใจไว้ทุกประการ
