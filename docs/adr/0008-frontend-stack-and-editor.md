# ADR 0008: Frontend stack, การจัดสไตล์ และตัวแก้ไข Note

## สถานะ
ยอมรับแล้ว

## บริบท
`client/` ติดตั้ง React, TypeScript, Vite, `react-router-dom`, `@tanstack/react-query` และ `axios` ไว้อยู่แล้ว (พบใน `node_modules` แต่ยังไม่มี `package.json` ถูก commit เพื่อล็อกไว้) ยังไม่มี library สำหรับ styling/UI หรือ client-state library ใด ๆ และข้อกำหนดผลิตภัณฑ์เดิมยังเปิดกว้างไว้อย่างชัดเจนในเรื่อง "เทคโนโลยีตัวแก้ไข Note ที่แน่ชัด" และ "รูปแบบที่แน่ชัดของ rich text"

## การตัดสินใจ
- กำหนดให้ package ที่ติดตั้งไว้แล้วเป็นฐานของ frontend อย่างเป็นทางการ: **React + TypeScript + Vite**, **react-router-dom** (การทำ routing), **TanStack Query** (server state — การดึง/cache Note, Topic, Category), **axios** (HTTP)
- ไม่มี client-state library เฉพาะทาง; `useState`/Context จัดการ state ที่เกี่ยวกับ UI เท่านั้น (ตัวกรองที่ใช้งานอยู่, ฉบับร่างของตัวแก้ไข, สถานะ sidebar) — TanStack Query เป็นเจ้าของ server state อยู่แล้ว
- **Tailwind CSS** ร่วมกับ component แบบ headless สไตล์ shadcn/ui สำหรับการจัดสไตล์ — สร้าง UI แบบสมุดโน้ตที่ปรับแต่งเองได้อย่างรวดเร็ว โดยไม่ต้องยึดติดกับความเห็นด้านภาพของ component library ที่หนัก
- **Tiptap** (ProseMirror) เป็นตัวแก้ไข Note ตั้งค่าให้ serialize ไปมากับ **Markdown** (รูปแบบการจัดเก็บหลัก) ให้รายการแบบ WYSIWYG bulleted/numbered และตัวเลือกภาษาของ code block ในขณะที่เนื้อหาที่จัดเก็บยังคงเป็น Markdown ธรรมดา

## ผลที่ตามมา
- ต้อง (สร้างใหม่) `package.json` ให้ตรงกับสิ่งที่ติดตั้งไว้จริง พร้อม script ที่เหมาะสม
- Markdown ยังคงเป็นแหล่งความจริงเดียวของเนื้อหา Note ไม่ว่าตัวแก้ไขใดจะ render มันก็ตาม — การเปลี่ยนตัวแก้ไขในภายหลังไม่ต้องอาศัยการทำ data migration
- ไม่มีการผูกติดกับ CSS-in-JS หรือ component library ที่หนัก; Tailwind utility class เป็นชั้นการจัดสไตล์ตลอดทั้งระบบ
