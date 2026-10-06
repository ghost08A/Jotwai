---
id: 62
title: "Research: ตัวเลือกการส่งอีเมลยืนยันสำหรับแอป ASP.NET Core"
type: wayfinder:research
status: closed
assignee: null
parent: ../map-v2.md
blocked_by: []
---

## Question

ตัวเลือกการส่งอีเมลยืนยันตัวตน (ลิงก์ยืนยันอีเมลตอนสมัคร) สำหรับ backend ASP.NET Core (.NET 10) ที่ใช้ ASP.NET Core Identity แบบ custom (ดู ADR-0011: `User`/`RefreshToken` POCO เอง ไม่ใช้ Identity เต็มชุด) และ deploy เป็น container: SMTP ตรง (เช่น MailKit), บริการส่งอีเมลแบบ API (เช่น SendGrid, Amazon SES, Resend, Mailgun ฯลฯ) และเครื่องมือจำลองสำหรับ dev (เช่น MailHog/smtp4dev) — ข้อเท็จจริงเรื่อง ราคา/โควตาฟรี, การตั้งค่าโดเมนส่ง (SPF/DKIM) ที่จำเป็นให้อีเมลไม่ตกสแปม, ลักษณะเอกสาร token ยืนยันที่เอกสารของ ASP.NET Core แนะนำ, และข้อจำกัดอื่นที่กระทบการตัดสินใจ — **รายงานข้อเท็จจริงเท่านั้น ห้ามเลือกแทนเจ้าของโปรเจกต์** เก็บที่ `docs/wayfinder/research/email-sending-options.md`

## Finding

รวบรวมเสร็จ (2026-10-06) ที่ [research/email-sending-options.md](../research/email-sending-options.md) — ข้อเท็จจริงล้วน ไม่เลือกแทนเจ้าของโปรเจกต์ ประเด็นหลัก: เอกสาร ASP.NET Core สร้าง token ยืนยันอีเมลผ่าน `UserManager` (Data Protection token อายุ default 1 วัน) แต่โปรเจกต์ไม่ใช้ `UserManager` ตาม ADR-0011; SMTP ตรงตั้งค่าให้ปลอดภัยยาก และ Microsoft ไม่แนะนำ `System.Net.Mail.SmtpClient` ให้ใช้ MailKit; โควตาฟรี/ราคาของบริการส่งเมล (ตรวจ 2026-10-06 เปลี่ยนได้): Resend 3,000/เดือน, Mailgun 100/วัน, SendGrid ทดลอง 60 วัน, SES $0.10/1,000 แต่บัญชีใหม่อยู่ใน sandbox จนกว่าจะขอ production access; เครื่องมือ dev: Mailpit/smtp4dev ยังพัฒนาอยู่ ส่วน MailHog เหมือนไม่มีการพัฒนาต่อ; Gmail ต้องการ SPF หรือ DKIM + PTR + TLS

ข้อจำกัดของ research: หน้าเว็บส่วนใหญ่ดึงผ่านตัวสรุป ไม่ใช่อ่านดิบ; ไม่ได้ตรวจเอกสารตั้งโดเมนละเอียดของ Resend/Mailgun/SendGrid, พฤติกรรม Data Protection key ใน container และ provider อื่น
