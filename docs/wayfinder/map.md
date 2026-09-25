---
title: Jotwai Requirements v0.1 — แผนผังการตัดสินใจ
label: wayfinder:map
archived_from: https://github.com/ghost08A/Jotwai/issues/7
---

## Destination

Spec ที่ตัดสินใจแล้วและพร้อมให้ engineer ทำงานต่อ resolve พฤติกรรมจาก Requirements v0.1 ที่ ARCHITECTURE.md / GLOSSARY.md / docs/adr/ ยังไม่ตอบไว้ — แสดงเป็น ticket ที่ปิดแล้วบนแผนผังนี้ การเทียบเอกสาร requirements กับเอกสาร domain ที่มีอยู่ตอนสร้างแผนผังพบว่า "open question" เดิม 14 ข้อส่วนใหญ่มีคำตอบบันทึกไว้อยู่แล้ว (editor tech, การแทนค่า rich-text, multi-category suggestion, การสร้าง category จาก suggestion, ความสามารถแก้ไข Topic, กฎการจับคู่ของ search, pagination/sorting) สิ่งที่เหลือต้องตัดสินใจ: UX การลบ/กู้คืน Note, วิธีที่ AI Categorization จัดการ Note ที่ไม่มี Category ไหนมั่นใจพอ, และการ accept Category ที่ถูกเสนอหลายอันเป็น transaction เดียวหรือแยก action กัน

แผนผังนี้ยัง audit ADR ทุกตัวที่ยอมรับแล้วใน `docs/adr/` เทียบกับโค้ดเบสจริงด้วย เนื่องจาก implementation (Identity module, backend scaffolding บางส่วน) นำหน้า ADR บางตัวและล้าหลังกว่าตัวอื่น

## Notes

- เอกสาร domain ที่ต้อง consult: ARCHITECTURE.md, GLOSSARY.md, `docs/adr/`
- Non-goal ที่ถูกกำหนดไว้แล้วโดย CLAUDE.md / ARCHITECTURE.md (ห้ามหยิบมาทบทวนใหม่โดยไม่ปรึกษาผู้ใช้ก่อน): real-time/push infra, LLM semantic/vector search, การแชร์/ทำงานร่วมกันของ note, note version history, การแนบไฟล์/รูปภาพ, การแยกเป็น true microservices
- **Override ของ wayfinder scope เริ่มต้น**: แผนผังนี้มี `wayfinder:research` audit ticket หนึ่งตัวต่อหนึ่ง ADR ที่ยอมรับแล้ว โดยมีคำถามว่า "โค้ดเบส implement การตัดสินใจของ ADR นี้หรือไม่ ถ้าไม่ ขาดอะไรไปบ้าง?" Resolve โดยการอ่านโค้ดเบส; ถ้ายืนยันว่า implement แล้ว ticket จะถูกปิดและบันทึกไว้ด้านล่าง; ถ้าเจอ gap ticket จะยังคงเปิดอยู่ (ไม่มีคน assign) พร้อมบันทึก finding ไว้ — เป็น tracked implementation debt ไม่ใช่ decision ที่ต้องทบทวนใหม่
- ADR ที่ถูกแทนที่แล้ว (0005, 0009) ไม่ถูก audit — เนื้อหาของมันเป็นประวัติศาสตร์ ถูกแทนที่ด้วย ADR-0011 และ ADR-0010 ตามลำดับ
- **Tracker**: แผนผังนี้ใช้ local-markdown tracker (`docs/wayfinder/`) ไม่ใช่ GitHub Issues ตามคำสั่งชัดเจนของผู้ใช้ (2026-09-22) ที่ override ค่า default ของ repo นี้ (`docs/agents/issue-tracker.md`) เฉพาะสำหรับงาน wayfinder ดู section "Wayfinding operations" ของเอกสารนั้นสำหรับ convention ของ local-markdown แผนผังและ ticket ของมันเดิมทีถูกสร้างเป็น GitHub issue #7–#19 (ghost08A/Jotwai) และย้ายมาที่นี่โดยเก็บเนื้อหาไว้ครบถ้วน; แต่ละไฟล์ ticket ด้านล่างบันทึก GitHub URL เดิมไว้เพื่อการ archive

## Decisions so far

- [ตัดสินใจ UX การลบ & กู้คืน Note](tickets/008-note-deletion-recovery-ux.md): เปิด view Trash/Bin — Note ที่ถูก soft-delete แสดง list ได้และผู้ใช้กู้คืนเองได้ ไม่ใช่หายไปเงียบๆ
- [ตัดสินใจพฤติกรรม AI เมื่อไม่มี Category ไหนมั่นใจพอ](tickets/009-ai-no-confident-category.md): ใช้ confidence threshold — ต่ำกว่านั้นจะไม่แสดง Suggestion เลย
- [ตัดสินใจความหมายของ bulk-accept สำหรับ Category suggestion](tickets/010-bulk-accept-suggestions.md): เป็น transaction เดียวแบบ atomic — สร้าง + attach Category ที่ accept ทั้งหมดพร้อมกัน หรือไม่ทำเลยสักอัน
- [Audit implementation: ADR-0010 Global three-tier project structure](tickets/018-audit-adr-0010.md): ยืนยันว่า implement แล้ว — project reference และ folder layout ตรงกันเป๊ะสำหรับ Identity module (module เดียวที่ build แล้วตอนนี้)
- [Audit implementation: ADR-0011 Custom Identity entities with password hashing](tickets/019-audit-adr-0011.md): ยืนยันว่า implement แล้ว — `User`/`RefreshToken` POCO แบบ custom, hashing ผ่าน `IPasswordHasher` เท่านั้นพร้อม constant-time handling สำหรับ email ที่ไม่มีในระบบ, JWT ที่ออกเอง, และ refresh-token rotation แบบ atomic compare-and-swap ทั้งหมดมีอยู่จริงและมี test ครบ
- [Frontend login flow](tickets/027-feature-frontend-login.md): build เสร็จและ verify ครบวงจรจริงแล้ว (register → me → logout → login ผ่าน Docker Postgres + API + browser click-through) ระหว่างทางเจอและแก้บั๊ก CORS preflight ที่โดน `UseHttpsRedirection()` ขวางไว้ ด้วยการลบ https redirect ออกและเรียง middleware ใหม่เป็น `UseRouting → UseCors → UseAuthentication → UseAuthorization`
- [Note capture & CRUD](tickets/020-feature-note-capture-crud.md): build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง backend (2026-09-23) — `NotesDbContext`/service/controller ครบ, 8 integration test ใหม่ผ่าน (รวม 27/27), พร้อม mediator infra ของ ADR-0002 ที่สร้างขึ้นครั้งแรกในคราวนี้ ทำให้ [ADR-0001 audit](tickets/011-audit-adr-0001.md) และ [ADR-0002 audit](tickets/012-audit-adr-0002.md) ปิดตามไปด้วย **client UI build เสร็จเพิ่มเติมแล้ว (2026-09-25)** — capture-first composer + note list + inline edit (overwrite) + trash/restore ผ่าน `PrivateLayout` ใหม่ (`/` = Notes, `/trash` = Trash) verify ผ่าน browser จริงกับ backend จริงครบวงจร ใช้ base editor (ticket 017) และ `Button` component (headless library) ที่มีอยู่แล้ว
- [การย้าย Note ไป Topic ต้องมี Topic อยู่ก่อนไหม](tickets/036-topic-must-exist-before-note-move.md): ต้องมีอยู่ก่อนเท่านั้น — endpoint ย้าย Note ไม่รองรับสร้าง Topic แบบ inline
- [Topic hierarchy](tickets/022-feature-topic-hierarchy.md): build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง backend (2026-09-24) — `OrganizationDbContext`/service/controller ครบ, sibling-uniqueness แบบ case-insensitive, delete guard ทั้ง child topics และ notes, 13 integration test ใหม่ผ่าน (รวม 39/39) พิสูจน์ cross-module read model จริงเป็นครั้งแรกของ repo (Notes↔Organization ผ่าน `KnownTopic`/`NoteCount`) ทำให้ [ADR-0003 audit](tickets/013-audit-adr-0003.md) ปิดตามไปด้วย client UI ยังไม่ทำ
- [Category flat structure](tickets/023-feature-category-flat.md): build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง backend (2026-09-24) — reuse pattern ของ Topic ตัด hierarchy/delete-guard ออก, many-to-many join `NoteCategories` เป็นของ Notes เอง, auto-detach ตอน Category ถูกลบผ่าน event, 11 integration test ใหม่ผ่าน (รวม 50/50) client UI ยังไม่ทำ
- [AI Categorization suggestion flow](tickets/024-feature-ai-categorization-flow.md): build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง backend (2026-09-24) — module `ai_categorization` ใหม่ทั้งหมด: outbox + `BackgroundService` (ADR-0004), `GeminiAiProvider` อยู่หลัง `IAiProvider`, confidence threshold แบบ DB-backed (ticket 035), bulk-accept แบบ atomic transaction ข้าม 3 schema จริง (ticket 010) เป็นครั้งแรกของ repo, พบและแก้ gap เรื่อง on-demand re-trigger ระหว่าง audit ticket 025, 9 integration test ใหม่ผ่าน (รวม 59/59) ด้วย fake AI provider ไม่มี network call จริงใน test client UI ยังไม่ทำ
- [User Ownership enforcement](tickets/025-feature-user-ownership.md): audit เสร็จแล้ว (2026-09-24) — ยืนยันว่า invariant ถูกบังคับใช้อยู่แล้วโดยการออกแบบของ 020/022/023/024 ไม่ต้องสร้าง subsystem ใหม่ ตามที่ ticket ระบุไว้เอง พบ gap เดียว (on-demand re-trigger) ซึ่งแก้ไปพร้อมกับ ticket 024 แล้ว
- [Search](tickets/026-feature-search.md): build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง backend (2026-09-25) — module ใหม่ทั้งหมด: schema `search`, read model (`SearchDocument`/`SearchTopic`/`SearchCategory`/join) sync ผ่าน event handler ใหม่ 3 ตัวที่ subscribe event ของ Notes/Organization ที่มีอยู่แล้ว, `ILIKE` substring match หนุนด้วย `pg_trgm` GIN index (ADR-0006), Topic filter รวม descendant คำนวณจาก mirror ของตัวเอง พบและปิด gap ที่ `NotesService.RestoreAsync` ไม่เคย publish event (เพิ่ม `NoteRestored` ใหม่) ทำให้ [ADR-0006 audit](tickets/015-audit-adr-0006.md) ปิดตามไปด้วย 7 integration test ใหม่ผ่าน (รวม 66/66) client UI ยังไม่ทำ
- Base editor (Tiptap): build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง client (2026-09-25) — `modules/notes/components/NoteEditor.tsx` + `NoteEditorToolbar.tsx` ห่อ Tiptap v3 พร้อม `tiptap-markdown` (Markdown เป็น source of truth ตาม ADR-0008) StarterKit ถูก configure ให้เหลือเฉพาะ content type ที่ GLOSSARY.md "Note" ระบุ (plain text/bulleted/numbered list/code block พร้อม language selector) ปิด mark/node ที่ไม่อยู่ใน scope ทั้งหมด verify ผ่าน browser click-through จริง (toggle list, code block, เปลี่ยนภาษา, ตรวจ markdown output) ไม่ได้แตะ headless component library (shadcn/ui style) หรือ feature code ที่เรียก backend จริง — เหลือใน [ADR-0008 audit](tickets/017-audit-adr-0008.md)
- Headless component library (shadcn/ui style): build เสร็จและ verify ครบวงจรจริงแล้วฝั่ง client (2026-09-25) — `components.json` + path alias `@/*` + `cn()` helper (`shared/utils/cn.ts`) ตั้งค่าครั้งแรก, design token ประกาศครั้งเดียวใน `index.css` ผ่าน Tailwind v4 `@theme inline` ตามกฎ client/CLAUDE.md, component ปฐมภูมิ `Button`/`Input`/`Label` มาจาก shadcn CLI จริง (แก้ `transition-colors` ที่ generator ใส่มาให้เป็น class ที่เจาะจงกว่าตามกฎ repo) พิสูจน์ว่าใช้งานจริงด้วยการ refactor `LoginPage`/`RegisterPage`/`Home` ให้ใช้ token+component ใหม่แทน raw Tailwind class เดิม verify ผ่าน browser จริง พฤติกรรมเดิมไม่เปลี่ยน ยังไม่สร้าง component เพิ่มเติม (Select/Dialog/ฯลฯ) เพราะยังไม่มี consumer จริง — เหลือใน [ADR-0008 audit](tickets/017-audit-adr-0008.md)

## Open implementation-debt tickets (audited, not yet built)

Audit แต่ละ ADR พบว่าสิ่งเหล่านี้ยังล้าหลังกว่า decision ที่บันทึกไว้ — ปล่อยเปิดไว้เป็น tracked backlog ไม่ใช่ decision ที่ต้องตัดสินใจเพิ่ม:

- ~~[ADR-0001 modular monolith](tickets/011-audit-adr-0001.md)~~ — ปิดแล้ว 2026-09-23 (module Notes พิสูจน์ pattern เป็นครั้งที่สองต่อจาก Identity)
- ~~[ADR-0002 in-process event communication](tickets/012-audit-adr-0002.md)~~ — ปิดแล้ว 2026-09-23 (mediator infra สร้างแล้ว, `NoteCreated`/`NoteUpdated`/`NoteDeleted` publish จริง ยังไม่มี handler subscribe ตามที่คาด)
- ~~[ADR-0003 schema-per-module + read models](tickets/013-audit-adr-0003.md)~~ — ปิดแล้ว 2026-09-24 (Organization schema สร้างแล้ว, read-model ข้าม module พิสูจน์แล้วทั้งสองทิศทางผ่าน ticket 022)
- ~~[ADR-0004 outbox + AI Categorization background job](tickets/014-audit-adr-0004.md)~~ — ปิดแล้ว 2026-09-24 (build ครบผ่าน ticket 024: outbox + BackgroundService + Gemini provider)
- ~~[ADR-0006 Postgres substring search](tickets/015-audit-adr-0006.md)~~ — ปิดแล้ว 2026-09-25 (build ครบผ่าน ticket 026)
- [ADR-0007 API-only backend, separate SPA deploy](tickets/016-audit-adr-0007.md) — re-audit 2026-09-25: รูปแบบโค้ดแบบ API-only ถูกต้อง, **CORS ปิด gap แล้ว** (ticket 027) เหลือแค่ deployment artifact (Dockerfile/CI) ที่ยังไม่มี มาพิสูจน์ว่า deploy แยกกันได้จริง
- [ADR-0008 frontend stack and editor](tickets/017-audit-adr-0008.md) — re-audit 2026-09-25 (รอบสอง): scaffolding React/Vite/router/TanStack Query/Tailwind + axios (ticket 027) + base editor Tiptap + **component library สไตล์ shadcn/ui (ใหม่)** พร้อมแล้วทั้งหมด เหลือแค่ component เพิ่มเติมตาม feature ที่จะสร้าง และ feature code (Note/Topic/Category/Search UI) ที่ยังไม่ทำ

## Open feature-implementation tickets (product spec decided, not yet built)

Product spec ของ Requirements v0.1 (Note capture, Code Block, Topic, Category, AI Categorization, User Ownership, Search) ถูกตัดสินใจครบถ้วนแล้วระหว่างเอกสาร requirements, GLOSSARY.md และ decision ของแผนผังนี้เอง — แต่ยังไม่มีอะไรอยู่ในโค้ดเลย ยืนยันได้จากการ grep `backend/` และ `client/src` หา `Note`/`Topic`/`Category`/Tiptap (มีแค่ Identity ที่ build แล้ว) เปิดไว้ล่วงหน้าเป็น backlog ตาม convention เดียวกับ ADR audit ticket ด้านบน:

- ~~[Note capture & CRUD](tickets/020-feature-note-capture-crud.md)~~ — ปิดแล้ว 2026-09-23 (ดู Decisions so far)
- [Code Block with language metadata](tickets/021-feature-code-block.md) — ถูก block ด้วย base editor (Tiptap, ADR-0008 audit ticket 017)
- ~~[Topic hierarchy](tickets/022-feature-topic-hierarchy.md)~~ — ปิดแล้ว 2026-09-24 (ดู Decisions so far)
- ~~[Category flat structure](tickets/023-feature-category-flat.md)~~ — ปิดแล้ว 2026-09-24 (ดู Decisions so far)
- ~~[AI Categorization suggestion flow](tickets/024-feature-ai-categorization-flow.md)~~ — ปิดแล้ว 2026-09-24 (ดู Decisions so far)
- ~~[User Ownership enforcement](tickets/025-feature-user-ownership.md)~~ — ปิดแล้ว 2026-09-24 (ดู Decisions so far)
- ~~[Search](tickets/026-feature-search.md)~~ — ปิดแล้ว 2026-09-25 (ดู Decisions so far)

## Open grilling tickets (database schema — not yet decided)

module Search ยังไม่ถูก grill เพราะ design ของมันขึ้นกับคำตอบ ticket 029 (Topic hierarchy) โดยตรง — ตอนนี้ 029 ตอบแล้ว (adjacency list) รอ Round ถัดไปเพื่อ grill schema ของ Search ต่อ:

(Round 1 ของคำถาม database schema ทั้งหมดตอบครบแล้วเมื่อ 2026-09-23 — ดู Decisions so far ด้านบน)

## Decisions so far (database schema, Round 1 — 2026-09-23)

- [User-scoping ของ Topic/Category](tickets/028-db-user-scoping.md): ผูกกับ User — ทุกตาราง (`Notes`/`Topics`/`Categories`) มี `UserId`, unique constraint เป็น composite `(UserId, Name)`
- [Topic hierarchy storage](tickets/029-db-topic-hierarchy-storage.md): Adjacency list (`Topics.ParentId` self-FK nullable)
- [Note มี Title แยกไหม](tickets/030-db-note-title.md): ไม่มี — list แสดง preview ของ `Content` แทน
- [Soft-delete shape ของ Note](tickets/031-db-note-soft-delete-shape.md): `Notes.DeletedAt` (timestamp, nullable) เดี่ยวๆ
- [Note-Category join table เก็บที่มาไหม](tickets/032-db-note-category-join-provenance.md): ไม่เก็บ — มีแค่ `NoteId + CategoryId`
- [AI Categorization "รันแล้วครั้งเดียว" เช็คจากไหน](tickets/033-db-ai-categorization-trigger-tracking.md): เก็บ flag บนตาราง `Notes` เอง (`AiCategorizationTriggeredAt`) — **ขัดกับคำแนะนำเดิม** (แนะนำ infer จาก `ai_categorization` schema) ผู้ใช้เลือกให้ query ตรงไปตรงมากว่าแทน
- [Suggestion เก็บ Category ใหม่ยังไง](tickets/034-db-suggestion-new-category-shape.md): ยืนยันเป็น nullable `CategoryId` FK + string `NewCategoryName` (exclusive กัน) ไม่สร้างแถว Category จริงจนกว่าจะ accept
- [Confidence threshold เก็บที่ไหน](tickets/035-db-confidence-threshold-storage.md): DB-backed global setting ใน schema `ai_categorization` — **ขัดกับคำแนะนำเดิม** (แนะนำ app config) ผู้ใช้เลือกให้แก้ค่าได้โดยไม่ต้อง deploy ใหม่

## Not yet specified

(ว่างในตอนนี้ — ประเด็นเดิมเรื่องการย้าย Note ไป Topic แบบ inline ถูกตอบแล้วที่ [ticket 036](tickets/036-topic-must-exist-before-note-move.md))

## Out of scope

- Real-time/push notification, semantic/vector search, การแชร์ note, version history, การแนบไฟล์, การแยกเป็น microservices — ตาม non-goal ที่ระบุไว้ชัดเจนใน CLAUDE.md และ ARCHITECTURE.md ไม่เปิดทบทวนใหม่ที่นี่
