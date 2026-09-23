---
id: 8
title: ตัดสินใจเรื่อง UX การลบและกู้คืน Note
type: wayfinder:grilling
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/8
---

## Question

GLOSSARY.md ระบุว่า Note สามารถ soft-delete ได้ แต่ไม่ได้บอกว่าผู้ใช้จะเห็นขั้นตอนการกู้คืนหรือไม่ ผลิตภัณฑ์นี้ควรมีหน้าจอ Trash/Bin ที่ผู้ใช้สามารถดูรายการและกู้คืน Note ที่ถูก soft-delete ไว้ได้หรือไม่ หรือว่า soft-delete เป็นเพียงกลไกป้องกันภายในที่ไม่มีการกู้คืนให้ผู้ใช้เห็น

## Resolution

**การตัดสินใจ**: ใช่ — เปิดให้มีหน้าจอ Trash/Bin โดย Note ที่ถูก soft-delete จะยังคงแสดงรายการและกู้คืนได้โดยผู้ใช้ (ผ่านปุ่ม "Restore") แทนที่จะให้ soft-delete เป็นเพียงกลไกป้องกันภายในเท่านั้น

เหตุผล: โครงสร้างข้อมูลรองรับอยู่แล้ว (soft delete ตามที่ระบุใน GLOSSARY.md/ARCHITECTURE.md) และผู้ใช้แอปจดบันทึกมักคาดหวังว่าจะสามารถ undo การลบได้
