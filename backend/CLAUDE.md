# CLAUDE.md

ไฟล์นี้ให้คำแนะนำแก่ AI coding agent (Claude Code, Codex, ฯลฯ) เมื่อทำงานกับโค้ดใน repository นี้

**Frontend App:** ดู [client/CLAUDE.md](../client/CLAUDE.md)

กฎที่ใช้ร่วมกันทั้ง repo (security, Definition of Done) อยู่ที่
[../CLAUDE.md](../CLAUDE.md) — ไฟล์นี้ครอบคลุมเฉพาะสิ่งที่เจาะจงกับฝั่ง backend เท่านั้น

## Tech Stack

- .NET 10, ASP.NET Core Web API (C#)
- Entity Framework Core + Npgsql.EntityFrameworkCore.PostgreSQL
- ASP.NET Core Identity (`Microsoft.Extensions.Identity.Core`) + JWT bearer auth (`Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`)
- Swashbuckle (Swagger/OpenAPI)

## Architecture

- ดู [../ARCHITECTURE.md](../ARCHITECTURE.md), [ADR-0010](../docs/adr/0010-global-three-tier-layering.md) และ [ADR-0012](../docs/adr/0012-dbcontext-in-domain-for-mkt-loyalty-consistency.md) สำหรับภาพรวมทั้งหมด
- Business logic อยู่ใน `Jotwai.Service` เท่านั้น
- Controller (`Web.Jotwai`) ทำหน้าที่แค่ orchestrate request/response
- ห้ามเข้าถึง `DbContext` ตรงๆ จาก Controller
- นำ service ที่มีอยู่แล้วกลับมาใช้ก่อนสร้างใหม่
- คง dependency direction `Domain` → `Service` → `Web` ไว้เสมอ
- Module (Notes, Organization, Nerd Assistant, Search, Identity) สื่อสารกันผ่าน in-process Domain Events เท่านั้น โดยเป็น convention — ห้ามเรียกกันโดยตรงหรือ query ข้าม schema ระหว่าง module (ADR 0001–0003, ADR-0010)
- การอ่านข้อมูลข้าม module ต้องผ่าน Read Model ของ module นั้นเองที่ denormalize ไว้ และอัปเดตผ่านการ subscribe Domain Events — ห้าม join ข้าม schema ของ module อื่นด้วย SQL แม้ทุก schema จะอยู่ใน PostgreSQL database เดียวกันก็ตาม

## Project Structure

ใช้ global Domain → Service → Web layering ชั้นเดียว (ADR-0010 แทนที่ ADR-0009 ที่แบ่ง layering ต่อ Module) — โฟลเดอร์จัดตาม technical layer ไม่ใช่ตาม Module `DbContext`/EF configuration/Migrations อยู่ใน `Jotwai.Domain` ไม่ใช่ `Jotwai.Service` (ADR-0012 แทนที่ส่วนนี้ของ ADR-0010 เพื่อความสอดคล้องกับ mkt-loyalty):

```
.editorconfig
Jotwai.Domain/              # ไม่ reference project อื่นเลย (มี PackageReference ของ EF Core/Npgsql ตรงๆ)
    Interfaces/              # interface นำหน้าด้วย I สำหรับทุก Module (เช่น IAuthService)
    Models/                  # entity/domain model
        Database/             # DbContext + EF configuration + migration (ADR-0012)
            <Module>DbContext.cs        # หนึ่ง DbContext ต่อหนึ่ง Module ผูกกับ schema ของตัวเอง
            <Module>DbContextFactory.cs
            Configurations/    # EF Core IEntityTypeConfiguration<T> ต่อ entity
            Migrations/        # EF Core migration ต่อ Module
    RequestModels/           # request DTO
    ValidateModel/           # validation/result model
    ViewModels/               # response/view model
Jotwai.Service/              # reference Jotwai.Domain เท่านั้น (ไม่มี PackageReference ของ EF Core/Npgsql ตรงๆ — ได้ผ่าน Jotwai.Domain)
    ImplementServices/        # business logic (หนึ่ง *Service ต่อหนึ่ง interface)
    EventHandlers/
    Helper/
Web.Jotwai/                  # reference Jotwai.Service + Jotwai.Domain
    Controllers/
    Properties/
    ServiceRegistrations/       # DI wiring ทุกไฟล์ที่มี IServiceCollection extension method (ย้ายจาก Jotwai.Service มาที่นี่)
        Features/                 # ต่อ Module - รวม AddDbContext<...>() (ไม่ใช่ตัว DbContext class เอง จึงอยู่ที่นี่ได้)
            <Module>ServiceRegistration.cs
        CrossCutting/              # ไม่ผูกกับ Module ไหน (CORS, Domain Event mediator)
            CorsServiceRegistration.cs
            EventingServiceRegistration.cs
    Program.cs                  # เรียก ServiceRegistrations/* + RegisterAutoServices (auto-DI, ดู "Auto-DI" ด้านล่าง)
Jotwai.IntegrationTests/     # XUnit + Testcontainers.PostgreSql + WebApplicationFactory
```

### Rules

- คง dependency direction `Domain` / `Service` / `Web` ไว้เสมอ — `Web.Jotwai`
  ขึ้นกับ `Jotwai.Service` และ `Jotwai.Domain`; `Jotwai.Service` ขึ้นกับ
  `Jotwai.Domain` เท่านั้น; `Jotwai.Domain` ไม่ขึ้นกับอะไรที่เจาะจง project เลย
- โค้ดใหม่ให้ใส่ในโฟลเดอร์ตาม technical layer ที่ตรงกัน (`Interfaces/`, `Models/`,
  `Models/Database/` (`DbContext`, `Configurations/`, `Migrations/`), `RequestModels/`,
  `ValidateModel/`, `ViewModels/` ใน `Jotwai.Domain`; `ImplementServices/` ใน
  `Jotwai.Service`) — ขอบเขตของ Module ถูกบังคับด้วย convention ของ Domain Events และ
  code review ไม่ใช่ด้วยโครงสร้างโฟลเดอร์ ดังนั้นอย่าคิดโฟลเดอร์ย่อยแยกตาม Module ขึ้นมาเอง
- แต่ละ Module มี `DbContext` ของตัวเอง (เช่น `IdentityDbContext`) ผูกกับ
  schema PostgreSQL ของตัวเอง — ห้ามใช้ `AppDbContext` ร่วมกัน (ADR-0012:
  `DbContext` อยู่ใน `Jotwai.Domain/Models/Database/` ไม่ใช่ `Jotwai.Service`)

## Coding Convention

### Naming

| Pattern            | ใช้กับ                                                 |
| ------------------- | -------------------------------------------------------- |
| `PascalCase`         | Class, method, property, public field, constant     |
| `camelCase`          | Local variable, method parameter                          |
| `_camelCase`         | Private field                                               |
| `IPascalCase`        | Interface (เช่น `IAuthService`)                             |
| `Async` suffix        | Async method (เช่น `RegisterAsync`, `LoginAsync`)             |
| `Service` suffix      | Service implementation (เช่น `AuthService`)                  |
| `Is`/`Has`/`Can` prefix | Boolean property/variable (เช่น `IsActive`, `HasRefreshToken`) |

ใช้คำศัพท์ของ Jotwai จาก [../CONTEXT.md](../CONTEXT.md) ก่อนแหล่งอื่นเสมอ

### Auto-DI (`IXxxService` / `XxxService`)

`RegisterAutoServices(builder)` (`private static` method ท้าย `Web.Jotwai/Program.cs`
โดยตรง — ตาม pattern ของ mkt-loyalty ที่เขียน logic นี้ไว้ใน host โดยตรง ไม่ห่อเป็น
extension method แยกใน `*.Service`, เรียกครั้งเดียวก่อน `builder.Build()`) ใช้ reflection
ผูก DI ให้อัตโนมัติสำหรับ interface/class ที่ตั้งชื่อตรงตาม pattern นี้เท่านั้น — **ห้าม**
`AddScoped<IXxxService, XxxService>()` มือใน `<Module>ServiceRegistration.cs` อีกถ้าเข้าเงื่อนไขข้อ 1-3:

1. Interface อยู่ใน `Jotwai.Domain/Interfaces/` ชื่อ `I<Name>Service` (ลงท้ายด้วย `Service` ตัว S ใหญ่)
2. Class อยู่ใน `Jotwai.Service/ImplementServices/` ชื่อ `<Name>Service` — ตัด `I` ออกจากชื่อ interface แล้วต้องตรงกับชื่อ class **เป๊ะ** (case-sensitive, ห้ามมีคำต่อท้าย/นำหน้าเพิ่ม)
3. ตั้งใจให้ lifetime เป็น **Scoped** เท่านั้น — ถ้าต้องการ Singleton/Transient หรือ implementation ที่ไม่ได้ inject ตรงๆ (เช่นผ่าน `AddHttpClient<TInterface, TImplementation>`) ต้องประกาศมือใน `<Module>ServiceRegistration.cs` เหมือนเดิม ห้ามใช้ auto-DI

ตัวที่ **ไม่เข้าเงื่อนไข** และยังต้องประกาศมือเหมือนเดิม: `DbContext`, `IEventHandler<T>`
(ชื่อไม่ลงท้าย `Service`), `IAiProvider`/`GeminiAiProvider` (ผูกผ่าน `AddHttpClient`, ชื่อ
implementation ไม่ลงท้าย `Service`), singleton อย่าง `IEventPublisher`/`InProcessEventPublisher`

ถ้าตั้งชื่อ interface/class ไม่ตรง pattern (เช่นสะกดผิด, เว้นคำนำหน้า) auto-DI จะ **เงียบๆ
ไม่ผูกให้** (ไม่มี exception ตอน build) แล้วจะไป fail ตอน runtime ด้วย
`InvalidOperationException: Unable to resolve service` แทน — เวลาสร้าง service ใหม่ตาม
pattern นี้ ให้เช็คชื่อให้ตรงเป๊ะก่อนเสมอ

### Coding Rules

- จัดการค่า null อย่างปลอดภัย
- ห้ามกลืน exception เงียบๆ
- เขียน method ให้ focus และสั้น
- หลีกเลี่ยง logic ที่ซ้ำกัน แยก helper method เฉพาะตอนที่ช่วยเรื่อง reuse หรือความชัดเจนจริงๆ
- ใช้ async/await สำหรับงาน I/O เป็นหลัก
- ห้ามใช้ `.Result` หรือ `.Wait()`
- Log ผ่าน `ILogger<T>` ที่ inject มา — ห้ามใช้ `Console.WriteLine`
- Register service ด้วย DI lifetime ที่ถูกต้อง (`Scoped` สำหรับ state ที่ผูกกับ request, `Singleton` สำหรับของที่ stateless/shared, `Transient` ใช้เท่าที่จำเป็น) — ห้าม resolve `Scoped` service จาก `Singleton`

### Don't over-engineer

สร้างเฉพาะสิ่งที่ ticket ปัจจุบันต้องการเท่านั้น ห้ามสร้าง:

- Shared kernel, generic repository หรือ repository ใดๆ โดยไม่มีความจำเป็นที่ชัดเจน
- Base class หรือ abstraction ที่ไม่จำเป็น
- Interface ที่มี implementation เดียวและไม่มีเหตุผลว่าจะมีตัวที่สองในอนาคต
- Event handler หรือ subscription สำหรับ event ที่ยังไม่มีใคร publish
- `DbContext`/schema ของ feature ที่ยังไม่มีอยู่จริง
- Authentication/authorization abstraction ที่เกินความจำเป็นของ feature ปัจจุบัน

### API Rules

- คง response format ของ API ให้สอดคล้องกับ endpoint ที่มีอยู่แล้ว
- ห้ามเปลี่ยนชื่อ route, request model หรือ response model เว้นแต่จำเป็นจริงๆ
- คืนข้อความ error ที่มีความหมาย โดยไม่เปิดเผยรายละเอียดภายในที่อ่อนไหว

### Database / EF Core Rules

- ห้ามแก้ database schema เว้นแต่ถูกร้องขอชัดเจน
- ห้ามเปลี่ยนชื่อ entity class หรือ property ที่ EF generate ไว้
- ใช้ DbContext และ entity configuration pattern ที่มีอยู่แล้ว (`IEntityTypeConfiguration<T>` ใน `Configurations/`)
- หลีกเลี่ยง N+1 query
- ใช้ `AsNoTracking()` สำหรับ query ที่อ่านอย่างเดียว
- ใช้ `Include()` เฉพาะตอนจำเป็นเท่านั้น
- หลีกเลี่ยงการโหลด navigation property ที่ไม่ได้ใช้
- Generate/verify EF Core migration ทุกครั้งที่ model เปลี่ยน

### Editor Config

Repo นี้บังคับ formatting ผ่าน `.editorconfig` ที่ root ของ repo AI agent ไม่มีกลไก auto-load editor config file แบบที่ IDE ทำได้ — กฎที่แท้จริงจึงถูกเขียนซ้ำไว้ที่นี่เพื่อให้อยู่ใน context จริงๆ ต้องทำตามในทุกไฟล์ที่คุณเขียนหรือแก้ไข อย่าเดา indentation จากการดูโค้ดรอบๆ

- Indent: 4 spaces (ค่า default ของทั้ง repo)
- Line ending: LF, UTF-8, ต้องมี final newline
- ตัด trailing whitespace (ยกเว้น Markdown ที่ trailing space มีความหมายสำหรับ line break)
- Line ending ถูกบังคับโดย `.gitattributes` ที่ root ไม่ใช่ Git setting ในเครื่องของคุณ — ห้าม "แก้" มันเองด้วยมือ และห้ามเปลี่ยน `core.autocrlf` เพื่อหนี complaint จาก formatter
- `.vscode/settings.json` ถูก commit ไว้และ pin กฎเดียวกันสำหรับ editor มันไม่ใช่ personal configuration — ห้ามเพิ่ม theme, font หรือ path เฉพาะเครื่องเข้าไป และห้ามผ่อนปรน formatting key ให้ตรงกับไฟล์ที่ผิดอยู่แล้ว
- `.csproj`/`.props`/`.targets` ใช้ 2 spaces แทนค่า default 4-space

ถ้า formatter รายงาน error เรื่อง line-ending หรือ whitespace ในไฟล์ที่คุณไม่ได้แตะ นั่นคือปัญหา configuration ของ repo ไม่ใช่ปัญหาโค้ด รายงานแล้วปล่อยไว้ ห้ามแก้ด้วยการ reformat ทั้ง repo และห้ามให้ไฟล์ที่ถูก reformat โดยไม่เกี่ยวข้องติดไปกับการเปลี่ยนแปลงของคุณ

### Formatting Display

- Date: `YYYY-MM-DD`
- DateTime: `YYYY-MM-DD HH:mm:ss`
- Time only: `HH:mm` / `HH:mm:ss`
- จำนวนเงินเก็บเป็น `decimal` — ห้ามใช้ `float`/`double` (ยังไม่มี Module ไหนใช้ตอนนี้ แต่กฎนี้ยังคงใช้ถ้ามีการเพิ่มในอนาคต)

### Comment Code

- `// Note:` — อนุญาตให้ใช้สำหรับ context ที่ช่วยให้เข้าใจง่ายขึ้น
- `// TODO:` — ถ้าเจอ **ในไฟล์ที่คุณกำลังแก้ไข** ให้แจ้งผู้ใช้ก่อนดำเนินการต่อ

### Before Creating New Code

ก่อนสร้าง helper, extension, service, DTO, ViewModel หรือ interface ใหม่ — ค้นหาก่อนว่ามี
implementation ที่เทียบเท่าอยู่แล้วหรือไม่ และนำกลับมาใช้ซ้ำเมื่อเป็นไปได้
ก่อนตั้ง type ใหม่ ให้ถามผู้ใช้เพื่อยืนยันชื่อ เว้นแต่ requirement จะระบุชื่อไว้ชัดเจนแล้ว

### Commands

```bash
dotnet restore
dotnet build
dotnet test
dotnet format --verify-no-changes   # บังคับตาม .editorconfig; ต้องผ่านใน CI ด้วย
```

## Testing & Quality

ก่อนถือว่างานเสร็จ:

1. รัน build command
2. แก้ compile error ให้หมด
3. ตรวจสอบ flow ที่ได้รับผลกระทบด้วยตัวเอง
4. เพิ่ม/แก้ test เมื่อ business logic เปลี่ยน
5. เปิด dependency Postgres (`docker-compose.yml`) ถ้าการเปลี่ยนแปลงเกี่ยวกับ persistence

### Test Runner

- XUnit ผ่าน `Jotwai.IntegrationTests`
- Test รันกับ **Testcontainers.PostgreSql** เท่านั้น — ห้ามใช้ Postgres ที่ติดตั้งในเครื่องโดยตรง
- `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) เป็นตัวขับ API แบบ in-process — ดู `JotwaiApiFactory.cs`
- ยังไม่มี unit-test project หรือ mocking library — ชุด test ปัจจุบันเป็น integration-only ถ้า business logic ล้วนๆ ซับซ้อนขึ้นจนคุ้มค่าที่จะมี unit test แยก ให้ตั้ง unit-test project แยกต่างหาก ไม่ใช่ปน unit test เข้ากับ `Jotwai.IntegrationTests`

### Unit Test Rules

ต้องมี test สำหรับ:

- Business logic
- Validation
- Permission / auth flow
- Bug fix

#### Test Style

- ใช้รูปแบบ Arrange / Act / Assert
- ตั้งชื่อ test ให้ชัดเจน
- Test ทั้ง success และ failure case
- ห้าม test private method โดยตรง
- เน้น test public behavior

### Rules

- **ห้ามแก้ test เพื่อให้มันผ่าน** เมื่อ test พังหลังจากแก้โค้ด ให้หยุดแล้วรายงานว่า test ไหนพังและเพราะอะไร — ตัดสินใจว่า _โค้ด_ regress จริงหรือ expected behavior เปลี่ยนไปจริงๆ สรุปผลกระทบ แล้วรอการอนุมัติอย่างชัดเจนก่อนแตะ test ใดๆ อย่าคิดเอาเองว่า test ผิดเพียงเพราะมันขึ้นสีแดง
