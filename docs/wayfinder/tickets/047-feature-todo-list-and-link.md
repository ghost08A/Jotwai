---
id: 47
title: "เพิ่ม To-do list (checkbox) และ Link ใน editor"
type: wayfinder:task
status: open
assignee: null
parent: ../map.md
blocked_by: [46]
---

## Question

แยกมาจาก [ticket 046](046-feature-topic-page-notion-layout.md) (2026-09-30) ผู้ใช้อนุญาตให้เพิ่ม dependency และเปิด Link แล้ว:

- To-do list: `@tiptap/extension-task-list` + `task-item` (Markdown `- [ ]` ต้อง round-trip ผ่าน `tiptap-markdown` ให้ได้), เพิ่มรายการ "To-do" ใน slash menu
- Link: ตอนนี้ปิดไว้ใน `NoteEditor.tsx` (`link: false`) — เปิด, ต้องตัดสินใจ UX การใส่ URL (เช่น ปุ่มใน bubble menu / วางลิงก์ทับข้อความ) และความสัมพันธ์กับ Topic Reference node (ticket 042) ที่ตั้งใจแยกจาก link mark
- อัปเดต GLOSSARY.md "Note" ที่ระบุ content type ที่รองรับ

## Resolution

(ยังไม่ได้ทำ)
