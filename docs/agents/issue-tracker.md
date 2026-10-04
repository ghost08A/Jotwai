# Issue tracker: GitHub

Issue และ spec ของ repo นี้อยู่ในรูปแบบ GitHub issue ใช้ `gh` CLI สำหรับทุก operation

## Conventions

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

สร้าง GitHub issue

## เมื่อ skill บอกว่า "ดึง ticket ที่เกี่ยวข้อง"

รัน `gh issue view <number> --comments`

## Wayfinding operations

ใช้โดย `/wayfinder` **Override ตัดสินใจเมื่อ 2026-09-22**: แผนผัง/ticket ของ wayfinder สำหรับ repo นี้อยู่ใน **local-markdown tracker** ที่ `docs/wayfinder/` ไม่ใช่ GitHub Issues — นี่แคบกว่า convention ทั่วไปที่ว่า "issue อยู่ใน GitHub" ด้านบน ซึ่งยังคงใช้กับทุกอย่างอื่น (bug, feature, PR-เป็นช่องทางรับ request) แผนผังแรก (Jotwai Requirements v0.1) เดิมทีถูกสร้างเป็น GitHub issue #7–#19 (ghost08A/Jotwai) และย้ายมาเป็นไฟล์ local โดยเก็บเนื้อหาไว้ครบถ้วน GitHub issue เหล่านั้นยังคงอยู่ในฐานะ archive ไม่ใช่ copy ที่ใช้งานจริงแล้ว

- **แผนผัง (Map)**: `docs/wayfinder/map.md` Frontmatter: `title`, `label: wayfinder:map` เนื้อหามี Destination / Notes / Decisions-so-far / Not yet specified / Out of scope ตามโครงสร้างที่ skill อธิบายไว้
- **Child ticket**: `docs/wayfinder/tickets/<id>-<slug>.md` หนึ่งไฟล์ต่อหนึ่ง ticket Frontmatter: `id` (ตัวเลข เพิ่มขึ้นเรื่อยๆ ตลอดทั้ง tracker), `title`, `type` (`wayfinder:research`/`prototype`/`grilling`/`task`), `status` (`open`/`closed`), `assignee` (`null` หรือ username — คนที่ claim), `parent` (relative path ไปยัง map), `blocked_by` (list ของ relative path ไปยังไฟล์ ticket ที่เป็นตัว block, ว่างเปล่าถ้าไม่ถูก block) เนื้อหา: `## Question` (คำถามของ ticket กำหนดไว้ตั้งแต่สร้าง) ตามด้วย `## Resolution`/`## Finding` (เพิ่มเมื่อ resolve แล้ว)
- **การ Block**: field frontmatter `blocked_by` ระบุด้วย file path Ticket จะไม่ถูก block เมื่อทุกไฟล์ใน `blocked_by` มี `status: closed`
- **Frontier query**: list ไฟล์ภายใต้ `docs/wayfinder/tickets/` ที่มี `status: open`, `assignee: null` และทุก entry ใน `blocked_by` เป็น `status: closed` แล้ว เรียงตาม `id` ตัวแรกชนะ
- **Claim**: แก้ frontmatter ของไฟล์ ticket, `assignee: null` → `assignee: <username>` — เป็นการเขียนแรกของ session นั้น ก่อนงานอื่นใดบน ticket นั้น
- **Resolve**: เพิ่ม `## Resolution` (สำหรับ decision ticket) หรือ `## Finding` (สำหรับ research ticket) ต่อท้าย body ของ ticket พร้อมคำตอบ, ตั้ง `status: closed` ใน frontmatter, แล้วเพิ่ม pointer หนึ่งบรรทัด (สรุปสั้นๆ + relative link ไปยังไฟล์ ticket) ใน Decisions-so-far ของ map ถ้า ticket แบบ research/audit เจอ gap แทนที่จะยืนยัน decision ให้ปล่อย `status: open` ไว้และบันทึก finding นั้นด้วย — มันจะกลายเป็น tracked backlog ไม่ใช่ decision ที่ต้องทบทวนใหม่ (ดู Notes ของ map ปัจจุบันสำหรับ convention เฉพาะของ project นี้เรื่อง audit ticket)
