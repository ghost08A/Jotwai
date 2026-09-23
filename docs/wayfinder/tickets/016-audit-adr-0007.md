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

`Program.cs` ไม่มีการเชื่อมต่อ `UseStaticFiles`/`UseSpaStaticFiles`/SPA-fallback ใดๆ และ `AuthController` เป็น `[ApiController]` แบบบริสุทธิ์ที่คืนค่า JSON — ครึ่งหนึ่งของการตัดสินใจที่ว่า "เป็น API เท่านั้น ไม่ serve SPA" นั้นได้รับการปฏิบัติตามแล้ว แต่ไม่มี `.github/workflows/`, ไม่มี `Dockerfile` อยู่ที่ไหนเลย และ `docker-compose.yml` รันเพียง Postgres/pgAdmin เท่านั้น — ยังไม่มีสิ่งใดแสดงให้เห็นว่าทั้งสองส่วนถูก deploy แยกกันอย่างเป็นอิสระ นอกจากนี้ยังไม่มีการตั้งค่า CORS ซึ่ง ADR ระบุว่าเป็นผลลัพธ์ที่จำเป็นด้วย

**ขาด**: การตั้งค่า deployment (Dockerfile/CI) ที่แสดงให้เห็นจริงว่า SPA และ API เป็น artifact แยกกัน และการตั้งค่า CORS สำหรับ origin ของ SPA

ยังคงเปิดไว้ — หนี้ทางการ implement ไม่ใช่การทบทวนการตัดสินใจใหม่
