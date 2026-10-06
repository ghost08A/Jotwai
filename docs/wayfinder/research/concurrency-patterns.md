# Research: แนวทางป้องกัน stale write / แก้ข้อมูลพร้อมกัน

Ticket: [050](../tickets/050-research-concurrency-patterns.md) · วันที่รวบรวม: 2026-10-05

> เอกสารนี้ **รายงานข้อเท็จจริงเท่านั้น** ไม่เลือก/แนะนำตัวเลือกแทนเจ้าของโปรเจกต์
> "stale write" = บันทึกทับด้วยข้อมูลที่เก่ากว่าของจริงในฐานข้อมูล (เช่น เปิดโน้ตเดียวกัน 2 Tab แล้วบันทึกจาก Tab ที่ข้อมูลเก่าไปทับ Tab ที่ใหม่กว่า)
> ส่วนที่ระบุว่า "(ข้อสังเกต)" คือการอนุมานจากข้อเท็จจริง ไม่ได้เป็นข้อความจากแหล่งโดยตรง

## 1. Optimistic concurrency และ concurrency token (EF Core)

อธิบายง่ายๆ: optimistic concurrency = "ไม่ล็อกข้อมูลไว้ก่อน แต่ตอนบันทึกให้เช็กว่าข้อมูลยังเป็นเวอร์ชันเดิมที่เราอ่านมาหรือเปล่า ถ้าไม่ใช่ ให้บันทึกล้มเหลว"

ข้อเท็จจริง (แหล่ง: [EF Core — Handling Concurrency Conflicts](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)):
- EF Core ใช้ optimistic concurrency โดยกำหนด property เป็น *concurrency token* ค่านี้ถูกโหลดมาพร้อม entity ตอน query และตอน `SaveChanges()` ค่าใน DB จะถูกเทียบกับค่าที่อ่านมาตอนแรก
- EF เติมเงื่อนไขใน SQL: `UPDATE ... WHERE Id = @p1 AND Version = @p2` ถ้ามีคนแก้ไปก่อน จะไม่มีแถวถูกอัปเดต (0 rows affected) แล้ว EF โยน `DbUpdateConcurrencyException` ให้ application จัดการ
- exception นี้โยนตอน update และ delete แต่ปกติไม่โยนตอน insert (insert ซ้ำ key จะเป็น exception เฉพาะของ provider)
- มี 2 วิธีกำหนด token:
  - **Database-generated** (`[Timestamp]` / `.IsRowVersion()`): DB เปลี่ยนค่าให้เองทุกครั้งที่แถวถูกแก้ ป้องกันทั้งแถว (ตัวอย่างในเอกสารเป็น SQL Server `rowversion`; เอกสารบอกว่ารายละเอียดต่างกันในแต่ละ DB)
  - **Application-managed** (`[ConcurrencyCheck]` / `.IsConcurrencyToken()` เช่น property `Guid`): โค้ดต้องกำหนดค่าใหม่เองทุกครั้งที่บันทึก (หรือใช้ `SaveChanges` interceptor) ข้อดีที่เอกสารระบุ: คุมได้ว่าคอลัมน์ไหนเปลี่ยนแล้วถึงนับเป็น conflict และใช้ได้กับ DB ที่ไม่มีชนิดข้อมูลอัปเดตเองอย่าง SQLite
- ทางเลือกในการ resolve conflict ที่เอกสารระบุ: (ก) แจ้งผู้ใช้ว่าบันทึกล้มเหลว ให้โหลดข้อมูลใหม่แล้วลองอีกครั้ง (ข) ถ้าเป็นงานอัตโนมัติ ให้ query ใหม่แล้ว retry วนซ้ำ (ค) *merge* ค่าที่ค้างอยู่กับค่าใน DB โดยรายละเอียดขึ้นกับแอป อาจให้ UI แสดงทั้งสองชุดค่า
- EF ให้ค่า 3 ชุดช่วย resolve: *Current values* (ที่แอปพยายามเขียน), *Original values* (ที่อ่านมาแต่แรก), *Database values* (ที่อยู่ใน DB ตอนนี้) ผ่าน `DbUpdateConcurrencyException.Entries`, `GetDatabaseValuesAsync()`, `OriginalValues.SetValues(...)`

## 2. การใช้กับ PostgreSQL (Npgsql)

ข้อเท็จจริง:
- PostgreSQL ไม่มีคอลัมน์ "rowversion" แบบอัปเดตเองเหมือน SQL Server Npgsql จึงใช้ system column `xmin` (เก็บ ID ของ transaction ที่อัปเดตล่าสุด) โดยแมป property ชนิด `uint` ด้วย `[Timestamp]` หรือ `.IsRowVersion()` ค่านี้เปลี่ยนเองทุกครั้งที่แถวถูกแก้ (แหล่ง: [Npgsql EF Core — Concurrency Tokens](https://www.npgsql.org/efcore/modeling/concurrency.html))
- เอกสาร PostgreSQL: `xmin` คือ transaction ID ของ transaction ที่สร้าง row version นั้น และ transaction ID เป็นเลข 32-bit ที่ **wraparound (วนกลับ) ได้**; เอกสารเตือนว่าอย่าพึ่งพาความไม่ซ้ำของ transaction ID ในระยะยาว (เกิน ~1 พันล้าน transaction) (แหล่ง: [PostgreSQL — System Columns](https://www.postgresql.org/docs/current/ddl-system-columns.html))
- (ข้อสังเกต) `xmin` เป็นของ row ทั้งแถว ดังนั้นการอัปเดตใดๆ ก็เปลี่ยนค่า ถ้าต้องการให้บางคอลัมน์ไม่นับเป็น conflict ต้องใช้ token แบบ application-managed แทน
- ไม่ได้ตรวจในรอบนี้: สถานะของ `UseXminAsConcurrencyToken()` ใน Npgsql เวอร์ชันล่าสุด (เอกสารที่อ่านระบุเฉพาะ `[Timestamp]`/`.IsRowVersion()` บน `uint`)

## 3. Isolation level แทน token

ข้อเท็จจริง (แหล่ง: EF Core docs ข้างต้น):
- PostgreSQL ที่ระดับ *repeatable read* ทำงานแบบ optimistic: ถ้ามี transaction อื่นแก้แถวเดียวกันไปก่อน เมื่อเราจะ update จะเกิด serialization error
- ข้อจำกัดที่เอกสารระบุ: ต้องมี transaction คลุมทุก operation ถ้าอ่านเพื่อโชว์ผู้ใช้แล้วรอให้ผู้ใช้แก้ transaction ต้องเปิดค้างนาน ซึ่งเอกสารบอกว่าควรหลีกเลี่ยงในกรณีส่วนใหญ่ เหมาะกับกรณีที่ operation ทั้งหมดทำเสร็จทันทีโดยไม่รอ input จากภายนอก
- (ข้อสังเกต) เงื่อนไขนี้ไม่ตรงกับ flow "โหลดโน้ตเข้า editor → ผู้ใช้พิมพ์ → autosave" ซึ่งคร่อมหลาย request

## 4. HTTP-level: ETag / If-Match / If-Unmodified-Since

ข้อเท็จจริง:
- **ETag**: ตัวระบุ (opaque) ของสถานะ representation เซิร์ฟเวอร์สร้างเอง อาจมาจาก content hash หรือ metadata ก็ได้ มี strong (ตรงกันทุก byte) และ weak (เท่ากันเชิงความหมาย) (แหล่ง: [RFC 9110 §8.8](https://www.rfc-editor.org/rfc/rfc9110.html))
- **If-Match**: request จะทำต่อเมื่อ ETag ปัจจุบันของ resource ตรงกับที่ client ส่งมา ใช้ strong comparison; ETag แบบ weak (`W/`) จะ **ไม่มีทาง match** ใน If-Match ถ้าไม่ตรงตอบ **412 Precondition Failed** ใช้กับ PUT/PATCH เพื่อกัน "mid-air collision" (แหล่ง: RFC 9110 §13.1.1, §15.5.13; [MDN — If-Match](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/If-Match))
- flow ตาม MDN: client GET ได้ ETag → แก้ในเครื่อง → PUT พร้อม `If-Match` → ถ้าไม่ตรง 412 → client ต้องดึงใหม่แล้วทำซ้ำ
- `If-Match` ไม่ใช่ forbidden request header จึงตั้งใน cross-origin request ได้ (MDN) (ข้อสังเกต: ถ้า client/API คนละ origin ยังต้องดู CORS preflight/allowed headers ซึ่งไม่ได้ตรวจในรอบนี้)
- **428 Precondition Required** (RFC 6585 §3): ให้เซิร์ฟเวอร์ "บังคับ" ว่า request ต้องมี precondition เพื่อกัน lost update; response ต้องไม่ถูก cache ([RFC 6585](https://www.rfc-editor.org/rfc/rfc6585.html))
- **If-Unmodified-Since + Last-Modified**: เทียบเวลาแก้ไขล่าสุด; HTTP-date มีความละเอียด **1 วินาที** จึงแยกการแก้ไขที่เกิดในวินาทีเดียวกันไม่ได้ และ Last-Modified อาจนับเป็น weak validator เว้นแต่เซิร์ฟเวอร์ยืนยันได้ว่าไม่มีการเปลี่ยนซ้อนในช่วงวินาทีนั้น (RFC 9110 §8.8.2, §13.1.4)
- ไม่พบ/ไม่ได้ยืนยัน: ว่า ASP.NET Core (.NET 10) มี built-in middleware สำหรับ If-Match/412 บน API endpoint หรือไม่ (หน้าเอกสารที่ลองอ่านไม่ได้ให้ข้อมูลนี้ชัดเจน) — ถ้าต้องการ ต้องตรวจเพิ่ม

## 5. Timestamp comparison (application-level)

ข้อเท็จจริงจากแหล่ง primary ที่ตรวจ: ยังไม่มีเอกสารเฉพาะ "เทียบ timestamp ใน body" แยกต่างหาก ข้อที่อ้างอิงได้คือความละเอียดของ Last-Modified (1 วินาที, หัวข้อ 4) และ EF Core ระบุว่า token แบบ application-managed คุมได้เองว่าเมื่อไรเปลี่ยนค่า
(ข้อสังเกต) timestamp ที่เก็บในคอลัมน์ของเราเองเป็น concurrency token แบบ application-managed ชนิดหนึ่ง ความละเอียด/ความเที่ยงตรงขึ้นกับชนิดคอลัมน์และนาฬิกาที่ใช้เขียน ซึ่งไม่ได้ตรวจเอกสารในรอบนี้

## 6. Merge และ real-time collaboration (CRDT / Yjs / Tiptap Collaboration)

ข้อเท็จจริง:
- **Merge แบบ 3 ค่า** (current/original/database) ที่ EF รองรับ เป็นการ merge ระดับ property/field; เอกสาร EF ไม่ได้กล่าวถึงการ merge ข้อความยาว (เช่น Markdown) ในระดับตัวอักษร/บรรทัด
- **Yjs** คือ CRDT ที่ระบุว่า shared data types "merge โดยไม่เกิด conflict" อัตโนมัติ, state เข้ารหัสเป็น binary update (`Y.encodeStateAsUpdate` / `Y.applyUpdate`), ลำดับการ apply update ไม่สำคัญ จึงรองรับการแก้ offline แล้วค่อย sync; network agnostic มี provider หลายราย ([Yjs docs](https://docs.yjs.dev/))  — เอกสารที่อ่านไม่ได้ระบุเรื่องการเก็บเป็น plain text/Markdown
- **Tiptap Collaboration** ซิงก์ด้วย Yjs CRDT ต้องมี backend (Hocuspocus ซึ่งเป็น open source หรือ Tiptap Collaboration Cloud / on-premises) และ persist เป็น "Yjs updates" ไม่ใช่ JSON; มี REST API สำหรับ export JSON/ผลักการแก้ไข, version history snapshot, comments ([Tiptap docs](https://tiptap.dev/docs/collaboration/getting-started/overview)) ราคา/ไลเซนส์และ offline support ไม่ได้ระบุในหน้าที่อ่าน
- (ข้อสังเกต เชื่อมกับ CLAUDE.md/ARCHITECTURE) กติกาโปรเจกต์ระบุว่าเนื้อหา Note เป็น Markdown เป็น source of truth, ห้ามเพิ่ม real-time/push infrastructure และ version history โดยไม่ปรึกษา — แนวทาง CRDT ที่เก็บเป็น Yjs update และต้องมี sync server จึงสัมพันธ์กับข้อกำหนดเหล่านี้ (ตัดสินใจโดยเจ้าของโปรเจกต์)

## 7. Conflict UX (สิ่งที่แหล่งระบุ)

ข้อเท็จจริง: EF Core docs ระบุทางเลือกเชิงพฤติกรรมเมื่อเจอ conflict ได้แก่ แจ้งผู้ใช้แล้วโหลดใหม่/ลองใหม่, retry อัตโนมัติ, หรือ merge โดยอาจให้ UI แสดงทั้งสองชุดค่า; MDN/RFC ระบุว่าเมื่อ 412 ฝั่ง client ต้องดึงสถานะใหม่แล้วค่อยทำซ้ำ; RFC 6585 ระบุว่าเซิร์ฟเวอร์ควรบอกวิธี resubmit ให้สำเร็จ ไม่พบแหล่ง primary ที่กำหนดรูปแบบ UI เฉพาะสำหรับ editor ข้อความ

## 8. ประเด็นที่เกี่ยวกับ autosave ใน browser (บริบท)

ข้อเท็จจริง (แหล่ง: [MDN — sendBeacon](https://developer.mozilla.org/en-US/docs/Web/API/Navigator/sendBeacon)):
- `sendBeacon()` ส่งได้เฉพาะ **POST**, ตั้ง custom header ไม่ได้, payload จำกัด 64 KiB, อ่าน response ไม่ได้
- MDN แนะนำ `fetch()` + `keepalive: true` เมื่อต้องการ method/header อื่น และแนะนำใช้ event `visibilitychange` แทน `unload`/`beforeunload` (ไม่น่าเชื่อถือและขัดกับ bfcache)
- (ข้อสังเกต) การบันทึกตอนปิด Tab ผ่าน `sendBeacon` จึงใส่ `If-Match` header ไม่ได้ ขีดจำกัดขนาดของ `fetch keepalive` ไม่ได้ตรวจในรอบนี้

## สรุปตารางข้อเท็จจริง (ไม่ใช่คำแนะนำ)

| แนวทาง | ตรวจจับ stale write | ต้องมีอะไรเพิ่ม | ข้อจำกัดที่แหล่งระบุ |
| --- | --- | --- | --- |
| EF concurrency token แบบ DB-generated (`xmin` บน Postgres) | ได้ (แถวทั้งแถว) | map `uint` + `[Timestamp]` | xmin เป็น 32-bit, wraparound ได้; ทุกการแก้ของแถวนับเป็น conflict |
| EF concurrency token แบบ app-managed (Guid/ตัวนับ/timestamp) | ได้ | โค้ดกำหนดค่าใหม่เองทุกครั้ง | คุมได้ว่าคอลัมน์ใดนับเป็น conflict; ลืมอัปเดตค่าไม่ได้ |
| ETag / If-Match / 412 (HTTP) | ได้ (ระดับ request) | server สร้าง/ตรวจ ETag, client ส่ง header | If-Match ใช้ strong เท่านั้น; sendBeacon ตั้ง header ไม่ได้ |
| If-Unmodified-Since | ได้แบบหยาบ | Last-Modified | ละเอียดแค่ 1 วินาที |
| Isolation level (repeatable read) | ได้ภายใน transaction | transaction คลุมงาน | ไม่เหมาะเมื่อมี user input คั่นกลาง |
| Merge เชิงข้อมูล (3 ชุดค่า) | ได้ + รวมค่า | โค้ด/UI สำหรับ merge | EF docs พูดถึงระดับ property |
| CRDT (Yjs / Tiptap Collaboration) | ไม่เกิด conflict (merge อัตโนมัติ) | sync backend (Hocuspocus/Cloud), เก็บ Yjs update | ผูกกับรูปแบบข้อมูลและ infra ที่ต่างจาก Markdown-as-source-of-truth |

## สิ่งที่ยังไม่ได้ตรวจ / ช่องว่าง
- built-in ของ ASP.NET Core (.NET 10) สำหรับ ETag/If-Match
- สถานะ `UseXminAsConcurrencyToken()` ใน Npgsql EF Core เวอร์ชันล่าสุด
- ขีดจำกัดขนาด `fetch keepalive`
- ราคา/ไลเซนส์ Tiptap Collaboration และ offline support
- ความละเอียดของชนิด timestamp ใน PostgreSQL (`timestamptz`) เทียบกับการเขียนซ้อนกัน
