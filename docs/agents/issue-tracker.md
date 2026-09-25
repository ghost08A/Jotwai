# Issue tracker: local-markdown (ไม่ใช้ GitHub Issues อีกต่อไป)

**Override ตัดสินใจเมื่อ 2026-09-25**: ผู้ใช้สั่งให้หยุดทำงาน issue/ticket บน GitHub ทั้งหมด ทำในเครื่อง (local-markdown) เพียงอย่างเดียว — นี่ขยาย scope ของ override เดิมเมื่อ 2026-09-22 ที่เคยจำกัดแค่งาน wayfinder (ดู section "Local-markdown tracker operations" ด้านล่าง) ให้ครอบคลุมทุกประเภทงาน (bug, feature, audit, ฯลฯ) ไม่ใช่แค่ wayfinder

- **อย่ารัน `gh issue`/`gh pr` เพื่อสร้าง, comment, หรือแก้ label/state ใดๆ อีก** GitHub Issues ของ ghost08A/Jotwai (รวมถึง #7–#19 ที่มีอยู่แล้ว) อยู่ในฐานะ **archive เท่านั้น** ไม่ใช่ tracker ที่ใช้งานจริง
- Tracker ที่ใช้งานจริงคือ `docs/wayfinder/` ตาม convention เดียวกับที่ section "Local-markdown tracker operations" อธิบายไว้ — ใช้โครงสร้างเดียวกันนี้สำหรับ ticket ทุกประเภทนับจากนี้ ไม่ใช่แค่ของ wayfinder เดิม
- ถ้าพบ reference ไปยัง GitHub issue number ในเอกสารเก่า (เช่น `archived_from:` frontmatter) นั่นคือ pointer ไปยัง archive เฉยๆ ห้ามใช้เป็นแหล่งอ้างอิงที่ต้องอัปเดตต่อ — ให้ตามไปที่ local ticket file แทน
- Section ด้านล่างที่พูดถึง `gh` CLI (การอ่าน/สร้าง issue, PR-เป็นช่องทางรับ request) เก็บไว้เป็นข้อมูลอ้างอิงในกรณีที่ต้อง **อ่าน** ของเก่าจาก archive หรือ workflow PR ภายนอกยังต้องใช้ `gh pr` จริง (PR ไม่ได้ย้ายมาเป็น local — ยังอยู่บน GitHub ตามปกติ) — เฉพาะ **issue/ticket tracking** เท่านั้นที่ย้ายมา local

## Conventions (สำหรับ PR และการอ่าน archive เท่านั้น — ไม่ใช้สร้าง/แก้ issue อีก)

- **สร้าง issue**: `gh issue create --title "..." --body "..."` ใช้ heredoc สำหรับ body หลายบรรทัด
- **อ่าน issue**: `gh issue view <number> --comments` กรอง comment ด้วย `jq` และดึง label มาด้วย
- **List issue**: `gh issue list --state open --json number,title,body,labels,comments --jq '[.[] | {number, title, body, labels: [.labels[].name], comments: [.comments[].body]}]'` พร้อม `--label` และ `--state` ตามความเหมาะสม
- **Comment บน issue**: `gh issue comment <number> --body "..."`
- **ใส่/เอา label**: `gh issue edit <number> --add-label "..."` / `--remove-label "..."`
- **ปิด**: `gh issue close <number> --comment "..."`

Infer repo จาก `git remote -v`; `gh` จะทำสิ่งนี้ให้อัตโนมัติเมื่อรันภายใน clone

## Pull request ในฐานะช่องทาง triage

**PR เป็นช่องทางรับ request: ไม่** _(ตั้งเป็น `yes` ถ้า repo นี้ถือว่า PR ภายนอกเป็น feature request; `/triage` จะอ่าน flag นี้)_

เมื่อตั้งเป็น `yes`, PR จะผ่าน label และ state เดียวกันกับ issue โดยใช้คำสั่ง `gh pr` ที่เทียบเท่ากัน:

- **อ่าน PR**: `gh pr view <number> --comments` และ `gh pr diff <number>` สำหรับดู diff
- **List PR ภายนอกสำหรับ triage**: `gh pr list --state open --json number,title,body,labels,author,authorAssociation,comments` แล้วเก็บเฉพาะ `authorAssociation` ที่เป็น `CONTRIBUTOR`, `FIRST_TIME_CONTRIBUTOR` หรือ `NONE` (ตัด `OWNER`/`MEMBER`/`COLLABORATOR` ออก)
- **Comment / label / close**: `gh pr comment`, `gh pr edit --add-label`/`--remove-label`, `gh pr close`

GitHub ใช้ number space ร่วมกันระหว่าง issue กับ PR ดังนั้น `#42` เปล่าๆ อาจเป็นอย่างใดอย่างหนึ่งก็ได้: resolve ด้วย `gh pr view 42` แล้ว fallback ไป `gh issue view 42`

## เมื่อ skill บอกว่า "publish ไปที่ issue tracker"

สร้าง ticket ใหม่ใน `docs/wayfinder/tickets/` ตาม convention ด้านล่าง (ไม่ใช่ GitHub issue อีกต่อไป)

## เมื่อ skill บอกว่า "ดึง ticket ที่เกี่ยวข้อง"

อ่านไฟล์ `docs/wayfinder/tickets/<id>-<slug>.md` ที่เกี่ยวข้องโดยตรง (ไม่ใช่ `gh issue view` อีกต่อไป)

## Local-markdown tracker operations

**Override ตัดสินใจเมื่อ 2026-09-22, ขยาย scope เมื่อ 2026-09-25**: ticket/issue ทุกประเภทของ repo นี้ (ไม่ใช่แค่ wayfinder) อยู่ใน **local-markdown tracker** ที่ `docs/wayfinder/` ไม่ใช่ GitHub Issues แผนผังแรก (Jotwai Requirements v0.1) เดิมทีถูกสร้างเป็น GitHub issue #7–#19 (ghost08A/Jotwai) และย้ายมาเป็นไฟล์ local โดยเก็บเนื้อหาไว้ครบถ้วน GitHub issue เหล่านั้นยังคงอยู่ในฐานะ archive ไม่ใช่ copy ที่ใช้งานจริงแล้ว

- **แผนผัง (Map)**: `docs/wayfinder/map.md` Frontmatter: `title`, `label: wayfinder:map` เนื้อหามี Destination / Notes / Decisions-so-far / Not yet specified / Out of scope ตามโครงสร้างที่ skill อธิบายไว้
- **Child ticket**: `docs/wayfinder/tickets/<id>-<slug>.md` หนึ่งไฟล์ต่อหนึ่ง ticket Frontmatter: `id` (ตัวเลข เพิ่มขึ้นเรื่อยๆ ตลอดทั้ง tracker), `title`, `type` (`wayfinder:research`/`prototype`/`grilling`/`task`), `status` (`open`/`closed`), `assignee` (`null` หรือ username — คนที่ claim), `parent` (relative path ไปยัง map), `blocked_by` (list ของ relative path ไปยังไฟล์ ticket ที่เป็นตัว block, ว่างเปล่าถ้าไม่ถูก block) เนื้อหา: `## Question` (คำถามของ ticket กำหนดไว้ตั้งแต่สร้าง) ตามด้วย `## Resolution`/`## Finding` (เพิ่มเมื่อ resolve แล้ว)
- **การ Block**: field frontmatter `blocked_by` ระบุด้วย file path Ticket จะไม่ถูก block เมื่อทุกไฟล์ใน `blocked_by` มี `status: closed`
- **Frontier query**: list ไฟล์ภายใต้ `docs/wayfinder/tickets/` ที่มี `status: open`, `assignee: null` และทุก entry ใน `blocked_by` เป็น `status: closed` แล้ว เรียงตาม `id` ตัวแรกชนะ
- **Claim**: แก้ frontmatter ของไฟล์ ticket, `assignee: null` → `assignee: <username>` — เป็นการเขียนแรกของ session นั้น ก่อนงานอื่นใดบน ticket นั้น
- **Resolve**: เพิ่ม `## Resolution` (สำหรับ decision ticket) หรือ `## Finding` (สำหรับ research ticket) ต่อท้าย body ของ ticket พร้อมคำตอบ, ตั้ง `status: closed` ใน frontmatter, แล้วเพิ่ม pointer หนึ่งบรรทัด (สรุปสั้นๆ + relative link ไปยังไฟล์ ticket) ใน Decisions-so-far ของ map ถ้า ticket แบบ research/audit เจอ gap แทนที่จะยืนยัน decision ให้ปล่อย `status: open` ไว้และบันทึก finding นั้นด้วย — มันจะกลายเป็น tracked backlog ไม่ใช่ decision ที่ต้องทบทวนใหม่ (ดู Notes ของ map ปัจจุบันสำหรับ convention เฉพาะของ project นี้เรื่อง audit ticket)
