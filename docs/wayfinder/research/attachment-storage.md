# Research: ตัวเลือกที่เก็บรูปภาพ/ไฟล์แนบ

> Ticket: 051 (`docs/wayfinder/tickets/051-research-attachment-storage.md`)
> วันที่ค้นคว้า: 2026-10-05
> **เอกสารนี้รายงานข้อเท็จจริงเท่านั้น ไม่มีข้อเสนอแนะ/การเลือกแทนเจ้าของโปรเจกต์**
> ทุกข้อมีแหล่งอ้างอิง (ลิงก์) เป็นเอกสาร primary ของเจ้าของเทคโนโลยีนั้นๆ ส่วนที่ "เอกสารไม่ได้ระบุ" ระบุไว้ตรงๆ ไม่เดา

บริบท: ASP.NET Core (.NET 10) API-only + PostgreSQL, React SPA deploy static แยก (คนละ origin กับ API), ไฟล์แนบอยู่ในเนื้อหา Markdown แบบ Notion, อาจแชร์ลิงก์ read-only ให้คนนอกระบบ

---

## 1. ตัวเลือกที่ครอบคลุม

| # | ตัวเลือก | คำอธิบายสั้น (ภาษาง่าย) |
| --- | --- | --- |
| A | Local disk (ดิสก์ของเซิร์ฟเวอร์ API) | เก็บเป็นไฟล์บนเครื่องที่รัน API แล้วให้ API ส่งไฟล์ออกมา |
| B | PostgreSQL `bytea` | เก็บไบต์ของไฟล์เป็นคอลัมน์ในตารางปกติ |
| C | PostgreSQL Large Objects | กลไกพิเศษของ Postgres สำหรับเก็บก้อนข้อมูลใหญ่ แยกจากตารางปกติ |
| D | S3-compatible object storage | บริการ "ถังเก็บไฟล์" (AWS S3, Cloudflare R2, MinIO ฯลฯ) เข้าถึงผ่าน HTTP API |

---

## 2. ข้อเท็จจริงของ ASP.NET Core ที่เกี่ยวกับทุกตัวเลือก

### 2.1 การรับอัปโหลด (upload) และขีดจำกัดขนาด
แหล่ง: [Upload files in ASP.NET Core (MS Learn)](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-9.0)

- Kestrel `MaxRequestBodySize` ค่าเริ่มต้น 30,000,000 bytes (~30 MB); ปรับได้ทั้งระดับ server และต่อ request (`IHttpMaxRequestBodySizeFeature`)
- `FormOptions.MultipartBodyLengthLimit` ค่าเริ่มต้น 128 MB (134,217,728 bytes); `MemoryBufferThreshold` 64 KB (เกินนี้ framework ย้ายไปบัฟเฟอร์ลงดิสก์ชั่วคราว)
- มี 2 แบบ: buffering (`IFormFile`, ง่าย แต่ใช้หน่วยความจำ/ดิสก์ชั่วคราวมากกว่า) กับ streaming (`MultipartReader`, ใช้หน่วยความจำน้อย เหมาะไฟล์ใหญ่)
- เอกสารระบุตัวเลือกที่เก็บ 3 แบบและข้อสังเกตของตัวเอง: Database ("faster for small files", สะดวกดึงกลับ, อาจถูกกว่า), Physical storage (เหมาะไฟล์ใหญ่), Cloud (Azure Blob: scalable/resilient/ถูกกว่าที่สเกลใหญ่) — เป็นข้อความในเอกสาร MS ไม่ใช่ตัวเลขวัดผล
- แนวปฏิบัติความปลอดภัยที่เอกสารระบุ: อย่าเชื่อ `IFormFile.FileName`, สร้างชื่อไฟล์สุ่ม (`Path.GetRandomFileName()`) แทนชื่อที่ผู้ใช้ส่ง, ตรวจนามสกุลด้วย allow-list, ตรวจ file signature (magic bytes), จำกัดขนาด, สแกนไวรัส/มัลแวร์, เก็บไฟล์นอกแผนผังแอป/นอก web root, ปิด execute permission ของโฟลเดอร์อัปโหลด, ตรวจฝั่ง server เสมอ (ห้ามพึ่ง client-side อย่างเดียว)

### 2.2 การส่งไฟล์ออก (serve / download) จาก API
แหล่ง: [Create responses in Minimal APIs (MS Learn)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-10.0), [Static files in ASP.NET Core (MS Learn)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-10.0)

- `TypedResults.File` / `Results.File` รับ `byte[]` หรือ `Stream` และใส่ชื่อไฟล์ (`fileDownloadName`) ได้ ตัวอย่างในเอกสารคืน PDF พร้อมชื่อ `report.pdf`
- `Results.Stream` เข้าถึง response stream โดยไม่บัฟเฟอร์ทั้งก้อน และมีตัวอย่าง `enableRangeProcessing: true` (รองรับ HTTP Range คือขอส่งไฟล์เป็นช่วง เช่น เลื่อนวิดีโอ)
- `TypedResults.PhysicalFile` ส่งไฟล์ตาม absolute path
- Static file middleware ให้บริการไฟล์นอก web root ได้ (`UseStaticFiles` + `FileProvider`) แต่เอกสารระบุว่าโดยปกติ static file middleware **ไม่ตรวจสิทธิ์** (ไฟล์ใต้ web root เข้าได้สาธารณะ); วิธีที่เอกสารให้สำหรับไฟล์ที่ต้องตรวจสิทธิ์คือเก็บไว้นอก web root แล้วให้ endpoint ที่ผ่าน authorization เป็นคนส่ง (เช่นเรียกหลัง `UseAuthorization`)

### 2.3 Browser, `<img>` และสิทธิ์ (กระทบทุกตัวเลือกที่ API ตรวจสิทธิ์ก่อนส่งไฟล์)
แหล่ง: [`<img>` (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/img)

- `<img src>` ใส่ header `Authorization` เองไม่ได้ ตัวเลือก `crossorigin="use-credentials"` ส่ง cookie ได้ (แต่ไม่ใช่ Authorization header); `crossorigin="anonymous"` ไม่ส่ง credentials
- ผลต่อ Markdown: ถ้าเนื้อหา Markdown อ้างรูปด้วย URL ธรรมดา (`![](url)`) เบราว์เซอร์จะดึงรูปเองโดยไม่ผ่านโค้ด JS ของแอป กลไกสิทธิ์จึงต้องเป็นอย่างใดอย่างหนึ่งของ: cookie, token ใน URL (เช่น presigned URL), หรือ endpoint สาธารณะ (เอกสาร MDN ไม่ได้พูดถึงทางเลือกอื่นเช่น fetch เป็น blob ด้วย JS — นั่นเป็นเทคนิคฝั่งแอป ไม่ใช่ข้อมูลจากแหล่งนี้)

### 2.4 Header ที่เกี่ยวกับการแสดงผล/ดาวน์โหลดและความปลอดภัย
แหล่ง: [Content-Disposition (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Content-Disposition), [X-Content-Type-Options (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/X-Content-Type-Options)

- `Content-Disposition: inline` (ค่า default) = แสดงในเบราว์เซอร์; `attachment` = บังคับให้ดาวน์โหลด; ชื่อไฟล์ใส่ผ่าน `filename` / `filename*` (UTF-8, ชื่อไทยต้องใช้ `filename*`; ถ้ามีทั้งสอง `filename*` ชนะ)
- `X-Content-Type-Options: nosniff` ให้เบราว์เซอร์ใช้ `Content-Type` ตามที่ server บอกโดยไม่เดาจากเนื้อหา; MDN ระบุว่าช่วยกันกรณีไฟล์ที่ผู้ใช้อัปโหลดถูกตีความเป็น HTML/สคริปต์ (XSS)
- header เหล่านี้กำหนดได้ทั้งโดย API (ตัวเลือก A/B/C) และโดย object storage เอง (ตัวเลือก D: `Content-Type`, `Content-Disposition` เป็น system metadata ที่ตั้งตอนอัปโหลดได้ — ดู 6)

---

## 3. ตัวเลือก A: Local disk

ข้อเท็จจริงจาก 2.1/2.2 (MS Learn) ที่ใช้ตรงๆ:

- key/URL อ้างอิง: ไม่มีกลไกสำเร็จรูป ต้องออกแบบเอง (เช่นเก็บ id/ชื่อสุ่มใน DB แล้วแมปเป็น path); เอกสาร MS แนะนำชื่อไฟล์สุ่มแทนชื่อจากผู้ใช้ และเก็บนอกแผนผังแอป
- แสดง inline/download: ผ่าน endpoint ของ API (`PhysicalFile`/`File`/`Stream`) กำหนด `Content-Disposition` เองได้, Range ใช้ได้ด้วย `enableRangeProcessing`
- สิทธิ์: static file middleware ไม่ตรวจสิทธิ์โดยตัวเอง ต้องให้ endpoint ที่ผ่าน authorization เป็นตัวส่ง หรือจัดลำดับ middleware ตามเอกสาร
- ลบ: ลบไฟล์ด้วย System.IO และ DB แยกกัน เอกสารไม่ได้ระบุกลไก transaction ข้ามสองที่ (ไฟล์กับแถว DB ไม่อยู่ใน transaction เดียวกัน)
- ขนาด/ชนิด: จำกัดที่ Kestrel/FormOptions + ตรวจ extension/signature ตามข้อ 2.1
- ข้อที่เอกสารไม่ได้พูดถึง: การทำงานเมื่อมีหลาย instance ของ API หรือ container ที่ดิสก์ไม่ถาวร; การสำรองข้อมูล — เป็นข้อเท็จจริงเชิงสถาปัตยกรรมทั่วไป (ไฟล์ผูกกับเครื่อง/volume ที่เก็บ) ไม่ได้มาจากเอกสารเฉพาะ ควรยืนยันกับ deployment จริงของโปรเจกต์

## 4. ตัวเลือก B: PostgreSQL `bytea`

แหล่ง: [PostgreSQL: Binary Data Types](https://www.postgresql.org/docs/current/datatype-binary.html), [PostgreSQL: TOAST](https://www.postgresql.org/docs/current/storage-toast.html), [Npgsql: Basic type mapping](https://www.npgsql.org/doc/types/basic.html)

- `bytea` เก็บไบต์ความยาวแปรผัน; ค่าที่ใหญ่เกิน ~2 kB ถูกบีบอัดและ/หรือย้ายไปตาราง TOAST อัตโนมัติ (chunk ~2000 bytes) และ **ขนาดสูงสุดต่อค่าหนึ่งช่อง = 1 GB**
- Npgsql: อ่านเป็น `byte[]` เป็นค่าเริ่มต้น; ไฟล์ใหญ่อ่านแบบ stream ได้ด้วย `CommandBehavior.SequentialAccess` + `NpgsqlDataReader.GetStream()`; เขียนได้ด้วย `byte[]`, `ReadOnlyMemory<byte>`, `Stream` (Npgsql 7.0+) เอกสาร Npgsql ไม่ระบุเพดานขนาดของ `bytea`
- รูปแบบ output ค่าเริ่มต้นเป็น hex (2 ตัวอักษรต่อ 1 byte) ใน text protocol (ตัวเลขนี้เป็นของ wire format; เอกสารที่อ่านไม่ได้ระบุว่า Npgsql ใช้ binary protocol หรือไม่ — ไม่ยืนยัน)
- key/URL: ใช้ PK ของแถว (เช่น GUID) — แต่การส่งออกยังต้องผ่าน endpoint ของ API เสมอ (Postgres ไม่มี HTTP)
- สิทธิ์: อยู่ในตารางเดียวกับข้อมูลโน้ต ใช้ตรรกะสิทธิ์ของ API เดียวกัน และอยู่ใน transaction/backup เดียวกับข้อมูลอื่น (ผลจากการเป็นคอลัมน์ปกติ)
- ลบ: `DELETE` แถวเดียว ได้ภายใน transaction เดียวกับการลบ/แก้โน้ต; พื้นที่ถูกคืนผ่านกลไก vacuum ของ Postgres (เอกสารที่ดึงมาไม่ลงรายละเอียด)
- ขนาด/ชนิด: เพดานทางเทคนิค 1 GB/ค่า; ขนาดจริงที่รับอัปโหลดถูกจำกัดโดย Kestrel/FormOptions (2.1)
- inline/download: ผ่าน endpoint ของ API (`Results.File`/`Stream`)

## 5. ตัวเลือก C: PostgreSQL Large Objects

แหล่ง: [PostgreSQL: Large Objects](https://www.postgresql.org/docs/current/largeobjects.html), [Implementation Features](https://www.postgresql.org/docs/current/lo-implementation.html)

- เป็นกลไกให้ "stream-style access" (อ่าน/เขียน/seek ทีละช่วง) กับข้อมูลที่ใหญ่เกินจะจัดการทั้งก้อน; เก็บเป็น chunk แถวละช่วงใน catalog ของระบบ พร้อม B-tree index
- มี API: create, import/export, open, read, write, seek, tell, truncate, close, `lo_unlink` (ลบ)
- สิทธิ์ (ตั้งแต่ PG 9.0): ต้องมี `SELECT` เพื่ออ่าน, `UPDATE` เพื่อเขียน/truncate; ลบ/เปลี่ยนเจ้าของ/comment ได้เฉพาะเจ้าของหรือ superuser; จัดผ่าน `GRANT`/`REVOKE` — เป็นระบบสิทธิ์ระดับ DB role ซึ่งแยกจากสิทธิ์ของผู้ใช้แอป
- ลบ: ต้องเรียก `lo_unlink` เอง (ตัว large object เป็นอ็อบเจ็กต์แยก มี OID) — เอกสารหน้าที่ดึงมา **ไม่ได้ระบุ** พฤติกรรมเมื่อแถวที่เก็บ OID ถูกลบ (ถือว่ายังไม่ยืนยันจากแหล่งนี้) และไม่ได้ระบุเพดานขนาด
- ใน Npgsql: เอกสารที่ดึงมา (หน้า basic types) ไม่ได้พูดถึง Large Object API ของ Npgsql จึงไม่ยืนยันรายละเอียดฝั่ง .NET จากแหล่งนี้

---

## 6. ตัวเลือก D: S3-compatible object storage

### 6.1 โมเดลข้อมูล, key, ขนาด
แหล่ง: [Naming Amazon S3 objects](https://docs.aws.amazon.com/AmazonS3/latest/userguide/object-keys.html), [Uploading objects](https://docs.aws.amazon.com/AmazonS3/latest/userguide/upload-objects.html)

- โครงสร้าง flat: bucket เก็บ object; ไม่มีโฟลเดอร์จริง (prefix + `/` คือแค่ convention)
- object key = ลำดับอักขระ UTF-8 สูงสุด 1,024 bytes รวม prefix; case-sensitive; เอกสารแนะนำชุดอักขระที่ปลอดภัย (ตัวอักษร/ตัวเลข `! - _ . * ' ( )`) และเตือนให้เลี่ยงอักขระอย่าง `\ { } ^ % ` ] " > [ ~ < # |` และ path segment `.`/`..`
- อัปโหลดครั้งเดียว (single PUT) สูงสุด 5 GB; multipart ใช้กับ object ตั้งแต่ 5 MB ขึ้นไป รองรับสูงสุด 50 TB (ตามหน้านี้)
- เอกสาร: object ใหม่ถูกเข้ารหัสฝั่ง server ด้วย SSE-S3 โดยค่าเริ่มต้น
- ขนาดจริงที่ผู้ใช้อัปโหลดผ่าน API ของเรายังถูกจำกัดตาม 2.1 ถ้าไฟล์ผ่านเซิร์ฟเวอร์ API (ต่างจากอัปโหลดตรงด้วย presigned URL — ดู 6.3)

### 6.2 การอ่าน/แสดง/ดาวน์โหลดและสิทธิ์
แหล่ง: [Sharing objects with presigned URLs](https://docs.aws.amazon.com/AmazonS3/latest/userguide/ShareObjectPreSignedURL.html), [Download and upload objects with presigned URLs](https://docs.aws.amazon.com/AmazonS3/latest/userguide/using-presigned-url.html), [Working with object metadata](https://docs.aws.amazon.com/AmazonS3/latest/userguide/UsingMetadata.html)

- ค่าเริ่มต้น: object ใน S3 เป็น private (เจ้าของเข้าถึงได้คนเดียว)
- แชร์ชั่วคราวได้ด้วย **presigned URL** — URL ที่ฝังลายเซ็นของ credentials ผู้สร้าง ให้สิทธิ์จำกัดเวลา (ดาวน์โหลดด้วย GET, อัปโหลดด้วย PUT); ผู้รับไม่ต้องมี AWS credentials
  - เอกสารเรียกมันว่า bearer token: ใครถือ URL ก็ใช้ได้จนหมดอายุ และใช้ซ้ำได้หลายครั้งภายในช่วงเวลา
  - อายุสูงสุด: IAM user + SigV4 = 7 วัน; ผ่าน console = 12 ชั่วโมง; ถ้าสร้างจาก temporary credentials (IAM role/STS) URL หมดอายุเมื่อ credentials หมด แม้ตั้งไว้นานกว่า (เช่น session ของ role ค่าเริ่มต้น 1 ชั่วโมง; EC2 role ราว 6 ชั่วโมง)
  - ตรวจเวลาหมดอายุ ณ ตอนส่ง HTTP request (ดาวน์โหลดที่เริ่มก่อนหมดอายุจะดำเนินต่อได้ แต่การต่อใหม่หลังหมดอายุจะล้ม)
  - จำกัดเพิ่มได้ด้วย bucket policy เช่น เงื่อนไข `s3:signatureAge`, จำกัดช่วง IP/VPC
  - เมื่อสร้างด้วย SDK ผู้สร้างต้องมีสิทธิ์ที่ตรงกับ operation (เช่น `s3:GetObject`)
- `Content-Type`, `Content-Disposition`, `Cache-Control` เป็น system metadata ที่ตั้งตอนอัปโหลดและแก้ได้ (การแก้ทำโดยการ copy object); metadata ที่ผู้ใช้กำหนดหลังอัปโหลดแก้ไม่ได้นอกจาก copy
- ผลต่อ `<img>`: presigned URL ใส่ใน `<img src>` ได้ เพราะสิทธิ์อยู่ใน query string (สอดคล้องกับ 2.3) แต่ URL ที่ฝังลงใน Markdown ต้องตัดสินใจเองว่าเก็บ key (แล้วค่อยออก URL ตอนอ่าน) หรือเก็บ URL — เอกสาร AWS ไม่ได้กำหนด (เป็นเรื่องการออกแบบของแอป); ข้อเท็จจริงเดียวที่เอกสารให้คือ URL มีวันหมดอายุตามที่กล่าว

### 6.3 การอัปโหลดตรงจากเบราว์เซอร์
แหล่ง: [using-presigned-url](https://docs.aws.amazon.com/AmazonS3/latest/userguide/using-presigned-url.html), [POST policy (browser-based upload)](https://docs.aws.amazon.com/AmazonS3/latest/API/sigv4-HTTPPOST.html)

- presigned PUT: ผู้ถือ URL อัปโหลด object ตาม key ที่ระบุได้; ถ้า key ซ้ำ object เดิมถูกแทนที่ ("replaces the existing object")
- เอกสาร presigned URL ระบุว่า capability ถูกจำกัดโดยสิทธิ์ของผู้สร้าง; หน้าที่ดึงมาไม่ได้ระบุกลไกบังคับขนาดสูงสุดของ presigned PUT
- อีกแบบคือ POST policy (ฟอร์มจากเบราว์เซอร์): มีเงื่อนไข `content-length-range [min, max]` จำกัดขนาด และ `starts-with $Content-Type` จำกัดชนิด (เช่น `image/`) พร้อมวันหมดอายุของ policy
- สังเกต: การตรวจ magic bytes/สแกนไวรัสแบบ 2.1 ทำในเส้นทางที่ไฟล์ผ่าน API ได้ทันที; ถ้าอัปโหลดตรงเข้า storage ไฟล์ไม่ผ่าน API ในจังหวะอัปโหลด (เอกสารที่ดึงมาไม่ได้พูดถึงการสแกนหลังอัปโหลด)

### 6.4 CORS (สำคัญเพราะ SPA กับ storage คนละ origin)
แหล่ง: [S3 CORS](https://docs.aws.amazon.com/AmazonS3/latest/userguide/cors.html), [R2 presigned URLs](https://developers.cloudflare.com/r2/api/s3/presigned-urls/)

- CORS คือกลไกให้เว็บที่โหลดจาก origin หนึ่งเรียกทรัพยากรอีก origin ได้; S3 ประเมินจาก `Origin`, `Access-Control-Request-Method`, `Access-Control-Request-Headers` เทียบกับ rule ใน `AllowedOrigins`/`AllowedMethods`/`AllowedHeaders` (rule แรกที่ตรง); ACL/policy ยังมีผลควบคู่
- R2: ถ้าใช้ presigned URL จากเบราว์เซอร์ ต้องตั้ง CORS rules บน bucket
- หมายเหตุ: การแสดง `<img>` ข้าม origin ปกติไม่ต้องใช้ CORS; CORS จำเป็นเมื่อ JS ของ SPA ทำ `fetch`/PUT ไปยัง storage โดยตรง (เป็นพฤติกรรมของเบราว์เซอร์ทั่วไป ไม่ได้อ้างจากหน้าเฉพาะข้างต้น)

### 6.5 การลบ
แหล่ง: [Deleting a single object](https://docs.aws.amazon.com/AmazonS3/latest/userguide/delete-objects.html)

- ลบด้วย DELETE API ต่อ object; ถ้า bucket ไม่เปิด versioning การลบถาวรและกู้ไม่ได้
- ถ้าเปิด versioning: ลบโดยไม่ระบุ version จะสร้าง delete marker (object เดิมยังอยู่ในรูปเวอร์ชันเก่า) ลบแบบถาวรต้องระบุ version id
- มี Lifecycle rule ตั้งลบ object อัตโนมัติได้
- การลบ object กับการลบแถวใน DB เป็นสองระบบแยกกัน ไม่อยู่ใน transaction เดียว (ข้อเท็จจริงโดยโครงสร้าง; เอกสารไม่ได้พูดถึงกรณีนี้โดยตรง)

### 6.6 ผู้ให้บริการที่ "S3-compatible" (ข้อเท็จจริงที่ตรวจได้)

**Cloudflare R2** — แหล่ง: [Presigned URLs](https://developers.cloudflare.com/r2/api/s3/presigned-urls/), [Public buckets](https://developers.cloudflare.com/r2/buckets/public-buckets/), [Pricing](https://developers.cloudflare.com/r2/pricing/)
- presigned URL อายุ 1 วินาที ถึง 7 วัน; รองรับ GET/HEAD/PUT/DELETE; **POST multipart ไม่รองรับ**; ใช้ได้เฉพาะโดเมน S3 API (`<ACCOUNT_ID>.r2.cloudflarestorage.com`) ไม่ใช้กับ custom domain (ถ้าจะตรวจสิทธิ์บน custom domain เอกสารชี้ไปที่ WAF HMAC validation)
- เปิดสาธารณะได้ 2 ทาง: custom domain (ใช้ cache/WAF/Access ได้) หรือ `r2.dev` (เอกสารระบุสำหรับ dev, มี rate limit); ถ้าใช้ WAF/Access ควรปิด `r2.dev` เพื่อไม่ให้ถูกข้าม
- ราคา (ณ วันที่ดึง): storage มาตรฐาน $0.015/GB-เดือน; Class A $4.50 ต่อล้าน request; Class B $0.36 ต่อล้าน; free tier 10 GB-เดือน, 1 ล้าน Class A, 10 ล้าน Class B; egress ฟรี ("does not incur data transfer ... charges")

**MinIO** — แหล่ง: [github.com/minio/minio](https://github.com/minio/minio)
- เป็น S3-compatible object storage ที่ host เองได้; license GNU AGPLv3 (README เตือนให้ตรวจภาระ AGPLv3 เมื่อใช้ใน stack รวมถึงการเปิดเผยโค้ดที่แก้ไข)
- repository (community edition) **ถูก archive เมื่อ 2026-04-25** เป็น read-only ไม่ได้รับการดูแลแล้ว และเปลี่ยนเป็นแจกเฉพาะซอร์ส (ไม่มี binary ใหม่); README ชี้ไปยัง AIStor Free / AIStor Enterprise เป็นทางเลือก (ตามข้อความใน repo ณ วันที่ดึง)

**AWS S3** — ราคา: หน้าที่ดึงมาในงานนี้ไม่ได้ยืนยันตัวเลขราคา จึงไม่ระบุ (ดู [S3 pricing](https://aws.amazon.com/s3/pricing) เอง)

**ผู้ให้บริการอื่นที่อ้างว่า S3-compatible** — ไม่ได้ตรวจใน research นี้; ระดับความเข้ากันของ API (เช่น ฟีเจอร์ presigned/POST policy/CORS) แต่ละเจ้าต่างกัน ดังตัวอย่างข้อจำกัดของ R2 ข้างต้น ควรอ่านเอกสารเจ้านั้นก่อนใช้

---

## 7. กรณีผู้รับ Share ลิงก์ read-only (คนนอกระบบ ไม่ได้ล็อกอิน)

สรุปเฉพาะข้อเท็จจริงที่ได้จากข้อ 2–6 ว่ากลไกใดทำอะไรได้บ้าง (ไม่ใช่การเลือก):

| ตัวเลือก | กลไกที่เอกสารรองรับให้คนไม่ล็อกอินเปิดรูปได้ | ข้อจำกัดที่เอกสารระบุ |
| --- | --- | --- |
| A, B, C | ต้องมี endpoint ของ API ที่ยอมรับ token/ลิงก์แชร์ (ต้องเขียนเอง) แล้วส่งไฟล์ด้วย `File`/`Stream`; `<img>` ส่งได้เฉพาะ cookie หรือ token ใน URL (MDN) | ไม่มีกลไกสำเร็จรูปของ ASP.NET Core/Postgres สำหรับ token แชร์ไฟล์; static file middleware ไม่ตรวจสิทธิ์เอง; ไบต์ทุกตัวผ่านเซิร์ฟเวอร์ API |
| D (S3/R2) | presigned GET URL (ผู้รับไม่ต้องมี credentials); หรือเปิด public (R2: custom domain / `r2.dev`) | presigned URL หมดอายุ (≤7 วันกับ IAM user/R2; ผูกกับอายุ credentials ถ้าเป็น temporary credentials) และเป็น bearer token; R2 presigned ใช้กับ custom domain ไม่ได้ |

หมายเหตุเรื่องวงจรชีวิต: เอกสาร AWS ระบุว่า URL หมดอายุแล้วดาวน์โหลดใหม่ไม่ได้ ดังนั้นลิงก์ที่ฝังอยู่ในหน้า Share ที่ควรเปิดได้นานกว่าอายุ URL ต้องมีการออก URL ใหม่ตอนเปิดหน้า — เป็นนัยโดยตรงของข้อเท็จจริงเรื่องวันหมดอายุ

---

## 8. ตารางเปรียบเทียบข้อเท็จจริง (ไม่จัดอันดับ)

| หัวข้อ | A Local disk | B `bytea` | C Large Object | D S3-compatible |
| --- | --- | --- | --- | --- |
| Key/ref | ออกแบบเอง (ชื่อสุ่ม+path) | PK ของแถว | OID ของ large object | object key ≤1,024 bytes UTF-8 |
| ส่งออกเป็น HTTP | ผ่าน API | ผ่าน API | ผ่าน API | ตรงจาก storage ผ่าน presigned/public หรือผ่าน API |
| เพดานขนาดเชิงเทคนิค | ตามดิสก์ | 1 GB/ค่า (TOAST) | เอกสารที่ดึงมาไม่ระบุ | single PUT 5 GB; multipart ถึง 50 TB (S3) |
| Range / stream | `enableRangeProcessing` | อ่านแบบ stream ได้ (Npgsql SequentialAccess) แต่ range ที่ระดับ DB ไม่ได้ยืนยัน | API มี seek/read เป็นช่วง (libpq) | storage รองรับเอง (เอกสารที่ดึงมาไม่ลงรายละเอียด Range); presigned+Range มีเงื่อนไข `If-Range` (AWS FAQ) |
| ลบ | ลบไฟล์แยกจาก DB | `DELETE` แถว ใน transaction เดียวกับโน้ต | `lo_unlink` ต้องเรียกเอง (พฤติกรรม orphan ไม่ยืนยัน) | DELETE API; versioning ทำให้เกิด delete marker; lifecycle rule ได้ |
| ลบ atomic กับ DB | ไม่ | ใช่ (ตารางเดียวกัน) | ไม่ยืนยันจากแหล่งที่อ่าน | ไม่ (ระบบแยก) |
| สิทธิ์ | ตรรกะใน API | ตรรกะใน API | GRANT/REVOKE ระดับ DB role + ตรรกะใน API | private by default; presigned URL / bucket policy / IAM; CORS |
| ภาระ API ตอนส่งไฟล์ | สูง (ไบต์ผ่าน API) | สูง (ไบต์ผ่าน API + DB) | สูง | ต่ำได้ถ้าใช้ presigned/public (ไบต์ไม่ผ่าน API) |
| Backup | แยกจาก DB | อยู่ใน backup ของ DB | อยู่ใน backup ของ DB (ตามความเข้าใจทั่วไป ไม่ได้ยืนยันจากหน้าที่อ่าน) | ผู้ให้บริการจัดการเอง (ไม่ได้ตรวจใน research นี้) |
| ค่าใช้จ่าย | ค่าดิสก์ | ขนาด DB โต | ขนาด DB โต | ตามผู้ให้บริการ (เช่น R2 ตัวเลขข้างบน); MinIO self-host มีประเด็น AGPL + archived |
| ขอบเขตที่ต้องทำเอง | endpoint+จัดเก็บ+ลบ | endpoint | endpoint + จัดการ OID/ลบ | SDK, bucket/CORS/credentials, การออก URL |

---

## 9. ช่องว่างที่เอกสารที่อ่านไม่ได้ยืนยัน (เปิดเป็น "ยังไม่รู้")

1. Large Objects: เพดานขนาด, พฤติกรรม orphan เมื่อลบแถวอ้างอิง, การรองรับใน Npgsql/EF Core
2. `bytea` ผ่าน EF Core: พฤติกรรมการโหลดทั้งก้อนเมื่อ query entity (ไม่ได้อ่านเอกสาร EF Core ในงานนี้)
3. ค่าใช้จ่ายจริงของ AWS S3 และผู้ให้บริการ S3-compatible อื่น; ความเข้ากันของแต่ละเจ้ากับ presigned/POST policy/CORS
4. การสแกนไวรัส/ตรวจ magic bytes เมื่ออัปโหลดตรงเข้า object storage (ไม่มีในหน้าที่อ่าน)
5. การรองรับ HTTP Range บน presigned URL ของ S3 ในเชิงพฤติกรรม (พบเพียง FAQ เรื่อง `If-Range` ต้องถูก sign)
6. ลักษณะการ deploy จริงของโปรเจกต์ (จำนวน instance, ดิสก์ถาวรหรือไม่, ผู้ให้บริการ hosting) ซึ่งกำหนดว่าตัวเลือก A ใช้ได้แค่ไหน

## 10. รายการแหล่งอ้างอิง

- ASP.NET Core: [File uploads](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-9.0), [Static files](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-10.0), [Minimal API responses](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-10.0)
- PostgreSQL: [Binary types](https://www.postgresql.org/docs/current/datatype-binary.html), [TOAST](https://www.postgresql.org/docs/current/storage-toast.html), [Large Objects](https://www.postgresql.org/docs/current/largeobjects.html), [LO implementation](https://www.postgresql.org/docs/current/lo-implementation.html)
- Npgsql: [Basic type mapping](https://www.npgsql.org/doc/types/basic.html)
- AWS S3: [Object keys](https://docs.aws.amazon.com/AmazonS3/latest/userguide/object-keys.html), [Uploading objects](https://docs.aws.amazon.com/AmazonS3/latest/userguide/upload-objects.html), [Presigned URLs](https://docs.aws.amazon.com/AmazonS3/latest/userguide/using-presigned-url.html), [Sharing with presigned URLs](https://docs.aws.amazon.com/AmazonS3/latest/userguide/ShareObjectPreSignedURL.html), [CORS](https://docs.aws.amazon.com/AmazonS3/latest/userguide/cors.html), [Deleting objects](https://docs.aws.amazon.com/AmazonS3/latest/userguide/delete-objects.html), [Object metadata](https://docs.aws.amazon.com/AmazonS3/latest/userguide/UsingMetadata.html), [POST policy](https://docs.aws.amazon.com/AmazonS3/latest/API/sigv4-HTTPPOST.html)
- Cloudflare R2: [Presigned URLs](https://developers.cloudflare.com/r2/api/s3/presigned-urls/), [Public buckets](https://developers.cloudflare.com/r2/buckets/public-buckets/), [Pricing](https://developers.cloudflare.com/r2/pricing/)
- MinIO: [github.com/minio/minio](https://github.com/minio/minio)
- MDN: [`<img>`](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/img), [Content-Disposition](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Content-Disposition), [X-Content-Type-Options](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/X-Content-Type-Options)

หมายเหตุวิธีการ: ข้อมูลดึงผ่านเครื่องมือสรุปหน้าเว็บ (ไม่ใช่การอ่านต้นฉบับทีละบรรทัดทุกหน้า) ตัวเลขสำคัญก่อนนำไปตัดสินใจควรเปิดเช็กกับหน้าต้นทาง โดยเฉพาะราคา R2 และสถานะ MinIO ซึ่งเปลี่ยนได้
