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

## Open implementation-debt tickets (audited, not yet built)

Audit แต่ละ ADR พบว่าสิ่งเหล่านี้ยังล้าหลังกว่า decision ที่บันทึกไว้ — ปล่อยเปิดไว้เป็น tracked backlog ไม่ใช่ decision ที่ต้องตัดสินใจเพิ่ม:

- [ADR-0001 modular monolith](tickets/011-audit-adr-0001.md) — มีแค่ Identity module เท่านั้นที่มีอยู่; Notes/Organization/AI Categorization/Search ยังไม่ได้ build
- [ADR-0002 in-process event communication](tickets/012-audit-adr-0002.md) — ยังไม่มี mediator/event infrastructure เลย
- [ADR-0003 schema-per-module + read models](tickets/013-audit-adr-0003.md) — มีแค่ schema/DbContext ของ `identity` เท่านั้น; pattern พิสูจน์แล้วว่าใช้ได้ แต่ยังไม่ถูกขยายไปที่อื่น
- [ADR-0004 outbox + AI Categorization background job](tickets/014-audit-adr-0004.md) — ยังไม่ได้ build อะไรเลย
- [ADR-0006 Postgres substring search](tickets/015-audit-adr-0006.md) — ยังไม่ได้ build อะไรเลย
- [ADR-0007 API-only backend, separate SPA deploy](tickets/016-audit-adr-0007.md) — รูปแบบโค้ดแบบ API-only ถูกต้องแล้ว แต่ยังไม่มี CORS config และยังไม่มี deployment artifact (Dockerfile/CI) มาพิสูจน์ว่า deploy แยกกันได้จริง
- [ADR-0008 frontend stack and editor](tickets/017-audit-adr-0008.md) — scaffolding ของ React/Vite/router/TanStack Query/Tailwind มีอยู่แล้ว; Tiptap, component สไตล์ shadcn/ui, การใช้ axios และ feature code ทั้งหมดยังขาดอยู่

## Open feature-implementation tickets (product spec decided, not yet built)

Product spec ของ Requirements v0.1 (Note capture, Code Block, Topic, Category, AI Categorization, User Ownership, Search) ถูกตัดสินใจครบถ้วนแล้วระหว่างเอกสาร requirements, GLOSSARY.md และ decision ของแผนผังนี้เอง — แต่ยังไม่มีอะไรอยู่ในโค้ดเลย ยืนยันได้จากการ grep `backend/` และ `client/src` หา `Note`/`Topic`/`Category`/Tiptap (มีแค่ Identity ที่ build แล้ว) เปิดไว้ล่วงหน้าเป็น backlog ตาม convention เดียวกับ ADR audit ticket ด้านบน:

- [Note capture & CRUD](tickets/020-feature-note-capture-crud.md) — สร้างได้โดยไม่ต้องมี Topic/Category, แก้ไขแบบ overwrite, soft delete + Trash
- [Code Block with language metadata](tickets/021-feature-code-block.md) — ถูก block ด้วย base editor (Tiptap, ADR-0008 audit ticket 017)
- [Topic hierarchy](tickets/022-feature-topic-hierarchy.md) — การซ้อนชั้น, ความ unique ของพี่น้อง, หนึ่ง Note มีได้หนึ่ง Topic, guard ป้องกันลบ Topic ที่ยังไม่ว่าง
- [Category flat structure](tickets/023-feature-category-flat.md) — ความ unique ทั้งระบบ, หนึ่ง Note มีได้หลาย Category
- [AI Categorization suggestion flow](tickets/024-feature-ai-categorization-flow.md) — background trigger, confidence threshold, bulk-accept แบบ atomic
- [User Ownership enforcement](tickets/025-feature-user-ownership.md) — invariant แบบ cross-cutting ครอบคลุม Notes/Topic/Category/AI Categorization; ถูก block จนกว่าทั้ง 4 อย่างจะเสร็จ
- [Search](tickets/026-feature-search.md) — search แบบ substring บนเนื้อหา/Topic/Category รวมถึงเนื้อหาใน code block
- [Frontend login flow](tickets/027-feature-frontend-login.md) — UI login/register, การเก็บ token + refresh, การ wiring Protected Route; backend Auth API build และ test เสร็จแล้ว, `axios` ฝั่ง client ยังไม่ถูกใช้งาน

## Not yet specified

- การ "ย้าย" Note ไปยัง Topic รวมถึงการสร้าง Topic ที่ยังไม่มีอยู่แบบ inline ในการกระทำเดียวกันหรือไม่ หรือต้องให้ Topic มีอยู่ก่อนแล้ว (ผุดขึ้นมาระหว่างการเจาะจง destination ให้แคบลง ยังไม่คมพอที่จะตั้งเป็น ticket)

## Out of scope

- Real-time/push notification, semantic/vector search, การแชร์ note, version history, การแนบไฟล์, การแยกเป็น microservices — ตาม non-goal ที่ระบุไว้ชัดเจนใน CLAUDE.md และ ARCHITECTURE.md ไม่เปิดทบทวนใหม่ที่นี่
