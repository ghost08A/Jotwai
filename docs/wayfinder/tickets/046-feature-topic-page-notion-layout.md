---
id: 46
title: "ปรับหน้าเขียน Topic ให้เป็น 'หน้ากระดาษ' แบบ Notion (layout + placeholder + Enter จากชื่อลงเนื้อหา)"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ผู้ใช้ขอให้หน้าจด (Topic Detail) เขียนได้ง่ายและมีรูปแบบเหมือน Notion ในฟังก์ชันพื้นฐาน (2026-09-30) — ออกแบบร่วมกันก่อน ผู้ใช้เลือก **แบบ 1 "หน้ากระดาษ"** และอนุญาตให้เพิ่ม dependency, ย้ายปุ่ม Delete เข้าเมนู `⋯`, และเปิด Link

ปัญหาที่วินิจฉัยได้จากโค้ด: พื้นที่เขียนว่างเปล่าไม่บอกว่าพิมพ์ได้/มีเมนู `/`, Enter ที่ชื่อไม่พาลงเนื้อหา, Topic ใหม่ไม่โฟกัสเนื้อหาให้, เนื้อหากว้างเต็มจอ, ปุ่ม Delete สีแดงและปุ่ม Polish ลอยแย่งสายตา, ข้อความสถานะเป็นไทยปนอังกฤษ

ผู้ใช้แจ้งว่าถ้าทำไม่จบในงานเดียวให้แบ่งเป็น ticket ทำทีละส่วน → ticket นี้ทำ layout + พฤติกรรมการเขียน ส่วน To-do list และ Link แยกไป [ticket 047](047-feature-todo-list-and-link.md)

## Resolution

Build เสร็จ (2026-09-30) ฝั่ง client อย่างเดียว เพิ่ม dependency `@tiptap/extension-placeholder`:

- `TopicDetailPage.tsx`: หน้ากระดาษกึ่งกลาง `max-w-3xl`, breadcrumb เล็กจาง ("Topics / …"), สถานะ "Saving…/Saved" เป็นอังกฤษ, ปุ่ม Delete ย้ายเข้าเมนู `⋯` (native `<details>` ไม่เพิ่ม dependency — ceiling: ไม่ปิดเมื่อคลิกนอกเมนู/กด Escape), ชื่อ Topic ใหญ่ขึ้น กด Enter แล้วเคอร์เซอร์ลงไปในเนื้อหา
- `NoteEditor.tsx`: placeholder "Write, or press '/' for commands…" (ผ่าน Tailwind arbitrary variant ตามกฎห้ามเขียน CSS เอง), autofocus เมื่อเอกสารว่างและแก้ไขได้, ตัวอักษรเนื้อหาใหญ่ขึ้น (`text-lg`) และสูงขั้นต่ำ `min-h-64`
- `ContentPolishButton.tsx`: จากปุ่มกลมลอย (fixed) เป็นปุ่มข้อความ "✨ Polish with AI" ใต้เอกสาร — ไม่ได้ย้ายเข้าเมนู `⋯` เพราะ content/`onApply` อยู่ใน state ของ editor ต้องยก state ขึ้นทั้งหน้าซึ่งจะ re-render ทุกตัวอักษร
- `TopicSuggestionPanel.tsx`: ปุ่ม "Suggest placement" เป็น ghost เงียบๆ แทน outline

Verify: `npx tsc -b`/`npm run lint`/`npm run build` ผ่าน; browser จริง (dark mode) — placeholder แสดง, กว้าง 768px, Enter จากชื่อลงเนื้อหา, `/` เปิด slash menu, `⋯` แสดง "Delete topic", ไม่มีปุ่มลอยเหลือ ยังไม่ได้ยืนยัน autofocus ด้วยตาเพราะ pane ไม่ได้อยู่ใน focus (`document.hasFocus()` = false), ไม่ได้เช็ก light/mobile, ยังไม่ได้ทดสอบกด Delete จริงผ่านเมนูใหม่
