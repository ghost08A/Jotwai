---
id: 16
title: "ตรวจสอบการ implement: ADR-0007 API-only backend, separately deployed static SPA"
type: wayfinder:research
status: open
assignee: null
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/16
---

## Question

โค้ดเบสปัจจุบัน implement การตัดสินใจที่บันทึกไว้ใน `docs/adr/0007-deployment-topology.md` หรือไม่? ตรวจสอบ Program.cs ว่ามีการ serve static-file/SPA หรือไม่, ตรวจสอบ controller ว่ามีรูปแบบเป็น pure-API หรือไม่, และตรวจสอบว่ามีการตั้งค่า CI/CD/Dockerfile/docker-compose ใดที่แสดงให้เห็นว่าสามารถ deploy แยกกันได้หรือไม่

## Finding (implement บางส่วนแล้ว)

**Re-audit 2026-09-25** (commit c87243b): `Program.cs` ไม่มีการเชื่อมต่อ `UseStaticFiles`/`UseSpaStaticFiles`/SPA-fallback ใดๆ ทุก controller (`AuthController`, `NotesController`, `TopicsController`, `CategoriesController`, `SuggestionsController`) เป็น `[ApiController]` แบบบริสุทธิ์ที่คืนค่า JSON — "เป็น API เท่านั้น ไม่ serve SPA" ยังคงเป็นจริง **CORS ถูกเพิ่มแล้ว** ระหว่าง [ticket 027 (frontend login flow)](027-feature-frontend-login.md): `Web.Jotwai/ServiceRegistrations/CrossCutting/CorsServiceRegistration.cs` + `AddCustomCors()` เรียกใน `Program.cs`, ต่อ middleware เป็น `UseRouting → UseCors → UseAuthentication → UseAuthorization` (ตั้งใจไม่ใช้ `UseHttpsRedirection` เพราะ preflight response redirect ไม่ได้ — มีคอมเมนต์อธิบายไว้ในโค้ด) gap เดิมเรื่อง CORS ปิดแล้ว

ยังไม่มี `.github/workflows/`, ไม่มี `Dockerfile` อยู่ที่ไหนเลย และ `docker-compose.yml` รันเพียง Postgres/pgAdmin เท่านั้น (ไม่มี service สำหรับ backend หรือ client) — ยังไม่มีสิ่งใดแสดงให้เห็นว่า SPA กับ API ถูก deploy แยกกันอย่างเป็นอิสระจริง

**ขาด**: การตั้งค่า deployment (Dockerfile/CI) ที่แสดงให้เห็นจริงว่า SPA และ API เป็น artifact แยกกัน

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่
