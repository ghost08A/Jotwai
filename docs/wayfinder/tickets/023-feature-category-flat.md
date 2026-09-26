---
id: 23
title: "สร้าง: โครงสร้าง Category แบบเรียบ (สร้าง, ความไม่ซ้ำกันทั่วทั้งระบบ, หลาย Category ต่อหนึ่ง Note)"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

Product spec (Requirements v0.1 §5; GLOSSARY.md "Category"; BR-005, BR-006) ตัดสินใจแล้วว่า: Category เป็นโครงสร้างแบบเรียบ (ไม่มีการซ้อนกันใน MVP) ชื่อไม่ซ้ำกันทั่วทั้งระบบแบบไม่สนตัวพิมพ์เล็ก-ใหญ่ หนึ่ง Note สามารถมีได้หลาย Category สร้างได้อย่างอิสระโดยผู้ใช้หรือถูกเสนอโดย AI ยังไม่มีสิ่งใดอยู่ในโค้ด (Module Organization ยังไม่ถูกสร้าง — การตรวจสอบ ADR-0001, ตั๋ว 011)

## Finding

ยังไม่เริ่มดำเนินการ ไม่มีเอนทิตี Category, DbContext หรือ UI อยู่ที่ใดในโปรเจกต์เลย

## Resolution

Build เสร็จและ verify ครบวงจรแล้ว (2026-09-24) — เฉพาะฝั่ง backend (client UI ยังไม่ทำ ไม่อยู่ใน scope ของ ticket นี้) reuse pattern เดียวกับ [ticket 022 (Topic)](022-feature-topic-hierarchy.md) แต่ตัดส่วนที่ Category ไม่มี (ไม่มี hierarchy, ไม่มี delete guard เพราะ spec ไม่ได้ขอ):

- `Jotwai.Domain`: `Models/Category.cs` (`Id`, `UserId`, `Name`/`NormalizedName` — unique ทั่วระบบต่อ user ตาม decision 028, ไม่มี `ParentId`/`NoteCount` เพราะไม่ต้อง), `Events/CategoryCreated.cs`/`CategoryRenamed.cs`/`CategoryDeleted.cs`, `Events/NoteCategoryAttached.cs`/`NoteCategoryDetached.cs`, `Models/KnownCategory.cs` (read model ของ Notes), `Models/NoteCategory.cs` (join table many-to-many ไม่เก็บที่มา ตาม decision 032), `Interfaces/ICategoriesService.cs`, extend `INotesService` ด้วย `AttachCategoryAsync`/`DetachCategoryAsync`, extend `NoteResponse` ด้วย `CategoryIds`
- **Cross-module read model ทิศทางเดียว** (ต่างจาก Topic ที่ต้องมีสองทิศทางเพราะมี delete-guard): Notes subscribe `CategoryCreated`/`CategoryDeleted` เพื่อรักษา `KnownCategories` ใช้ validate ตอน attach — Organization ไม่ต้องรู้จำนวน Note ต่อ Category เพราะไม่มี delete-guard ให้บังคับ; เมื่อ Category ถูกลบ handler ฝั่ง Notes (`NotesKnownCategoriesProjectionHandler`) จะลบแถว `NoteCategories` ที่ค้างอยู่เองด้วย (auto-detach) เพราะ Organization มองไม่เห็น join table นี้เลย
- `Jotwai.Service`: `Categories` DbSet ใน `OrganizationDbContext` (schema เดิม, migration ใหม่), `CategoriesService` (create/list/get/rename/delete — delete ไม่มี guard ลบได้แม้มี Note ติดอยู่), `NotesService.AttachCategoryAsync`/`DetachCategoryAsync` (แบบ idempotent — attach/detach ซ้ำไม่ error), แก้ N+1 โดย batch query `NoteCategories` ตอน list หลาย Note พร้อมกัน
- `Web.Jotwai`: `CategoriesController` (`POST/GET/PUT/DELETE /api/categories`), `NotesController` เพิ่ม `POST/DELETE /api/notes/{id}/categories/{categoryId}`
- Integration test 11 เคสใหม่ (`CategoriesTests.cs`) ครอบคลุม duplicate-name, cross-user ชื่อซ้ำได้, rename ชนชื่อ, delete แม้มี note ติดอยู่, attach หลายอัน, attach ซ้ำ idempotent, attach ของไม่มีจริง 400, detach, และ auto-detach ตอน category ถูกลบ — รวม test เดิมผ่านครบ **50/50**, `dotnet format --verify-no-changes` ผ่าน
- Verify end-to-end จริงผ่าน curl: duplicate 409, attach หลาย category, attach ของไม่มีจริง 400, ลบ category ที่มี note ติดอยู่สำเร็จ (204) แล้ว note นั้น auto-detach เอง, detach เอง — ครบทุก use case

**Client UI (2026-09-25)**: build เสร็จและ verify ครบวงจรจริงแล้วผ่าน browser ต่อ backend จริง —

- `modules/organization/components/CategoriesPage.tsx` + `CategoryRow.tsx` (list แบบเรียบ ไม่มี hierarchy, create/rename/delete) เพิ่ม route `/categories` + nav link ใหม่ใน `PrivateLayout`
- `modules/notes/components/NoteCategoryChips.tsx` — chip toggle ต่อ Category (ไม่ใช่ multi-select) เพราะ Category ผูกกับ Note แบบ many-to-many ต่างจาก Topic ที่เป็น single-value คลิกเพื่อ attach/detach ทันที ใช้งานได้บนมือถือ (ไม่ต้อง ctrl/cmd-click แบบ native multi-select)
- `modules/notes/hooks/useAttachCategory.ts`/`useDetachCategory.ts` เพิ่ม `attachCategory`/`detachCategory` ใน `modules/notes/services/notesService.ts`, `useDeleteCategory` invalidate query ของ Notes ด้วย (เพราะลบ category จะ auto-detach ฝั่ง backend)
- Verify ผ่าน browser จริงกับ backend จริงครบ: สร้าง 2 category, attach เข้า note, reload ยืนยัน persist, detach สำเร็จ
- `npm run lint` + `npm run build` ผ่านสะอาด
