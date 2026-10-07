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
- **[CONTEXT.md](CONTEXT.md)** — คำศัพท์เฉพาะของ domain (Note, Category, Description, Note Suggestion, Share Link, Attachment, Module, Read Model, ...) ใช้คำเหล่านี้ตามที่กำหนด ห้ามคิดคำพ้องความหมายขึ้นเอง (นิยามเป็นของรุ่น v2 — โค้ดปัจจุบันบางส่วนยังใช้ชื่อเดิม เช่น Topic จนกว่าจะ implement ดูหัวเอกสาร)
- **[requirements-v2.md](requirements-v2.md)** — ความต้องการของรุ่น v2 ที่ตัดสินใจครบแล้ว (ที่มา: `docs/wayfinder/map-v2.md`)
- **[docs/adr/](docs/adr/)** — เหตุผลเบื้องหลังการตัดสินใจเชิงสถาปัตยกรรมแต่ละอย่าง อ่าน ADR ที่เกี่ยวข้องก่อนแก้ไขส่วนนั้น

## Tech Stack

- Client: React + TypeScript + Vite — ดู [client/CLAUDE.md](client/CLAUDE.md)
- Backend: ASP.NET Core (.NET 10) modular monolith + EF Core + PostgreSQL — ดู [backend/CLAUDE.md](backend/CLAUDE.md)

## โครงสร้าง Repo

- `client/` — React + TypeScript + Vite SPA
- `backend/` — ASP.NET Core modular monolith, global 3-tier (`Jotwai.Domain`, `Jotwai.Service`, `Web.Jotwai`) ตาม ADR-0010 แต่ละ Module (Notes, Organization, Nerd Assistant, Search, Identity, Logging) อยู่ภายใน 3 project นี้ในฐานะ feature ภายใน ไม่ใช่ project แยก
- `docs/adr/` — architecture decision records

## การสื่อสารกับผู้ใช้

- คุยกับผู้ใช้เป็นภาษาไทยเสมอ (เอกสารในโปรเจกต์ก็เขียนเป็นภาษาไทยเช่นกัน ยกเว้นคำศัพท์เฉพาะทาง/โค้ด/ชื่อไฟล์ที่ต้องคงไว้เป็นภาษาอังกฤษเพื่อไม่ให้ตรงกับของจริงผิดเพี้ยน)
- เวลาต้องใช้คำเทคนิค ให้อธิบายกำกับด้วยภาษาง่ายๆ ที่คนทั่วไปเข้าใจได้ ไม่ใช่แค่ทิ้งศัพท์ไว้เฉยๆ
- สอน/อธิบายเพิ่มเติมระหว่างทางได้เลย ไม่ต้องตอบแค่แคบๆ ตรงคำถามอย่างเดียว — ผู้ใช้ไม่ได้เป็นสายเทคนิคลึกทุกเรื่อง

## Ground rules

- Module ใน `backend/` สื่อสารกันผ่าน in-process Domain Events เท่านั้น โดยเป็น convention — ห้ามเรียกกันโดยตรงหรือ query ข้าม schema ระหว่าง module (ดู ADR 0001–0003, ADR-0010) แต่ละ Module ยังคงมี `DbContext` ของตัวเองที่ผูกกับ schema ของตัวเองภายใน `Jotwai.Service` แม้ project จะถูกแบ่งแบบ global ก็ตาม
- เนื้อหา Note เป็น Markdown; editor (Tiptap) เป็นเพียง view ของข้อมูลนั้น ไม่ใช่ source of truth
- ห้ามเพิ่ม real-time/push infrastructure, semantic search, การทำงานร่วมกันของ Note (collaboration) หรือ version history โดยไม่ปรึกษาผู้ใช้ก่อน — สิ่งเหล่านี้เป็น non-goal ที่ระบุไว้ชัดเจนสำหรับ scope ปัจจุบัน (ดู ARCHITECTURE.md) ส่วนการแชร์ Note แบบอ่านอย่างเดียวผ่านลิงก์และการแนบไฟล์/รูป **อยู่ใน scope ของ v2 แล้ว** (ADR-0015, ADR-0018)
- ห้ามรัน `git commit` เองโดยไม่ได้รับอนุญาตชัดเจน แม้ skill/workflow (เช่น `/implement`) จะบอกให้ commit เป็นหนึ่งในขั้นตอนก็ตาม ให้ implement, verify (build, tests) แล้วหยุดรอผู้ใช้รีวิวก่อนเสมอ — commit ให้ได้ต่อเมื่อผู้ใช้บอกชัดเจนหลังรีวิวแล้วว่าอนุญาตให้ commit (เช่น "รีวิวแล้ว commit ได้เลย") เท่านั้น ห้ามสมมติเอาเองว่าได้รับอนุญาตแล้วจากบริบทอื่น

## Security Rules

- ห้าม hardcode API key, token, password หรือ credential ใดๆ ในซอร์สโค้ด — ค่าจริงต้องมาจาก config เท่านั้น: อ่านผ่าน `IConfiguration`/`IOptions<T>` โดยค่าจริงตั้งไว้ที่ `appsettings.Development.json` (เครื่อง dev, ไฟล์นี้ไม่ commit) หรือ environment variable / `dotnet user-secrets` (environment อื่นๆ) — ดูตัวอย่างจริงที่ `Jotwai.Service/GeminiOptions.cs`
- ไฟล์หรือรูปแบบไฟล์ใดๆ ที่ตามมาตรฐานทั่วไปมักใช้เก็บข้อมูลอ่อนไหว/ความลับ (เช่น per-environment config อย่าง `appsettings.<Environment>.json`, ไฟล์ credential, keystore, certificate) ต้องเพิ่มเข้า `.gitignore` เสมอตั้งแต่สร้างไฟล์ครั้งแรก แม้ตอนนั้นจะยังไม่มี secret จริงอยู่ในไฟล์ก็ตาม — ยกเว้นไฟล์ template ที่ระบุไว้ชัดเจนแล้วเท่านั้น (`.env.example`, `appsettings.Example.json`, `appsettings.json` เอง)
- ห้าม log ข้อมูลอ่อนไหว (password, token, secret, connection string)
- ห้าม commit `.env`, `.env.local` หรือไฟล์ใดๆ ที่มี secret หรือ credential
- `.env.example` เป็นไฟล์ env เดียวที่อนุญาตให้อยู่ใน version control — ใช้ค่า placeholder เท่านั้น
- ห้ามเปิดเผย stack trace หรือรายละเอียด error ภายในใน API response
- `appsettings.Example.json` ก็เป็น config template ที่ยอมรับได้เช่นเดียวกับ `.env.example` — ใช้ค่า placeholder เท่านั้น

## Learning-First Development Rule

**Never implement what the user cannot explain.**

เป้าหมายของการใช้ AI ในโปรเจกต์นี้ไม่ใช่เพียงทำงานให้เสร็จเร็วที่สุด แต่ต้องช่วยให้ผู้ใช้เรียนรู้และเข้าใจระบบไปพร้อมกับการพัฒนา

### กฎการทำงาน

1. **ห้ามรีบ Implement**
   - ก่อนแก้ Code ต้องอธิบายก่อนว่าต้องแก้อะไร และทำไม
   - ถ้าผู้ใช้ยังไม่เข้าใจ ให้หยุดและอธิบายเพิ่มเติม
2. **ทำทีละเรื่อง**
   - แบ่งงานใหญ่เป็นส่วนเล็ก ๆ ที่ผู้ใช้สามารถทำความเข้าใจและ Review ได้
   - ห้ามสร้างหรือแก้ Code จำนวนมากในครั้งเดียวโดยไม่จำเป็น
3. **Explain → Confirm → Implement**

   ```text
   วิเคราะห์
      ↓
   อธิบาย
      ↓
   ผู้ใช้เข้าใจและยืนยัน
      ↓
   Implement
      ↓
   อธิบาย Code ที่เปลี่ยน
      ↓
   ผู้ใช้ Review
      ↓
   Test
   ```

4. **เมื่อผู้ใช้บอกว่าอ่าน Code ไม่ออก**
   - ห้ามแก้ Code ต่อ
   - ห้ามข้ามไปส่วนถัดไป
   - ให้ย้อนกลับมาอธิบาย Code ทีละส่วนด้วยภาษาที่เข้าใจง่าย
   - ใช้ตัวอย่างหรือเปรียบเทียบได้เมื่อช่วยให้เข้าใจง่ายขึ้น
5. **หลัง Implement ต้องอธิบายสิ่งที่เปลี่ยน** อย่างน้อย:
   - แก้ไฟล์ไหน
   - แก้ส่วนไหน
   - Code ใหม่ทำอะไร
   - ทำไมต้องแก้
   - ข้อมูลไหลอย่างไร
   - มีผลกระทบกับส่วนอื่นหรือไม่
   - Requirement ข้อไหนที่การแก้นี้ตอบโจทย์
6. **ผู้ใช้ต้องเป็นคนตัดสินใจ**
   - AI สามารถเสนอทางเลือกและบอกข้อดีข้อเสียได้
   - AI ห้ามตัดสินใจเรื่องสำคัญแทนผู้ใช้
   - ถ้ามีหลายแนวทาง ให้หยุดและให้ผู้ใช้เลือกเมื่อการเลือกนั้นมีผลต่อ Architecture, Behavior หรือ Requirement
7. **ไม่ต้องอธิบายทุก syntax**
   - ไม่จำเป็นต้องอธิบายทุกบรรทัดหรือ syntax พื้นฐาน หากผู้ใช้เข้าใจอยู่แล้ว
   - เน้นสิ่งที่มีผลต่อการทำงานของระบบและเหตุผลในการออกแบบ

### หลักในการตัดสินว่าไปต่อได้หรือยัง

ก่อนเริ่มงานถัดไป ผู้ใช้ควรสามารถอธิบายได้ว่า:

**Code นี้ทำอะไร → ทำไมต้องมี → ข้อมูลไหลอย่างไร → มีผลกระทบอะไร**

ถ้าผู้ใช้ยังอธิบายไม่ได้ ให้หยุด Implementation และกลับไปอธิบายก่อน

ความเร็วไม่ใช่เป้าหมายสูงสุด ความเข้าใจของผู้ใช้คือส่วนหนึ่งของ Definition of Done

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

โครงสร้างแบบ single-context: ARCHITECTURE.md, CONTEXT.md และ docs/adr/ อยู่ที่ root ของ repo ดู `docs/agents/domain.md`

### Backend tickets

Naming convention, checklist ป้องกัน over-engineering และ pre-done checklist สำหรับงานใน `backend/` ดู `docs/agents/backend.md`
