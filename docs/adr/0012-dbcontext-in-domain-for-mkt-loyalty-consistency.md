# ADR 0012: ย้าย DbContext เข้า `Jotwai.Domain` เพื่อความสอดคล้องกับ mkt-loyalty (แทนที่ ADR-0010)

## สถานะ
ยอมรับแล้ว แทนที่ ADR-0010

## บริบท

ADR-0010 กำหนดให้ `Jotwai.Domain` เป็น leaf project ที่ **ไม่มี dependency กับอะไรเลย** (รวมถึง EF Core) โดยยึดหลัก Persistence Ignorance — `DbContext`, EF configuration (`IEntityTypeConfiguration<T>`) และ EF Core Migrations ทั้งหมดอยู่ใน `Jotwai.Service` แทน เหตุผลเดิมคือ Domain ควรเป็น "หน้าตาของข้อมูล + สัญญา" ที่ไม่รู้จักว่าจะถูก persist ยังไง

ทีมที่ดูแล Jotwai ดูแล **mkt-loyalty** (`Loyalty-Merchant`/`Loyalty-Customer`) คู่กันอยู่แล้ว และจะดูแลต่อไป mkt-loyalty ใช้โครงสร้าง 3-tier แบบเดียวกัน (`Shared.Domain` → `Shared.Service` → `Web.*`) แต่วาง `DbContext` (`loyaltyContext : DbContext`) ไว้ใน `Shared.Domain/Models/` ตรงๆ ปนกับ entity model เลย — ไม่ได้แยก Persistence layer ออกมาต่างหาก ตรวจสอบโค้ดจริงแล้วยืนยันว่า `Shared.Domain.csproj` มี `PackageReference` ของ `Npgsql.EntityFrameworkCore.PostgreSQL` ตรงๆ และเป็น leaf project (ไม่มี `ProjectReference` ไปที่ไหนเลย) เหมือน `Jotwai.Domain`

ทีมตัดสินใจว่า **ความสอดคล้องของโครงสร้างข้ามสองโปรเจกต์พี่น้อง (consistency)** มีค่ามากกว่าความถูกต้องเชิงทฤษฎีของ Persistence Ignorance ในกรณีนี้ — วิศวกรที่สลับไปมาระหว่าง Jotwai กับ mkt-loyalty ควรเจอโครงสร้างที่ "อ่านแล้วคุ้น" แบบเดียวกัน

หมายเหตุ: mkt-loyalty เป็น **Database-First** (สร้างด้วย EF Core Power Tools, ไม่มี `Migrations/` folder, ไม่มี `IEntityTypeConfiguration<T>` แยกไฟล์ — config รวมอยู่ใน `OnModelCreating()` เดียว) ต่างจาก Jotwai ที่เป็น **Code-First** พร้อม EF Core Migrations การตัดสินใจนี้จำกัดเฉพาะ **ตำแหน่งของ DbContext เท่านั้น** ไม่ได้เปลี่ยน workflow การพัฒนา schema ของ Jotwai — Jotwai ยังคง Code-First + EF Core Migrations ตามเดิม เพราะ schema ยังอยู่ระหว่างพัฒนาเร็วๆ และ Migrations ยังมีประโยชน์อยู่

## การตัดสินใจ

**`Jotwai.Domain` ไม่ใช่ leaf project ที่ปลอด dependency อีกต่อไป** — มี `PackageReference` ของ `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design` และ `Npgsql.EntityFrameworkCore.PostgreSQL` ตรงๆ (ยังคงไม่มี `ProjectReference` ไปที่ไหนเลย — เป็น leaf ในแง่ project reference เหมือนเดิม เปลี่ยนแค่เรื่อง package)

**โครงสร้างใหม่ของ `Jotwai.Domain`:**
```
Jotwai.Domain/
    Interfaces/
    Models/
        <entity classes เดิม>
        Database/
            <Module>DbContext.cs          # เช่น NotesDbContext.cs
            <Module>DbContextFactory.cs   # design-time factory สำหรับ dotnet ef migrations
            Configurations/
                <Entity>Configuration.cs  # IEntityTypeConfiguration<T>
            Migrations/
                <Module>/                 # EF Core Migrations ต่อ module (Notes/, Organization/, AiCategorization/)
                <root files>              # Identity module (ยังไม่มี subfolder ของตัวเอง เหมือนเดิม)
    RequestModels/
    ValidateModel/
    ViewModels/
```

**`Jotwai.Service` ยังคงมี business logic เหมือนเดิม** (`ImplementServices/`, `Helper/`) แต่ **ไม่มี `PackageReference` ของ EF Core/Npgsql ตรงๆ อีกต่อไป** — ได้ package เหล่านี้แบบ transitive ผ่าน `ProjectReference` ไปที่ `Jotwai.Domain` (ยกเว้น `Microsoft.EntityFrameworkCore.Design` ที่ตั้ง `PrivateAssets="all"` ไว้ ไม่ไหลข้าม project เพราะเป็น dev-time tool เท่านั้น — ถูกต้องแล้วที่ Design อยู่แค่ที่ Domain)

`*ServiceRegistration.cs` (`AddNotesFeature`, `AddIdentityFeature`, ...) **ยังคงอยู่ใน `Jotwai.Service` เหมือนเดิม** — `services.AddDbContext<NotesDbContext>(...)` เป็น DI wiring ไม่ใช่ตัว `DbContext` class เอง ไม่ผิดหลัก layering ที่ตั้งไว้

**Namespace**: `Jotwai.Service` → `Jotwai.Domain.Models.Database` (และ `.Configurations`, `.Migrations.<Module>` ตามลำดับ) สำหรับไฟล์ที่ย้าย

## Trade-off ที่ยอมรับ

- **สูญเสีย testability แบบ mock `DbContext` ง่ายๆ** — `Jotwai.Domain` ผูกกับ EF Core โดยตรงแล้ว จะ unit-test business logic โดยไม่พึ่ง EF Core (เช่น mock ผ่าน pure interface) ทำได้ยากขึ้น (ปัจจุบัน Jotwai ยังเป็น integration-test only อยู่แล้ว ผลกระทบเชิงปฏิบัติจึงต่ำในตอนนี้)
- **สูญเสียความสามารถ swap persistence implementation ได้อิสระ** — `Jotwai.Domain` ไม่ใช่ "หน้าตาข้อมูลล้วนๆ" ที่ไม่รู้จัก database อีกต่อไป
- **ทางแยกตัว module ในอนาคตยังคงอยู่ที่ระดับ schema/DbContext เหมือนเดิม** (แต่ละ module ยังมี `DbContext` ของตัวเอง ผูก schema ของตัวเอง) — จุดนี้ไม่เปลี่ยนจาก ADR-0010 ยังคงเป็น "ขอบเขตที่แข็งแรงเพียงหนึ่งเดียว" สำหรับการแยก module ในอนาคต การย้ายไฟล์ไปอยู่ project ไหนไม่กระทบ boundary นี้

สิ่งที่ **ไม่เปลี่ยน** จาก ADR-0010 (ยังใช้เหตุผลเดิมได้):
- ทิศทาง dependency โดยรวม `Web.Jotwai → Jotwai.Service → Jotwai.Domain` (Service ยัง reference แค่ Domain)
- การสื่อสารข้าม module ผ่าน in-process Domain Event เท่านั้น (ADR-0001, ADR-0002)
- การแยก schema ต่อ module (ADR-0003) — แต่ละ `DbContext` ยังผูกกับ schema ของตัวเอง
- Jotwai ยังคง Code-First + EF Core Migrations (ต่างจาก mkt-loyalty ที่เป็น Database-First) — ไม่ align ส่วนนี้

## ผลที่ตามมา

- โครงสร้าง `Jotwai.Domain`/`Jotwai.Service` อ่านแล้วคุ้นเคยตรงกับ `Shared.Domain`/`Shared.Service` ของ mkt-loyalty มากขึ้น
- `backend/CLAUDE.md` ต้องอัปเดต Project Structure ให้ตรงกับตำแหน่งไฟล์ใหม่
- Migration ที่มีอยู่แล้วทั้งหมด (Identity, Notes, Organization, AiCategorization) ถูกย้าย namespace แต่เนื้อหา `Up()`/`Down()` ไม่เปลี่ยน — ไม่กระทบ schema ที่มีอยู่จริงในฐานข้อมูล
- แทนที่ ADR-0010 ทั้งฉบับ (ดูสถานะของ ADR นั้น) — แนวคิดเรื่อง module boundary ของ ADR-0001/0002/0003 ยังใช้ได้ทั้งหมด มีเพียงตำแหน่งไฟล์ของ `DbContext`/EF Core เท่านั้นที่เปลี่ยน
