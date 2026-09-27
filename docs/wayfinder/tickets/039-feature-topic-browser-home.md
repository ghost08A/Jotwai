---
id: 39
title: "สร้าง: หน้าแรกใหม่แบบ Notion (topic browser + search) และหน้า detail ของ topic"
type: wayfinder:task
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

ผู้ใช้ขอเปลี่ยนหน้าแรก ("/") จากหน้าจดโน้ตแบบ capture-first เดิม ให้เป็นหน้าสารบัญ topic แบบ Notion sidebar/page แทน — grill กันจนได้ scope ที่ยืนยันแล้วดังนี้ (2026-09-26):

### หน้าแรก (`/`) — ใหม่ทั้งหมด

- ช่องค้นหา + ตัวเลือก category (เลือกได้หลายอัน, semantics แบบ **AND** — note ต้องติดครบทุก category ที่เลือก)
- ด้านล่างเป็น tree ของ topic ทั้งหมด **หุบไว้เป็นค่าเริ่มต้น** (เห็นแค่ root topic ก่อน)
- แต่ละแถวมี 2 จุดกดแยกกัน: ลูกศร/chevron = กาง/หุบดู child topic ชั้นถัดไป (ไม่เปลี่ยนหน้า), ชื่อ topic = navigate ไปหน้า detail ของ topic นั้น (`/topics/:topicId`)
- Search ค้นทั้ง **ชื่อ topic และเนื้อหา note ที่ผูกอยู่ข้างใน** (ไม่ว่าจะซ้อนลึกแค่ไหน) — ไม่ใช่แค่ชื่อ topic เฉยๆ (คงความสามารถเดิมของหน้า `/search` ไว้)
- เมื่อมี filter (search text และ/หรือ category) active: topic ที่ตรงเอง หรือเป็น **ancestor ของ topic ที่มี note ตรง** จะถูกกางออกให้เห็นเป็นเส้นทางทั้งสายอัตโนมัติ (ไม่ต้องกดลูกศรเอง) ส่วนกิ่งที่ไม่เกี่ยวข้องเลยจะถูกซ่อน ถ้าใส่ทั้ง search text และ category พร้อมกัน ต้องตรง**ทั้งคู่** (AND ข้าม filter type) ถ้าไม่พบอะไรเลยแสดง empty state
- ตัด single-select topic dropdown ที่เคยอยู่ใน mockup เดิมออก (ซ้ำซ้อนกับ tree + search ที่มีอยู่แล้ว)
- ปุ่ม "+ new topic" สร้าง topic ระดับบนสุด (เหมือนพฤติกรรมเดิมของ `/topics`)

### หน้า detail ของ topic (`/topics/:topicId`)

- แสดงเฉพาะ note ที่ผูกกับ topic นี้ตรงๆ เท่านั้น (`topicId` ตรง, **ไม่ roll-up** note ของ subtopic ขึ้นมา)
- reuse `NoteEditor`/`NoteCard` เดิมทั้งหมด (composer สร้าง note ใหม่ preset `topicId` เป็น topic ปัจจุบัน, แก้ไข/ลบ/ย้าย topic ทำงานเหมือน note ทั่วไปทุกอย่าง)
- ส่วนแสดง subtopic: list ของ child topic **ชั้นถัดไปชั้นเดียวเท่านั้น** (ไม่มี nested tree ซ้อนอยู่ในหน้านี้ — กดเข้าไปทีละชั้นเรื่อยๆ แทน) พร้อมปุ่มสร้าง subtopic ใหม่
- Breadcrumb เต็มรูปแบบด้านบน (เช่น `หน้าแรก > งาน > โปรเจกต์ A > งาน UI`) กดขั้นไหนก็กระโดดไปขั้นนั้นได้เลย
- แก้ไขชื่อ / ลบ topic ปัจจุบันได้จากหน้านี้เลย (delete guard เดิม: ลบไม่ได้ถ้ายังมี subtopic หรือ note อยู่ข้างใน)

### หน้าจดโน้ตเดิม

- Composer + note list + trash ย้ายจาก `/` ไปเป็น `/notes` (พฤติกรรมเดิมทั้งหมดไม่เปลี่ยน แค่ย้าย route + nav)

### หน้าที่ถูกลบ

- `/topics` (ของเดิม, ticket 022) และ `/search` (ticket 017 audit) ถูกลบทิ้ง — ความสามารถทั้งหมดถูกดูดรวมเข้าไปในหน้าแรกใหม่ + หน้า detail ของ topic ข้างต้นแล้ว

### หมายเหตุด้าน implementation (ทราบไว้ล่วงหน้า ไม่ใช่ scope เพิ่ม)

- `GET /api/search` ปัจจุบันรองรับแค่ topic เดียว (`topicId`) และไม่รองรับ multi-category AND filter หรือส่ง ancestor-path กลับมาให้ — ต้องขยาย backend (เพิ่ม parameter/endpoint) เพื่อรองรับ scope ข้างต้น เป็นรายละเอียดทางเทคนิคที่ตัดสินใจตอน implement ไม่ใช่ product decision เพิ่มเติม

## Resolution

Build เสร็จและ verify ครบวงจรจริงแล้ว (2026-09-26):

**Backend**: `GET /api/search` เพิ่ม `categoryIds` (repeated query param, `List<Guid>?`) ที่ `SearchController`/`ISearchService`/`SearchService` — AND semantics จริง (resolve เป็น NoteId ที่มีครบทุก CategoryId ที่ขอ ผ่าน `GroupBy` + `Distinct().Count() == requiredCategoryIds.Count`) รวมเข้ากับ `query`/`topicId` filter เดิมแบบ AND (สาม `.Where()` ต่อกันบน `IQueryable` เดียวกัน) เพิ่ม integration test ใหม่ `Search_filtering_by_multiple_categories_requires_every_one_to_match` (positive + negative case) — รวม test ผ่านครบ 67/67, `dotnet format --verify-no-changes` ผ่าน

**Client — หน้าแรกใหม่ (`modules/organization/components/TopicBrowserPage.tsx`)**: search box (debounce 300ms) + `CategoryFilterChips` (multiselect chip toggle ใหม่) + topic tree (`TopicBrowserNode.tsx`, หุบเป็นค่าเริ่มต้น, chevron แยกจาก name-link) เรียก `useSearch` (module search, ตัด `topicId` ออกจาก frontend เพราะไม่มีใครเรียกใช้แล้ว — backend ยังคงรองรับไว้เพราะ test เดิมยังอ้างอิงอยู่) คำนวณ topic ที่ต้องโผล่ (`utils/computeVisibleTopicIds.ts`: matched-by-search-result ∪ matched-by-name (เฉพาะตอนไม่มี category filter) ∪ ancestor ของทั้งหมด) แล้ว prune topic list ก่อนส่งเข้า `buildTopicTree` เดิม — เมื่อ filter active ทุก node ที่เหลือ render แบบ force-expand (ไม่มี local collapse)

**Client — หน้า detail (`modules/organization/components/TopicDetailPage.tsx`)**: breadcrumb เต็ม (`utils/buildAncestorChain.ts`) rename/delete topic ปัจจุบัน (reuse `useRenameTopic`/`useDeleteTopic` เดิม, delete guard เดิมยังทำงาน — verify แล้วว่า topic ที่มี note/subtopic ลบไม่ได้พร้อม error message เดิม) composer สร้าง note ใหม่ reuse `NoteEditor` + `useCreateNote` แล้วต่อด้วย `useAssignNoteTopic` (2 mutation เรียงกัน) เพื่อ preset topic อัตโนมัติ — verify แล้วว่า note ใหม่ได้ topic ถูกต้องจริง note list ใช้ `NoteCard` เดิมเป๊ะ กรอง client-side ด้วย `topicId` ตรงเท่านั้น (ไม่ roll-up) subtopic แสดงเป็น flat list ชั้นเดียว + `CreateTopicForm` เดิม (parentId = topic ปัจจุบัน)

**Route/nav**: `/` → `TopicBrowserPage` (ใหม่), `/notes` → `NotesPage` (ย้ายจาก `/` เดิม พฤติกรรมไม่เปลี่ยน), `/topics/:topicId` → `TopicDetailPage` (ใหม่, ผ่าน `ROUTES.topicDetailPattern` ไม่ hardcode path), ลบ `/topics` (`TopicsPage.tsx`/`TopicNode.tsx`) และ `/search` (`SearchPage.tsx`/`SearchResultCard.tsx`/`SearchResultCardSkeleton.tsx`) ทิ้งทั้งหมด nav ใน `PrivateLayout` อัปเดตเป็น Home/Notes/Categories/Trash

**Refactor ระหว่างทาง**: ดึง `CategoryChip` ออกมาเป็น shared component (`shared/components/CategoryChip.tsx`) ใช้ร่วมกันระหว่าง `NoteCategoryChips` (attach/detach บน note) กับ `CategoryFilterChips` (multiselect filter ใหม่) เพื่อไม่ให้ markup ของ chip ซ้ำกันสองที่

**Verify ผ่าน browser จริง**: expand/collapse chevron ไม่เปลี่ยนหน้า, กด topic name navigate ไปหน้า detail ถูกต้อง, breadcrumb หลายชั้นกระโดดกลับถูก, สร้าง note ใหม่ในหน้า detail ได้ topic ที่ถูกต้องจริง (ตรวจ `<select>` topic ของ note ใหม่), search ด้วยชื่อ topic เจอ topic ว่างที่ไม่มี note เลย (client-side name match), search ไม่เจอแสดง empty state ถูกต้อง, delete guard บล็อก topic ที่มี note พร้อม error message, delete topic ว่างสำเร็จแล้ว navigate กลับถูกที่ (root → home), rename UI แสดงถูกต้อง, สร้าง root topic ใหม่สำเร็จ, **multi-category AND filter** verify ผ่าน browser จริงด้วย (สร้าง category ทดสอบ ติดกับ note ใน `aaaaa`, เลือก chip แล้ว tree กรองเหลือแค่ path ที่ถูกต้อง — bug ตัวจริงที่เจอระหว่างทางคือ axios ส่ง array param เป็น `categoryIds[]=` ซึ่ง ASP.NET Core's `List<Guid>?` ไม่รู้จัก แก้โดยสร้าง query string เองด้วย `URLSearchParams` แทนใน `searchService.ts`)

ผ่าน `/code-review` สองแกน (Standards/Spec) แก้ hard violation 2 จุดที่เจอ (non-null assertion ใน `TopicDetailPage.tsx`, hardcoded route path ใน `AppRouter.tsx` แทนที่ด้วย `ROUTES.topicDetailPattern`) และ duplicated code (chip markup) ตามที่สรุปไว้ด้านบน

## Update (2026-09-26) — ปรับ UI ตาม feedback ให้เป็นแบบ Notion มากขึ้น

หลังจากลอง build เสร็จ ผู้ใช้ทดลองใช้แล้วขอปรับ UX ของหน้า detail และ editor ให้ใกล้เคียง Notion มากขึ้น:

- **ชื่อ topic แก้ได้ตรงๆ ไม่ต้องกด Rename**: `TopicDetailPage.tsx` เปลี่ยนจาก toggle ปุ่ม Rename/Save/Cancel เป็น `<input>` แก้ inline ตลอดเวลา (พิมพ์แล้ว blur หรือกด Enter เพื่อ commit, ค่าว่างจะ revert กลับเป็นชื่อเดิม) ระหว่างทางเจอ bug จริง: `useState(topic?.name ?? "")` init ด้วยค่าว่างเพราะ topics ยังโหลดไม่เสร็จตอน render แรก แก้โดยแยก component ออกเป็น wrapper (เช็ค loading/not-found ก่อน) + `TopicDetailPageContent` ที่รับ `topic` เป็น prop ที่ยืนยันว่ามีค่าแล้วเท่านั้น พร้อม `key={topicId}` บังคับ remount ทุกครั้งที่ navigate ไป topic อื่น (แก้ bug แฝงอีกตัวที่ local state เช่น draft ของ note composer จะรั่วไหลข้าม topic ได้ถ้าไม่ remount)
- **ตัด toolbar ถาวรออกจาก editor ทั้งหมด**: ลบ `NoteEditorToolbar.tsx` ทิ้ง (ใช้ร่วมกันทุกที่ที่มี `NoteEditor` — NotesPage, NoteCard, TopicDetailPage) เพราะมี "/" slash menu (ticket 037) สร้าง block ใหม่อยู่แล้ว
- **เพิ่ม floating format menu แบบ Notion**: `NoteEditorBubbleMenu.tsx` ใหม่ ใช้ `BubbleMenu` จาก `@tiptap/react/menus` โผล่อัตโนมัติทันทีที่ select ข้อความ (ไม่ต้องกดอะไรเพิ่ม) มีปุ่มเดิมทั้งหมด (H2/H3/Bold/Italic/Strike/Inline code/Quote/Bulleted/Numbered/Code block) ยกเว้น Divider (ย้ายไปอยู่ที่ "/" อย่างเดียวเพราะเป็น insert ไม่ใช่ reformat ของ selection) ใช้ `shouldShow` แบบ custom ให้โผล่ตอน cursor อยู่ในโค้ดบล็อกด้วย (แม้ไม่ได้ select ข้อความ) เพื่อเปลี่ยนภาษาได้ — ตอนอยู่ในโค้ดบล็อกจะโชว์แค่ dropdown ภาษา ไม่โชว์ปุ่ม format อื่น verify ผ่าน browser จริงครบ (select ข้อความเห็นเมนูลอยขึ้นจริง, กด Bold ติดจริง, slash menu ยังทำงานปกติ, เข้า code block เห็น dropdown ภาษาอย่างเดียว)
- **ทุกหน้าขยายเต็มความกว้าง**: เปลี่ยน `mx-auto max-w-2xl p-6` เป็น `w-full px-6 py-6 md:px-12 lg:px-24` (pattern เดียวกับที่ `NotesPage` ใช้อยู่แล้ว) ใน `TopicBrowserPage.tsx`, `TopicDetailPage.tsx`, `TrashPage.tsx`, `CategoriesPage.tsx` verify ผ่าน browser จริงด้วย `getBoundingClientRect()` ว่า container กว้างเท่า viewport ลบ padding จริง (ไม่ถูกจำกัดที่ 42rem อีกต่อไป)
