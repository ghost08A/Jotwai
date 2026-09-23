---
id: 21
title: "สร้าง: Code Block พร้อมข้อมูลภาษาที่เป็นตัวเลือก"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: [020-feature-note-capture-crud.md]
---

## Question

Product spec (Requirements v0.1 §3) ตัดสินใจแล้วว่า: เนื้อหาของ Note สามารถมี code block ได้ โดยเลือกระบุภาษาได้ (C#, JavaScript, TypeScript, Python, SQL, Java ฯลฯ); block ที่ไม่ได้ระบุภาษาถือเป็นข้อความ/โค้ดทั่วไป ตาม ADR-0008 Tiptap เป็นผู้จัดหาตัวเลือกภาษา โดยทั้ง block และภาษาของมันเป็นส่วนหนึ่งของเนื้อหา Markdown และทั้งคู่สามารถค้นหาได้ (ตั๋ว 026)

ยังไม่มี dependency ของ Tiptap เลย (`client/package.json` ไม่มีแพ็กเกจ `@tiptap/*` — ยืนยันแล้วจากการตรวจสอบ ADR-0008, ตั๋ว 017) ดังนั้นงานนี้ถูก block อยู่จนกว่า editor พื้นฐานจะมีอยู่จริง

## Finding

ยังไม่เริ่มดำเนินการ ขึ้นอยู่กับ editor ของ Note (ตั๋ว 020) และการผนวก Tiptap ตาม ADR-0008 (ตั๋ว 017) ต้องเสร็จก่อน
