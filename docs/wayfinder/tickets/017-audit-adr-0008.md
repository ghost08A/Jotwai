---
id: 17
title: "ตรวจสอบการ implement: ADR-0008 Frontend stack, styling, and note editor"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/17
---

## Question

โค้ดเบสปัจจุบัน implement การตัดสินใจที่บันทึกไว้ใน `docs/adr/0008-frontend-stack-and-editor.md` หรือไม่? อ่าน `client/package.json` เพื่อดู dependency และตรวจสอบ `client/src` ว่ามีโค้ดแอปจริง, การทำ routing, หรือ component ของ editor หรือไม่

## Finding (implement บางส่วนแล้ว)

ติดตั้งและใช้งานแล้ว: React 19, Vite, react-router-dom, TanStack Query, Tailwind — `main.tsx` เชื่อมต่อ `BrowserRouter` + `QueryClientProvider` ไว้แล้ว, `App.tsx` มี route ตัวอย่างเพียงหนึ่งเดียว `axios` เป็น dependency ที่ติดตั้งไว้แต่ยังไม่ได้ใช้งาน (ยังไม่มี API client/hook ใดอ้างอิงถึงมัน)

**ขาดไปทั้งหมด**: Tiptap (ไม่มี package `@tiptap/*` อยู่เลย), component library สไตล์ shadcn/ui ใดๆ (มีเพียง Tailwind class ดิบๆ เท่านั้นในตอนนี้), โค้ดฟีเจอร์ของ Note/Topic/Category ใดๆ, hook สำหรับดึงข้อมูลจริงใดๆ, editor ใดๆ

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่ เป็นเรื่องที่คาดไว้แล้ว: ฝั่ง frontend มีเพียงโครงสร้างเบื้องต้นเท่านั้นในตอนนี้
