---
id: 21
title: "สร้าง: Code Block พร้อมข้อมูลภาษาที่เป็นตัวเลือก"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §3) ตัดสินใจแล้วว่า: เนื้อหาของ Note สามารถมี code block ได้ โดยเลือกระบุภาษาได้ (C#, JavaScript, TypeScript, Python, SQL, Java ฯลฯ); block ที่ไม่ได้ระบุภาษาถือเป็นข้อความ/โค้ดทั่วไป ตาม ADR-0008 Tiptap เป็นผู้จัดหาตัวเลือกภาษา โดยทั้ง block และภาษาของมันเป็นส่วนหนึ่งของเนื้อหา Markdown และทั้งคู่สามารถค้นหาได้ (ตั๋ว 026)

ยังไม่มี dependency ของ Tiptap เลย (`client/package.json` ไม่มีแพ็กเกจ `@tiptap/*` — ยืนยันแล้วจากการตรวจสอบ ADR-0008, ตั๋ว 017) ดังนั้นงานนี้ถูก block อยู่จนกว่า editor พื้นฐานจะมีอยู่จริง

## Finding

Blocker ปลดล็อกแล้ว: base editor (Tiptap, ticket 017) และ Note capture/CRUD (ticket 020) build เสร็จทั้งคู่ตั้งแต่ 2026-09-25

## Resolution

Re-audit เมื่อเริ่มงาน (2026-09-26) พบว่า acceptance criteria ของ ticket นี้ถูกปิดไปแล้วเป็นผลพลอยได้จากงานอื่นที่เสร็จก่อนหน้า — ไม่ต้องเขียนโค้ด feature ใหม่ ยืนยันด้วยหลักฐานที่มีอยู่จริง:

- **Code block พร้อมเลือกภาษา**: `NoteEditorToolbar.tsx` มีปุ่ม toggle code block + `<select>` ภาษาที่ผูกกับ `codeBlock` node attribute ของ Tiptap ผ่าน `updateAttributes` (C#, JavaScript, TypeScript, Python, SQL, Bash, JSON, HTML, CSS) — **เพิ่ม Java เข้า list วันนี้** (ตรงตาม product spec ที่ระบุชื่อไว้ชัดเจน) verify ผ่าน browser จริง (เลือกภาษา Java, พิมพ์โค้ด, render ถูกต้อง)
- **Block ที่ไม่ระบุภาษา = ข้อความ/โค้ดทั่วไป**: ตัวเลือกว่าง ("Plain text") เป็นค่า default ตรงตาม decision ของ spec ข้อนี้เป๊ะ
- **เนื้อหาและภาษาเป็นส่วนหนึ่งของ Markdown**: `tiptap-markdown` serialize code block เป็น fenced code block พร้อม language (` ```csharp `) — พิสูจน์แล้วจาก `NotesTests`/`SearchTests` ที่ backend ที่ใช้ content แบบนี้ตรงๆ
- **ค้นหาได้ทั้ง block และภาษา**: `SearchTests.Search_matches_note_content_case_insensitively` สร้าง Note ด้วย content `` ```csharp\nvar x = 1;\n``` `` แล้วค้นหาคำใน code block เจอจริง (ILIKE substring match ครอบคลุมทั้ง fence และเนื้อหาโค้ด เพราะ Content เป็น text ธรรมดา)
- **ผูกกับ Note ตัวจริงผ่าน API**: `NoteEditor`/`NoteCard`/`NotesPage` ใช้ editor เดียวกันนี้อยู่แล้วสำหรับ capture/edit ของ Note จริง (ticket 020) — ไม่มี gap เหลือ

ปิด ticket — ไม่มีหนี้เชิง implementation เหลือสำหรับ feature นี้
