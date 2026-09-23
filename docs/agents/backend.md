# Backend tickets

Checklist สำหรับ agent ที่ implement ticket ฝั่ง backend (`backend/`) เพิ่มเติมจาก architecture ใน ADR-0001–0003, ADR-0010, ADR-0011

## อย่า over-engineer

สร้างเฉพาะสิ่งที่ ticket ปัจจุบันต้องการเท่านั้น ห้ามสร้าง:

- Shared kernel, generic repository หรือ repository ใดๆ โดยไม่มีความจำเป็นที่ชัดเจน
- Base class หรือ abstraction ที่ไม่จำเป็น
- Interface ที่มี implementation เดียวและไม่มีเหตุผลว่าจะมีตัวที่สองในอนาคต
- Event handler หรือ subscription สำหรับ event ที่ยังไม่มีใคร publish
- `DbContext`/schema ของ feature ที่ยังไม่มีอยู่จริง
- Authentication/authorization abstraction ที่เกินความจำเป็นของ feature ปัจจุบัน

## Naming

- Class, method: `PascalCase`
- Variable: `camelCase`
- Interface: นำหน้าด้วย `I` (`IAuthService`)
- Service implementation: ลงท้ายด้วย `Service` (`AuthService`)
- Async method: ลงท้ายด้วย `Async` (`RegisterAsync`, `LoginAsync`)
- Boolean property/variable: นำหน้าด้วย `Is`/`Has`/`Can` (`IsActive`, `HasRefreshToken`)

ใช้คำศัพท์ของ Jotwai จาก [`GLOSSARY.md`](../../GLOSSARY.md) ก่อนแหล่งอื่นเสมอ

## ก่อนถือว่างานเสร็จ

1. ตรวจสอบว่า project reference ตรงกับ dependency direction ใน ADR-0010 (`Web.Jotwai` → `Jotwai.Service` → `Jotwai.Domain`; `Jotwai.Domain` → ไม่ขึ้นกับอะไร)
2. Build ทั้ง solution; ต้องไม่มี warning และไม่มี error
3. Generate/verify EF Core migration ทุกครั้งที่ model เปลี่ยน
4. เปิด dependency Postgres (`docker-compose.yml`) ถ้าการเปลี่ยนแปลงเกี่ยวกับ persistence
5. รัน integration test suite — ต้องรันกับ **Testcontainers** เท่านั้น ห้ามใช้ Postgres ที่ติดตั้ง/native ในเครื่อง
6. ตรวจสอบ `git diff` เพื่อหาไฟล์ค้างจาก structure หรือแนวทางเดิมที่ไม่ได้ใช้แล้ว
7. อย่าปิด GitHub issue จนกว่า implementation และ test จะเสร็จสมบูรณ์จริงๆ
