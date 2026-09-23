# ADR 0010: โครงสร้าง project แบบ three-tier ระดับ global ให้สอดคล้องกับ convention ของบริษัท (แทนที่ ADR-0009)

## สถานะ
ยอมรับแล้ว แทนที่ ADR-0009

## บริบท
ADR-0009 นำเสนอคู่ project Domain/Service *ต่อ Module* เพื่อให้ได้ขอบเขต Module ที่ ADR-0001 เคยอธิบายว่าบังคับใช้ผ่าน review เท่านั้น ให้กลายเป็นแบบบังคับใช้โดย compiler หลังจากทบทวนเทียบกับแบบแผนทางวิศวกรรมที่มีอยู่แล้วของบริษัท — โปรเจกต์ Loyalty-Merchant — พบว่าแบบแผนนั้นใช้โครงสร้าง three-tier แบบ **global** เพียงชุดเดียวต่อ backend (Domain project หนึ่งชุด, Service project หนึ่งชุด, Web project หนึ่งชุด) ไม่ใช่คู่ project ต่อ feature เพื่อให้สอดคล้องกับแบบแผนของทีม เครื่องมือ และความคาดหวังของ CI/onboarding ที่มีอยู่แล้ว Jotwai จึงย้อนกลับไปใช้โครงสร้าง global นั้น บางส่วนของ Issue #2 ถูกจัดโครงร่างไปแล้วตามรูปแบบต่อ module ของ ADR-0009 งานนั้นกำลังถูกจัดโครงสร้างใหม่ให้ตรงกับการตัดสินใจนี้ก่อนที่ ticket จะเสร็จสิ้น

## การตัดสินใจ

**โครงสร้าง repository:**
```
Jotwai/
├── client/
└── backend/
    ├── Jotwai.Domain/
    ├── Jotwai.Service/
    ├── Web.Jotwai/
    └── Jotwai.sln
```
(`server/` ถูกเปลี่ยนชื่อเป็น `backend/`)

**Project** (หนึ่งชุดสำหรับทั้ง backend ไม่ใช่หนึ่งชุดต่อ feature):
- **`Jotwai.Domain`** — ไม่มี dependency กับ project อื่น entity/model, enum, request model, view model, validation model และ interface สำหรับ *ทุก* feature จัดกลุ่มตาม **โฟลเดอร์ชั้นเทคนิค** (`Models/`, `Enums/`, `Interfaces/`, `RequestModels/`, `ValidateModel/`, `ViewModels/`) — ตรงตามแบบแผนของ Loyalty-Merchant ไม่ได้จัดกลุ่มตามโฟลเดอร์ย่อยของแต่ละ feature
- **`Jotwai.Service`** — reference เฉพาะ `Jotwai.Domain` เท่านั้น business logic (`ImplementServices/`), helper (`Helper/`) และ `DbContext` + configuration ของ EF Core ของแต่ละ feature (ดูกลยุทธ์ DbContext ด้านล่าง)
- **`Web.Jotwai`** — reference ทั้ง `Jotwai.Service` และ `Jotwai.Domain` Controller, `Program.cs`, middleware/configuration, `ExceptionHandler.cs`
- ทิศทางของ dependency: `Web.Jotwai` → `Jotwai.Service` → `Jotwai.Domain`; `Jotwai.Domain` → ไม่มีอะไรเลย

Identity (และทุก feature ในอนาคต — Notes, Organization, AI Categorization, Search) อยู่ภายในสาม project นี้ในฐานะ feature ภายใน **ไม่ใช่** คู่ project แยกต่างหาก ไม่มี `src/Modules`, ไม่มี project `<Feature>.Domain`/`<Feature>.Service`

**กลยุทธ์ DbContext** (สิ่งนี้คือสิ่งที่รักษาการแยกข้อมูลของ ADR-0003 ให้คงอยู่ต่อไปแม้จะรวม project เข้าด้วยกัน): แต่ละ feature ยังคงมี **`DbContext` ของตัวเอง จำกัดขอบเขตอยู่ที่ Postgres schema ของตัวเอง** — `IdentityDbContext` → schema `identity` ในตอนนี้, `NotesDbContext` → schema `notes` ในภายหลัง และต่อ ๆ ไป ไม่มี `AppDbContext` ที่ใช้ร่วมกัน นี่เป็นทางเลือกที่ตั้งใจเพื่อรักษาการแยกในระดับ schema และเส้นทางการแยกตัวในอนาคต แม้ว่าการแบ่งแยกในระดับ project จะไม่มีอีกต่อไป

**การสื่อสารข้าม feature**: ไม่เปลี่ยนแปลงในสาระสำคัญจาก ADR-0002 — feature ต่าง ๆ ยังคงสื่อสารกันผ่าน in-process Domain Event สไตล์ MediatR โดยยังคงเป็นเพียง **แนวปฏิบัติ** ไม่ใช่กฎที่บังคับใช้โดย compiler ไม่มีขอบเขตระดับ project อีกต่อไปที่ป้องกันไม่ให้โค้ดของ feature หนึ่งเรียกโค้ดของอีก feature หนึ่งโดยตรง เรื่องนี้ตอนนี้อาศัยวินัยของทีมและ code review เพียงอย่างเดียวทั้งหมด การเรียก service-to-service โดยตรงข้าม feature นั้นไม่ใช่ค่าเริ่มต้นอย่างชัดเจน แม้ว่าจะไม่มีสิ่งใดในทางเทคนิคป้องกันได้อีกต่อไปแล้วก็ตาม

**ประเภทของ Domain Event** อยู่ใน `Jotwai.Domain` (ใช้ร่วมกันโดยทุก feature — ไม่มีปัญหาการ reference ข้าม project ที่ต้องแก้ไขอีกต่อไป เนื่องจากเป็น project เดียว) ตรรกะ publish/handle อยู่ใน `Jotwai.Service` คำถามเรื่อง "shared-kernel แบบแคบ" ของ ADR-0009 หมดความสำคัญไปแล้ว: เมื่อมี project Domain/Service เพียงชุดเดียว จึงไม่จำเป็นต้องมี contracts project แยกต่างหากสำหรับ event ข้าม feature อีกเลย

## ผลที่ตามมา
- สอดคล้องกับแบบแผนของบริษัทที่มีอยู่ (Loyalty-Merchant) — มีคุณค่าต่อความคุ้นเคยของทีม เครื่องมือ/เทมเพลต CI ที่ใช้ร่วมกัน และความคาดหวังใน onboarding สิ่งนี้ถูกตัดสินว่ามีน้ำหนักมากกว่าการแยกตัวที่บังคับใช้โดย compiler ที่ ADR-0009 เสนอ
- ขอบเขตของ feature (Identity และในภายหลัง Notes/Organization/AI Categorization/Search) ตอนนี้บังคับใช้ได้เพียงด้วยวินัยของโฟลเดอร์ภายใน `Jotwai.Domain`/`Jotwai.Service` (โครงสร้างโฟลเดอร์ที่ยึดชั้นเทคนิคเป็นหลัก ไม่ใช่แม้แต่ยึด feature เป็นหลัก) บวกกับ code review และแนวปฏิบัติเรื่อง event ของ MediatR นี่คือการสูญเสียความเข้มงวดที่แท้จริงและถูกยอมรับ เมื่อเทียบกับ ADR-0009
- ข้อกล่าวอ้างของ ADR-0001 ที่ว่า "การแยก Module ในภายหลังเป็นการเปลี่ยน deployment ไม่ใช่การออกแบบใหม่" ไม่ได้เป็นจริงอย่างสมบูรณ์อีกต่อไป: การแยก feature ในภายหลังหมายถึงการตัดโค้ดออกจาก project `Jotwai.Domain`/`Jotwai.Service` ที่ใช้ร่วมกัน ซึ่งเป็นการออกแบบใหม่ที่แท้จริง — แม้จะจำกัดอยู่ที่ชั้นข้อมูลก็ตาม การแยก `DbContext`/schema ต่อ feature เป็นขอบเขตที่แข็งแรงเพียงหนึ่งเดียวที่ยังคงอยู่ และเป็นสิ่งที่ป้องกันไม่ให้การออกแบบใหม่ในอนาคตนั้นกลายเป็นการเขียนชั้นข้อมูลใหม่ทั้งหมด
- แทนที่ ADR-0009 ทั้งหมด (ดูสถานะของ ADR นั้น) แนวคิดการออกแบบเรื่อง module/event ของ ADR-0001 และ ADR-0002 ยังคงใช้ได้ มีเพียงการนำไปปฏิบัติจริง (โครงสร้าง project) เท่านั้นที่เปลี่ยนแปลง
- Issue #2 ถูกปรับขอบเขตใหม่ให้สร้าง `Jotwai.Domain`, `Jotwai.Service`, `Web.Jotwai` (พร้อม test project) ภายใต้ `backend/` โดยมี entity, `DbContext`, migration และการลงทะเบียน DI ของ Identity อยู่ภายในสิ่งเหล่านี้ในฐานะ feature แรก — ไม่ใช่ในฐานะ project ที่ตั้งชื่อ Identity แยกต่างหาก
