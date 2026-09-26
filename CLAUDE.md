# Jotwai

แอปจดโน้ตแบบ "จดก่อน จัดทีหลัง" (capture first, organize later) ประกอบด้วย React SPA client + ASP.NET Core modular-monolith backend + PostgreSQL

## Routing — อ่านก่อนเสมอ แม้จะเป็นการแก้ไขเล็กน้อย

ก่อนแตะไฟล์ใดๆ ภายใต้ path ด้านล่าง คุณ**ต้อง** Read ไฟล์ที่ตรงกันก่อน — ไม่มีข้อยกเว้นแม้เป็นการแก้ไขเล็กน้อย

| Path prefix | อ่านไฟล์นี้ก่อน |
| --- | --- |
| `client/**` | `client/CLAUDE.md` |
| `backend/**` | `backend/CLAUDE.md` |

ก่อนเริ่มทำงานใน repo นี้ ให้อ่าน:
- **[ARCHITECTURE.md](ARCHITECTURE.md)** — แผนผัง Module, กลยุทธ์ข้อมูล, flow ของ AI categorization, frontend stack, deployment
- **[GLOSSARY.md](GLOSSARY.md)** — คำศัพท์เฉพาะของ domain (Note, Topic, Category, Suggestion, Module, Read Model, ...) ใช้คำเหล่านี้ตามที่กำหนด ห้ามคิดคำพ้องความหมายขึ้นเอง
- **[docs/adr/](docs/adr/)** — เหตุผลเบื้องหลังการตัดสินใจเชิงสถาปัตยกรรมแต่ละอย่าง อ่าน ADR ที่เกี่ยวข้องก่อนแก้ไขส่วนนั้น

## Tech Stack

- Client: React + TypeScript + Vite — ดู [client/CLAUDE.md](client/CLAUDE.md)
- Backend: ASP.NET Core (.NET 10) modular monolith + EF Core + PostgreSQL — ดู [backend/CLAUDE.md](backend/CLAUDE.md)

## โครงสร้าง Repo

- `client/` — React + TypeScript + Vite SPA
- `backend/` — ASP.NET Core modular monolith, global 3-tier (`Jotwai.Domain`, `Jotwai.Service`, `Web.Jotwai`) ตาม ADR-0010 แต่ละ Module (Notes, Organization, AI Categorization, Search, Identity) อยู่ภายใน 3 project นี้ในฐานะ feature ภายใน ไม่ใช่ project แยก
- `docs/adr/` — architecture decision records

## Ground rules

- Module ใน `backend/` สื่อสารกันผ่าน in-process Domain Events เท่านั้น โดยเป็น convention — ห้ามเรียกกันโดยตรงหรือ query ข้าม schema ระหว่าง module (ดู ADR 0001–0003, ADR-0010) แต่ละ Module ยังคงมี `DbContext` ของตัวเองที่ผูกกับ schema ของตัวเองภายใน `Jotwai.Service` แม้ project จะถูกแบ่งแบบ global ก็ตาม
- เนื้อหา Note เป็น Markdown; editor (Tiptap) เป็นเพียง view ของข้อมูลนั้น ไม่ใช่ source of truth
- ห้ามเพิ่ม real-time/push infrastructure, semantic search, การแชร์โน้ต หรือ version history โดยไม่ปรึกษาผู้ใช้ก่อน — สิ่งเหล่านี้เป็น non-goal ที่ระบุไว้ชัดเจนสำหรับ scope ปัจจุบัน (ดู ARCHITECTURE.md)
- ห้ามรัน `git commit` แทนผู้ใช้เด็ดขาด แม้ skill/workflow (เช่น `/implement`) จะบอกให้ commit เป็นหนึ่งในขั้นตอนก็ตาม ให้ implement, verify (build, tests) แล้วหยุด — ผู้ใช้เป็นคน commit เอง

## Security Rules

- ห้าม hardcode API key, token, password หรือ credential ใดๆ ในซอร์สโค้ด — ค่าจริงต้องมาจาก config เท่านั้น: อ่านผ่าน `IConfiguration`/`IOptions<T>` โดยค่าจริงตั้งไว้ที่ `appsettings.Development.json` (เครื่อง dev, ไฟล์นี้ไม่ commit) หรือ environment variable / `dotnet user-secrets` (environment อื่นๆ) — ดูตัวอย่างจริงที่ `Jotwai.Service/GeminiOptions.cs`
- ไฟล์หรือรูปแบบไฟล์ใดๆ ที่ตามมาตรฐานทั่วไปมักใช้เก็บข้อมูลอ่อนไหว/ความลับ (เช่น per-environment config อย่าง `appsettings.<Environment>.json`, ไฟล์ credential, keystore, certificate) ต้องเพิ่มเข้า `.gitignore` เสมอตั้งแต่สร้างไฟล์ครั้งแรก แม้ตอนนั้นจะยังไม่มี secret จริงอยู่ในไฟล์ก็ตาม — ยกเว้นไฟล์ template ที่ระบุไว้ชัดเจนแล้วเท่านั้น (`.env.example`, `appsettings.Example.json`, `appsettings.json` เอง)
- ห้าม log ข้อมูลอ่อนไหว (password, token, secret, connection string)
- ห้าม commit `.env`, `.env.local` หรือไฟล์ใดๆ ที่มี secret หรือ credential
- `.env.example` เป็นไฟล์ env เดียวที่อนุญาตให้อยู่ใน version control — ใช้ค่า placeholder เท่านั้น
- ห้ามเปิดเผย stack trace หรือรายละเอียด error ภายในใน API response
- `appsettings.Example.json` ก็เป็น config template ที่ยอมรับได้เช่นเดียวกับ `.env.example` — ใช้ค่า placeholder เท่านั้น

## Definition of Done

งานจะถือว่าเสร็จสมบูรณ์ก็ต่อเมื่อ

- Build ผ่าน
- Tests ผ่าน
- Formatting และ analyzer ที่ตั้งค่าไว้ผ่าน โดยยืนยันผลใน CI ไม่ใช่แค่ในเครื่อง
- พฤติกรรมเดิมยังคงทำงานถูกต้อง
- ฟังก์ชันที่ร้องขอถูก implement ครบ
- ไม่มีไฟล์ที่ไม่จำเป็นถูกเพิ่มเข้ามา
- ไม่มีโค้ดที่ไม่เกี่ยวข้องถูกแก้ไข

## Agent skills

### Issue tracker

Issue อยู่ใน GitHub Issues ของ ghost08A/Jotwai โดยใช้ `gh` CLI ดู `docs/agents/issue-tracker.md`

### Domain docs

โครงสร้างแบบ single-context: ARCHITECTURE.md, GLOSSARY.md และ docs/adr/ อยู่ที่ root ของ repo ดู `docs/agents/domain.md`

### Backend tickets

Naming convention, checklist ป้องกัน over-engineering และ pre-done checklist สำหรับงานใน `backend/` ดู `docs/agents/backend.md`
