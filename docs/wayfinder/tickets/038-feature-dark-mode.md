---
id: 38
title: "สร้าง: Dark mode พร้อมปุ่มสลับ light/dark"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ผู้ใช้ขอ dark mode สำหรับ client ยืนยัน scope ระหว่าง grill: มีปุ่มสลับ light/dark เอง (ไม่ใช่ dark ล้วนตายตัว), จำค่าที่เลือกไว้ข้ามการ reload, default ตาม system preference ถ้ายังไม่เคยเลือก

## Finding

ยังไม่เริ่มดำเนินการ

## Resolution

Build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง client (2026-09-26)

**Dark palette**: เพิ่ม `.dark` class ใน [index.css](../../../client/src/index.css) ด้วยค่ามาตรฐานของ shadcn/ui (New York, neutral) — ใช้ token ชุดเดียวกับที่มีอยู่แล้ว (`--background`, `--foreground`, `--card`, ...) แค่ override ค่าใต้ `.dark` เท่านั้น ไม่ต้องแก้ component ไหนที่ใช้ token เหล่านี้อยู่แล้วถูกต้องตาม client/CLAUDE.md ("UI & Design System")

**State + persistence**: [ThemeContext.tsx](../../../client/src/shared/context/ThemeContext.tsx) (`ThemeProvider`/`useTheme`) ตาม pattern เดียวกับ `AuthContext.tsx` ที่มีอยู่แล้ว — เก็บค่าใน `localStorage` (`jotwai.theme`), default อ่านจาก `prefers-color-scheme` ถ้ายังไม่เคยตั้งค่า toggle เปลี่ยน `document.documentElement`'s `dark` class ตรงๆ (ไม่ผ่าน React re-render ของทั้งต้นไม้ เพื่อไม่ให้ theme switch ทำให้ทุก component re-render โดยไม่จำเป็น)

**กัน flash ตอน reload**: เพิ่ม inline script เล็กๆ ใน [index.html](../../../client/index.html) ที่รันก่อน React mount เพื่ออ่าน `localStorage`/system preference แล้วใส่ `dark` class ให้ `<html>` ทันที ก่อนที่ React จะ render อะไรเลย — ป้องกัน "กระพริบ" เห็น theme ผิดแวบเดียวตอนโหลดหน้า

**Toggle button**: [ThemeToggle.tsx](../../../client/src/shared/components/ThemeToggle.tsx) ใช้ `Button` component (headless library) ที่มีอยู่แล้ว, glyph ☀/🌙 แบบ plain text/emoji ตาม convention เดียวกับ toolbar icon อื่นๆ ในโปรเจกต์ (ไม่เพิ่ม icon library ใหม่) วางไว้ใน [PrivateLayout.tsx](../../../client/src/core/layouts/PrivateLayout.tsx) header ข้าง Log out

**แก้ hardcoded color ที่ bypass token**: พบว่า `NoteEditor.tsx`/`NoteEditorToolbar.tsx` (ticket 037) และ `Unauthorized.tsx` ใช้ Tailwind `neutral-*`/`bg-white`/`text-black` ตรงๆ แทน semantic token — ค่าพวกนี้ไม่ตอบสนองต่อ `.dark` class เลย ทำให้ editor toolbar/code block/blockquote/hr และหน้า session-expired จะดูผิดเพี้ยนถ้าเปิด dark mode แก้ทั้งหมดให้ใช้ token แทน (`bg-muted`, `border-border`, `text-muted-foreground`, `bg-accent`, `bg-primary`, ฯลฯ)

verify ผ่าน browser จริง: toggle สลับ light↔dark ถูกต้อง (ตรวจ `document.documentElement.classList` + computed background color ตรงกับค่า token ที่คาดไว้), ค่าที่เลือก persist ข้าม reload จริง (`localStorage`), editor toolbar/code block/blockquote ใน dark mode ใช้สีจาก token ถูกต้อง (ไม่ใช่ neutral-* เดิมที่ค้างสีเดิม)

**ไม่ได้ทำใน scope นี้ (ตั้งใจ)**: ปุ่ม toggle บน public page (login/register/Unauthorized) — theme (CSS) ใช้ได้ทั้งแอปอยู่แล้วเพราะเป็น global token แค่ปุ่มควบคุมไม่มีที่นั่น ไม่ได้ถูกขอ ไม่เพิ่มเพื่อเลี่ยง over-engineering

**Update (2026-09-26)**: ผู้ใช้ทดลองใช้แล้วพบว่า header กับพื้นหลัง "ดำเนียนไปด้วยกัน" มองไม่ออกว่าคนละส่วน — สาเหตุคือ `--border` เดิม (`14.9%`) มีค่าเท่ากับ `--muted` (`14.9%`) พอดี เส้นขอบ header เลยกลืนไปกับสีพื้น body ที่อยู่ติดกันสนิท แก้โดยปรับ `--border`/`--input` เป็น `22%` (เข้มขึ้นชัดเจน แยกจากทั้ง `--card` และ `--muted`) พร้อมเพิ่ม `shadow-sm` ให้ header ใน [PrivateLayout.tsx](../../../client/src/core/layouts/PrivateLayout.tsx) เสริมมิติอีกชั้น และผู้ใช้รายงานว่าตัวอักษรสีขาวจัดเกินไป ปรับ `--foreground`/`--card-foreground`/`--popover-foreground`/`--secondary-foreground`/`--accent-foreground`/`--destructive-foreground` จาก `98%` เป็น `92%` (สีนวลอมขาวแทนขาวล้วน ตาม pattern ที่หลาย design system ใช้) ไม่แตะ `--primary-foreground` (ใช้เป็นตัวหนังสือสีเข้มบนปุ่ม primary สีอ่อน ไม่เกี่ยวกับปัญหานี้) verify ผ่าน browser จริงว่า border/shadow เห็นชัดขึ้น และสีตัวอักษรนวลลงจริง
