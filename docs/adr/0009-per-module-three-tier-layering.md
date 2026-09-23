# ADR 0009: โครงสร้าง project แบบ three-tier ต่อ module (Domain/Service ต่อ Module + API ที่ใช้ร่วมกัน)

## สถานะ
**ถูกแทนที่โดย ADR-0010** เก็บไว้เป็นบันทึกทางประวัติศาสตร์ — การแบ่ง project ต่อ module ที่อธิบายไว้ด้านล่างถูก implement ไปแล้วบางส่วนสำหรับ Issue #2 แล้วถูกยกเลิกก่อนที่ ticket นั้นจะเสร็จสิ้น เพื่อให้สอดคล้องกับ convention 3-tier แบบ global ที่บริษัทใช้อยู่แล้ว (ดู ADR-0010 สำหรับเหตุผลและสิ่งที่มาแทนที่)

## บริบท
บริษัทบังคับใช้มาตรฐาน project แบบ 3-tier กับ backend project ทั้งหมด:

- **Domain** — ไม่มี dependency กับ project อื่น; entity, enum, DTO, model ทั่วไป, core interface
- **Service** — reference เฉพาะ Domain; `DbContext` + การตั้งค่า EF Core, business logic/service, service interface, การเชื่อมต่อภายนอก
- **API** — reference ทั้ง Service และ Domain; controller/endpoint, middleware, auth/JWT, configuration, การลงทะเบียน DI
- ทิศทางของ dependency: API → Service → Domain

ADR-0001 ได้แบ่ง backend แบบ **แนวตั้ง** ออกเป็นห้า Module อยู่แล้ว (Notes, Organization, AI Categorization, Search, Identity) แต่ละ Module เป็น bounded context ที่ออกแบบให้แยกออกได้อย่างอิสระในอนาคต โดยมีขอบเขตบังคับใช้ผ่านความเป็นเจ้าของ schema (ADR-0003) และการสื่อสารผ่าน event เท่านั้น (ADR-0002) การใช้มาตรฐาน 3-tier ตามตัวอักษรเป็นชุด project `Domain`/`Service`/`API` แบบ global ชุดเดียวสำหรับทั้ง backend จะแยกโค้ดตามชั้นเทคนิคแทนที่จะตาม Module — เป็นแกนตรงข้ามกับ ADR-0001 — ซึ่งจะรวม entity ของทั้งห้า Module ไว้ใน Domain assembly เดียว และรวม business logic ทั้งหมดไว้ใน Service assembly เดียว สิ่งนี้จะบั่นทอนเหตุผลหลักของ ADR-0001 ("การแยก Module ในภายหลังเป็นการเปลี่ยน deployment ไม่ใช่การออกแบบใหม่") และไม่มีที่อยู่ที่ชัดเจนสำหรับ Domain Event, การส่ง mediator หรือ Read Model projection (ADR-0002, ADR-0003)

## การตัดสินใจ
ใช้การแบ่งชั้น 3-tier ของบริษัท **แยกกันภายในแต่ละ Module** ไม่ใช่ครั้งเดียวแบบ global:

- **`<Module>.Domain`** — entity, enum, DTO และ core interface สำหรับ Module นั้นเท่านั้น ไม่มี dependency กับ project อื่น
- **`<Module>.Service`** — reference เฉพาะ `.Domain` ของ Module ตัวเอง เป็นเจ้าของ `DbContext` และการตั้งค่า EF Core ของ Module นั้น (จำกัดขอบเขตอยู่ที่ Postgres schema ของ Module นั้นเอง ตาม ADR-0003), business logic, service interface และการเชื่อมต่อภายนอก
- **`Jotwai.Api`** — โฮสต์ ASP.NET Core Web API ที่ใช้ร่วมกันเพียงหนึ่งเดียว (รักษาแนวคิด "deployable เดียว" จาก ADR-0001) reference `.Service` ของทุก Module (และโดยผ่าน `.Domain` ของมันด้วย) โฮสต์ controller/endpoint, middleware, การยืนยันตัวตนแบบ JWT, configuration และการลงทะเบียน DI สำหรับทุก Module

**กฎเข้มงวด**: `.Domain` หรือ `.Service` ของ Module หนึ่งต้องไม่ reference `.Domain` หรือ `.Service` ของอีก Module หนึ่งเด็ดขาด สิ่งนี้ทำให้ขอบเขตของ Module ที่ ADR-0001 อธิบายว่าบังคับใช้ผ่าน code review กลายเป็นขอบเขตที่ **บังคับใช้โดย compiler**

**การตัดสินใจนี้ไม่นำ project แบบ Shared Kernel เข้ามาใช้** การมี Shared Kernel จะสมเหตุสมผลก็ต่อเมื่อมีความต้องการข้าม Module ที่เป็นรูปธรรมและไม่สามารถแก้ไขได้ด้วยวิธีอื่น — ซึ่งยังไม่มีในตอนนี้ (ดู Domain Event ด้านล่าง)

**ตำแหน่งของ Domain Event (แก้ช่องว่างของ ADR-0002):** ประเภทของ Domain Event ของแต่ละ Module ถูกประกาศไว้ใน project `.Domain` ของ Module นั้น เป็น plain type ที่ไม่มี dependency กับ framework ตรรกะการ publish และ handle (การเชื่อมต่อ mediator) อยู่ใน project `.Service` ของ Module นั้น `.Service` ของ Module หนึ่งจะจัดการเฉพาะ event ที่ตัวเอง define และ publish เท่านั้น — ไม่มีทางที่จะ reference `.Domain` ของ Module อื่นเพื่อดูรูปร่าง event ของ Module นั้น การบริโภค event ข้าม Module (เช่น Search Module ในอนาคตที่ตอบสนองต่อ `NoteCreated` จาก Notes Module) ถูกเลื่อนออกไป — นี่คือความต้องการที่เป็นรูปธรรมซึ่งจะสมเหตุสมผลสำหรับการสร้าง shared contracts project ขนาดเล็กในภายหลัง ที่จำกัดขอบเขตเฉพาะ event marker type เมื่อมีผู้บริโภค event ข้าม Module รายแรกเกิดขึ้นจริง สำหรับ Issue #2 นั้น Identity Module ไม่มีผู้บริโภค event ข้าม Module — Module อื่นจะระบุตัวผู้ใช้ปัจจุบันผ่าน JWT bearer contract ที่บังคับใช้ที่ชั้น HTTP ของ `Jotwai.Api` ไม่ใช่ผ่าน Domain Event — ดังนั้นจึงยังไม่จำเป็นต้องมี shared project ในตอนนี้

**ADR-0003 (schema-per-module, DbContext-per-module) ไม่เปลี่ยนแปลงและได้รับการยืนยันอีกครั้ง** `DbContext` ของแต่ละ Module ตอนนี้มีที่อยู่ที่ชัดเจนและไม่คลุมเครือ — คือ project `.Service` ของตัวเอง — ซึ่งปิดช่องว่างความคลุมเครือที่ถ้อยคำ "DbContext … ใน Service" แบบเอกพจน์ของมาตรฐานบริษัทอาจทำให้เกิดขึ้นได้ (เช่น `DbContext` ที่ใช้ร่วมกันข้าม module)

## ผลที่ตามมา
- มี project โดยรวมมากขึ้น (ในที่สุดประมาณ 11 project ทั่วทั้งห้า Module: 5×`.Domain` + 5×`.Service` + 1×`Jotwai.Api`) แต่ขอบเขตของ Module ตอนนี้บังคับใช้โดย compiler ไม่ใช่แค่ review เท่านั้น
- Issue #2 จัดโครงร่างเฉพาะ `Jotwai.Identity.Domain`, `Jotwai.Identity.Service` และ `Jotwai.Api` — project ของอีกสี่ Module ที่เหลือจะถูกสร้างขึ้นก็ต่อเมื่อ ticket ของตัวเองเริ่มต้น เพื่อหลีกเลี่ยงการจัดโครงร่างล่วงหน้าโดยไม่จำเป็น
- หาก Module ในอนาคตต้องบริโภค Domain Event ของ Module อื่นจริง ๆ นั่นคือจุดกระตุ้นให้สร้าง shared contracts project ที่จำกัดขอบเขตแคบ (เฉพาะ event marker type) — ไม่ใช่ก่อนหน้านั้น
- ADR นี้แทนที่ผลกระทบด้านโครงสร้าง project ของ ADR-0001 (ซึ่งอธิบาย Module ไว้เฉพาะระดับ schema/การสื่อสารเท่านั้น ไม่ใช่ระดับ project) โดยไม่เปลี่ยนแปลงขอบเขต Module ของ ADR-0001 เอง
