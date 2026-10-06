# Research: ตัวเลือกการส่งอีเมลยืนยันตัวตนสำหรับแอป ASP.NET Core

- Ticket: [062](../tickets/062-research-email-sending-options.md)
- วันที่เก็บข้อมูล: 2026-10-06
- ขอบเขต: **รายงานข้อเท็จจริงจากแหล่ง primary เท่านั้น ไม่เลือก/ไม่แนะนำตัวเลือกแทนเจ้าของโปรเจกต์**
- หมายเหตุวิธีเก็บ: หน้าเว็บส่วนใหญ่ดึงผ่านเครื่องมือที่สรุปหน้าให้ (ไม่ใช่อ่านดิบทั้งหน้า) ยกเว้นหน้า Microsoft Learn และ AWS docs บางหน้าที่ได้เนื้อหาดิบ **ราคา/โควตาฟรีเปลี่ยนได้ ตัวเลขทั้งหมดตรวจเมื่อ 2026-10-06 ก่อนตัดสินใจจริงควรเปิดหน้าต้นทางซ้ำ** หน้าที่ดึงไม่ได้หรือข้อมูลไม่ครบจะระบุไว้ว่า "ไม่พบ"

## 1. สถานะใน repo (ข้อเท็จจริงจากโค้ด/ADR)

- ADR-0011: Identity Module ใช้ `User`/`RefreshToken` POCO (คลาสธรรมดา) และ `IdentityDbContext` ที่เขียนเอง **ไม่ใช้** `UserManager`/`SignInManager`/`IdentityUser` ของ ASP.NET Core Identity นำมาใช้แค่ `IPasswordHasher<User>` กับ password policy
- ค้นโค้ด `backend/` ไม่พบ `IEmailSender`, MailKit, `Smtp`, หรือการตั้งค่า `DataProtection`/`TokenLifespan` เลย (ยังไม่มีการส่งอีเมล)
- `docker-compose.yml` ปัจจุบันมีแค่ `postgres` และ `pgadmin` ยังไม่มี mail service

## 2. Token ยืนยันอีเมล: เอกสาร ASP.NET Core ว่าอย่างไร

ที่มา: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm (หน้า "Account confirmation and password recovery", ms.date 2026-05-11)

- ตัวอย่างในเอกสารสร้าง token ด้วย `UserManager.GenerateEmailConfirmationTokenAsync(user)` แล้วเข้ารหัสด้วย `WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code))` ก่อนใส่ในลิงก์ (query `userId` + `code`)
- token ที่ได้จาก Identity เป็นแบบ **Data Protection token** (token ที่เข้ารหัส/เซ็นด้วยกลไก Data Protection ของ ASP.NET Core ไม่ได้เก็บแถวใน DB) เอกสารระบุว่า "built-in Identity user tokens ... have a one day timeout" (อายุเริ่มต้น 1 วัน) ปรับได้ผ่าน `DataProtectionTokenProviderOptions.TokenLifespan` (ตัวอย่างใช้ 3 ชม.) หรือสร้าง `DataProtectorTokenProvider<TUser>` เองเพื่อให้อีเมลมีอายุต่างจาก token อื่น (ตัวอย่าง 4 ชม.)
- ข้อเท็จจริงที่เกี่ยวกับโปรเจกต์ (ประกอบ ADR-0011 กับเอกสารข้างต้น): ตัวอย่างทั้งหมดผูกกับ `UserManager<IdentityUser>` ซึ่งโปรเจกต์นี้ไม่ได้ใช้ เอกสารหน้านี้จึงไม่ได้ให้แนวทางสำหรับกรณี `User` แบบกำหนดเอง
- Data Protection ใน container: หน้า https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview มีตัวเลือกเก็บ key ที่ `PersistKeysToFileSystem`, `PersistKeysToDbContext`, `PersistKeysToAzureBlobStorage` และระบุตำแหน่ง default เป็น `%LOCALAPPDATA%` (ส่วนที่ว่า container ไม่มี volume แล้ว key หายหรือไม่ ผมไม่ได้ยืนยันจากข้อความหน้านี้ตรงๆ ในการเก็บครั้งนี้)
- เอกสารเตือนเรื่อง **เปิด email confirmation บนระบบที่มีผู้ใช้อยู่แล้ว**: ผู้ใช้เดิมจะถูกล็อกเอาท์เพราะยังไม่ confirm ทางออกที่เอกสารเสนอคือ ตั้ง confirmed ให้ผู้ใช้เดิมใน DB หรือส่งลิงก์ยืนยันเป็นชุด
- ตัวอย่างโค้ดในเอกสารปิด click tracking ของ SendGrid (`SetClickTracking(false, false)`) ในอีเมลที่มีลิงก์ยืนยัน (เอกสารไม่ได้อธิบายเหตุผลไว้ตรงนั้น)
- ส่วน "resend email confirmation" มีอยู่เป็นฟีเจอร์ในตัวอย่าง (ลิงก์ที่หน้า Login)

## 3. ตัวเลือก: SMTP ตรง

- เอกสาร ASP.NET Core หน้า accconfirm เขียนว่า "We recommend using SendGrid or another email service to send email rather than SMTP. SMTP is difficult to secure and set up correctly." (เวอร์ชันเก่า 3.1/5.0 ของหน้าเดียวกันเขียนว่า SMTP ยาก "so mail is not marked as spam")
- `System.Net.Mail.SmtpClient`: Microsoft ระบุว่า "We don't recommend that you use the SmtpClient class for new development because SmtpClient doesn't support many modern protocols. Use MailKit or other libraries instead." (https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient?view=net-10.0)
- MailKit (https://github.com/jstedfast/MailKit): ไลบรารี SMTP/POP3/IMAP สำหรับ .NET สร้างบน MimeKit ใบอนุญาต MIT (.NET Foundation) ใช้ `SmtpClient` ของตัวเอง (`Connect`/`Authenticate`/`Send`)
- ข้อสังเกตเชิงข้อเท็จจริง: SMTP ตรงต้องมี SMTP server ปลายทางเสมอ (ของ provider หรือ server ที่โฮสต์เอง) ตัวไลบรารีไม่ได้แก้เรื่อง SPF/DKIM/reputation ของ IP (ดูหัวข้อ 6)

## 4. ตัวเลือก: บริการส่งอีเมล (API/SMTP relay)

ราคา ตรวจ 2026-10-06 จากหน้า pricing ของแต่ละเจ้า

| บริการ | แผนฟรี/ทดลอง | แผนเสียเงินเริ่มต้น | หมายเหตุจากแหล่ง |
| --- | --- | --- | --- |
| Resend (https://resend.com/pricing) | Free: 3,000 อีเมล/เดือน, จำกัด 100/วัน, 3 โดเมน | Pro $20/เดือน 50,000 อีเมล (10 โดเมน, ไม่มีเพดานรายวัน) | ต้อง add และ verify โดเมนอย่างน้อย 1 โดเมนถึงส่งได้ มี `onboarding@resend.dev` ใช้ช่วงเริ่มต้นก่อนมีโดเมน (เอกสารหน้า introduction ไม่ได้ระบุข้อจำกัดของ address นี้) |
| Amazon SES (https://aws.amazon.com/ses/pricing/) | ลูกค้า AWS ใหม่ได้เครดิต Free Tier $200 ใช้ได้ 6 เดือนหลังสร้างบัญชี | ส่งออก $0.10 ต่อ 1,000 อีเมล (แบบ a la carte) | ดูเรื่อง sandbox ด้านล่าง |
| SendGrid / Twilio (https://www.twilio.com/en-us/products/email-api/pricing) | ทดลองฟรี 60 วัน 100 อีเมล/วัน ไม่ต้องใช้บัตร | Essentials เริ่ม $19.95/เดือน, Pro เริ่ม $89.95/เดือน | ไม่พบแผน "ฟรีถาวร" ในหน้านี้ ตัวอย่างในเอกสาร ASP.NET Core ใช้ SendGrid |
| Mailgun (https://www.mailgun.com/pricing/) | Free: 100 อีเมล/วัน, 1 custom domain | Basic $15/เดือน 10,000 อีเมล, Foundation $35/เดือน 50,000 | ส่วนเกินราคา $1.10-1.80 ต่อ 1,000 (ตามแผน) |

รายละเอียดเฉพาะ Amazon SES (https://docs.aws.amazon.com/ses/latest/dg/request-production-access.html):
- บัญชีใหม่อยู่ใน **sandbox** (โหมดจำกัด ต่อ Region): ส่งถึงได้เฉพาะอีเมล/โดเมนที่ verify แล้ว หรือ mailbox simulator, สูงสุด 200 ข้อความ/24 ชม., 1 ข้อความ/วินาที
- พ้น sandbox ต้องยื่นคำขอ production access (ระบุประเภท transactional/marketing, URL เว็บไซต์, อีเมลติดต่อ, ยืนยันว่ามีกระบวนการจัดการ bounce และ complaint) AWS ระบุว่าตอบเบื้องต้นภายใน 24 ชม. และอาจนานกว่านี้ถ้าต้องขอข้อมูลเพิ่ม การ verify โดเมนก่อนยื่นช่วยให้อนุมัติเร็วขึ้น
- ยังต้อง verify identity ที่ใช้เป็น From/Source/Sender/Return-Path แม้พ้น sandbox
- มีทั้ง SMTP interface และ API ข้อมูล SMTP credentials แยกจาก AWS access key และต่อ Region (https://docs.aws.amazon.com/ses/latest/dg/send-email-smtp.html)
- ราคาหน้า pricing หน้าเดียวกันยังมีแผน Essentials/Pro/Enterprise ($0.16-0.23 ต่อ 1,000 ที่ 0-10M) และ Dedicated IP ($15/เดือน แบบ managed หรือ $24.95/IP/เดือน ขั้นต่ำ 256 IP แบบ standard/BYOIP) ซึ่งไม่น่าเกี่ยวกับขนาดงานตอนนี้ แต่เป็นข้อเท็จจริงที่ปรากฏ
- หมายเหตุ: ถ้าส่งจาก EC2 อาจต้องขอปลด throttle พอร์ต 25 (ตามหมายเหตุในหน้าเดียวกัน)

ข้อสังเกตทั่วไป: ทุกเจ้าข้างต้นเรียกผ่าน HTTP API ได้ และหลายเจ้ามี SMTP relay ด้วย (ยืนยันจากเอกสารในรอบนี้เฉพาะ SES ส่วน Mailgun หน้า pricing ระบุ "SMTP/API access" ที่แผน Basic) การเลือกใช้ SMTP relay จะใช้ MailKit ตัวเดียวกับหัวข้อ 3 ได้ ส่วน API ใช้ SDK ของเจ้านั้นหรือ `HttpClient`

## 5. เครื่องมือจำลองสำหรับ dev (รับเมลไว้ดูในเว็บ ไม่ส่งออกจริง)

- **Mailpit** (https://github.com/axllent/mailpit): MIT, image `axllent/mailpit`, SMTP พอร์ต 1025 เว็บ UI พอร์ต 8025, มี REST API, รองรับ HTML compatibility check และ spam assessment README ระบุว่าสร้างขึ้นเพราะ MailHog ไม่มี active development/security updates แล้ว
- **smtp4dev** (https://github.com/rnwood/smtp4dev): BSD-3-Clause สร้างบน .NET มี Web UI และ TUI, IMAP/POP3, MIME inspector, หลาย mailbox, REST API มี Dockerfile/docker-compose ใน repo (README ที่ดึงมาไม่ระบุคำสั่ง docker run และพอร์ต ไม่พบข้อมูลนี้ในรอบนี้)
- **MailHog** (https://github.com/mailhog/MailHog): image `mailhog/mailhog` พอร์ต 1025/8025 repo แสดง release ล่าสุด v1.0.0 และ issue ค้าง 220 รายการ ดูเหมือนไม่ได้พัฒนาต่อ (ข้อมูลวันที่ไม่ปรากฏในหน้าที่ดึงได้ ประกอบกับข้อความจาก README ของ Mailpit ข้างบน)
- ทั้งหมดรับ SMTP แบบไม่ต้อง auth/ไม่ต้องมีโดเมน จึงใช้ MailKit ต่อได้ตรงๆ แค่ตั้ง host/port

## 6. SPF / DKIM / DMARC และการไม่ตกสแปม

(คำอธิบายง่ายๆ: SPF = ประกาศใน DNS ว่า server ไหนส่งเมลแทนโดเมนเราได้, DKIM = ลายเซ็นดิจิทัลแนบในเมลให้ผู้รับตรวจว่าไม่ถูกแก้, DMARC = นโยบายใน DNS บอกผู้รับว่าถ้า SPF/DKIM ไม่ผ่านให้ทำอย่างไร)

ที่มา: Gmail sender guidelines https://support.google.com/a/answer/81126 (สรุปจากเครื่องมือดึงหน้า ไม่ใช่อ่านดิบ)
- ผู้ส่งทุกราย (ใช้บังคับตั้งแต่ 1 ก.พ. 2024): ต้องตั้ง SPF **หรือ** DKIM สำหรับโดเมนที่ส่ง, มี DNS forward และ reverse (PTR) ที่ถูกต้อง, ใช้ TLS, รักษา spam rate ต่ำกว่า 0.3% (ดูใน Postmaster Tools), เป็นไปตาม RFC 5322
- ผู้ส่งจำนวนมาก (5,000+ ข้อความ/วัน ไปยัง Gmail): ต้องมี DMARC (policy เป็น none ได้), From domain ต้อง align กับโดเมน SPF หรือ DKIM, และอีเมลการตลาด/subscription ต้องมี one-click unsubscribe (ตามหน้าที่ดึงมา) ไม่พบข้อความในหน้าที่ดึงว่าข้อกำหนด unsubscribe ครอบคลุมอีเมล transactional แบบยืนยันตัวตน
- ข้อควรสังเกตของ provider:
  - Amazon SES: Easy DKIM จะใส่ DKIM key 2048-bit ให้ทุกอีเมลจาก domain identity ต้องแก้ DNS ของโดเมน (ถ้าใช้ Route 53 SES สร้าง record ให้อัตโนมัติ) รองรับ custom MAIL FROM domain (https://docs.aws.amazon.com/ses/latest/dg/send-email-authentication-dkim-easy.html)
  - Resend: ต้อง verify โดเมนก่อนส่ง และแนะนำให้ตั้ง DMARC หลัง verify (รายละเอียด record SPF/DKIM อยู่ในหน้า add-a-domain ซึ่งไม่ได้ดึงในรอบนี้)
  - SendGrid: เอกสาร ASP.NET Core ระบุว่าบัญชีอาจต้อง "add a Sender" (ไม่พบรายละเอียดเรื่องโดเมนในหน้าที่ดึง)
  - ไม่ได้ดึงเอกสารการตั้งโดเมนของ Mailgun และ SendGrid ในรอบนี้
- เอกสารไม่ได้ให้ตัวเลขว่า SMTP ที่โฮสต์เอง "ผ่านกี่เปอร์เซ็นต์" แต่ข้อกำหนดข้างต้นใช้กับทุกวิธีส่ง รวมถึงการส่งตรงเอง (ต้องตั้ง SPF/DKIM, PTR, TLS เอง)
- ทั้งหมดนี้ต้องมีโดเมนที่แก้ DNS ได้ (โดเมนของ Jotwai ปัจจุบันไม่พบใน repo ว่าตัดสินใจไว้ ตรวจแล้ว docker-compose ใช้ `admin@jotwai.dev` แค่เป็นค่า pgAdmin ไม่ได้บอกว่าเป็นโดเมนจริง)

## 7. ข้อจำกัดอื่นที่กระทบการตัดสินใจ (ข้อเท็จจริงที่พบ)

- ทุกตัวเลือกต้องเก็บ secret (API key / SMTP password) ผ่าน config ตามกฎ Security ของโปรเจกต์ เอกสาร ASP.NET Core ตัวอย่างใช้ `dotnet user-secrets` ใน dev และ environment variable/application settings ใน production
- แผนฟรีมีเพดานรายวัน/รายเดือนต่างกัน (Resend 100/วัน, Mailgun 100/วัน, SendGrid ทดลอง 100/วัน 60 วัน, SES sandbox 200/วัน เฉพาะผู้รับที่ verify แล้ว) ผู้ใช้สมัครเกินเพดานในวันนั้นจะส่งไม่ได้ตามแผนนั้น
- SES sandbox: ส่งได้เฉพาะผู้รับที่ verify แล้วจนกว่า AWS อนุมัติ production access
- ตัวอย่างทางการของ ASP.NET Core ใช้ interface `IEmailSender` (namespace `Microsoft.AspNetCore.Identity.UI.Services`) ผูกกับ Identity UI ไม่พบในเอกสารว่าแพ็กเกจนี้จำเป็นต้องใช้ ถ้าไม่ใช้ Identity UI (โปรเจกต์นี้ไม่ใช้)
- ผู้ใช้เดิมใน DB (ถ้ามีเมื่อเปิดบังคับ confirm) จะถูกล็อกเอาท์ตามหัวข้อ 2
- หน้าเว็บที่ดึงไม่สำเร็จ: https://www.twilio.com/en-us/sendgrid/email-api/pricing (404); หน้า pricing ของ SendGrid หน้าอื่นเปลี่ยน URL ผู้เขียนใช้หน้า https://www.twilio.com/en-us/products/email-api/pricing แทน

## 8. สิ่งที่ยังไม่ได้ตรวจในรอบนี้ (ช่องว่างข้อมูล)

- เอกสารตั้งโดเมน/DNS ของ Resend, Mailgun, SendGrid แบบละเอียด
- พฤติกรรม Data Protection key เมื่อ container restart โดยไม่มี volume (ข้อความตรงๆ จากเอกสาร)
- คำสั่ง docker run ของ smtp4dev และพอร์ตที่แน่นอน
- ผู้ให้บริการอื่น (Brevo, Postmark, Azure Communication Services, Google Workspace SMTP relay ฯลฯ) ไม่ได้ครอบคลุม
- ข้อกำหนดด้านกฎหมาย/ความเป็นส่วนตัวของผู้ให้บริการแต่ละเจ้า (ที่ตั้งข้อมูล, PDPA)
