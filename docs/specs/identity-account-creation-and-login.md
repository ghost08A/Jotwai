# Identity: Account Creation & Login

> เผยแพร่เป็น [ghost08A/Jotwai#1](https://github.com/ghost08A/Jotwai/issues/1) (label `ready-for-agent`) ไฟล์นี้ถูกเก็บไว้เป็น source of truth ในเครื่อง คู่ขนานไปกับ issue นั้น

## Problem Statement

Note ของ Jotwai ต้อง sync ข้ามทุกอุปกรณ์ที่ผู้ใช้ login (ตาม product requirements: จดจากที่ไหนก็ได้ จัดที่ไหนก็ได้) ตอนนี้ยังไม่มีวิธีให้คนยืนยันตัวตนกับระบบ จึงไม่มีอะไรให้ผูก Note, Topic หรือ Category เข้าไว้ด้วย และไม่มีทางที่ข้อมูลของคนคนเดียวกันจะติดตามเขาจากอุปกรณ์หนึ่งไปอีกอุปกรณ์หนึ่งได้

## Solution

ผู้ใช้สามารถสร้าง account ด้วย email และ password และ login จากอุปกรณ์ไหนก็ได้เพื่อเข้าถึงข้อมูลของตัวเอง เมื่อ login แล้วจะยังคง login อยู่ตลอด session โดยไม่ต้องใส่ credential ซ้ำในทุก request และสามารถ logout อย่างชัดเจนเพื่อจบ session บนอุปกรณ์นั้นได้

## User Stories

1. ในฐานะผู้ใช้ใหม่ ฉันต้องการสร้าง account ด้วย email และ password ของฉัน เพื่อที่จะมีที่ให้ Note ของฉันสังกัดอยู่
2. ในฐานะผู้ใช้ใหม่ ฉันต้องการ error ที่ชัดเจนถ้าฉันพยายาม register ด้วย email ที่ถูกใช้แล้ว เพื่อที่จะเข้าใจว่าทำไม registration ถึงล้มเหลว
3. ในฐานะผู้ใช้ใหม่ ฉันต้องการ error ที่ชัดเจนถ้า password ที่ฉันเลือกไม่ตรงตามข้อกำหนดขั้นต่ำ เพื่อที่จะรู้ว่าต้องแก้อะไร
4. ในฐานะผู้ใช้ใหม่ ฉันต้องการถูก login อัตโนมัติทันทีหลัง register เพื่อที่จะเริ่มจดโน้ตได้ทันทีโดยไม่ต้องผ่านขั้นตอน login แยก
5. ในฐานะผู้ใช้ที่กลับมาใช้งานอีกครั้ง ฉันต้องการ login ด้วย email และ password จากอุปกรณ์ไหนก็ได้ เพื่อที่จะเข้าถึง note ของฉันได้ไม่ว่าจะอยู่ที่ไหน
6. ในฐานะผู้ใช้ที่กลับมาใช้งานอีกครั้ง ฉันต้องการ error ที่ชัดเจนและเป็นกลางเมื่อ email หรือ password ผิด เพื่อที่จะรู้ว่าต้องลองใหม่โดยไม่ถูกบอกว่าอันไหนใน 2 อย่างนั้นผิด (ป้องกันการรั่วไหลว่า email ไหนถูก register ไว้แล้ว)
7. ในฐานะผู้ใช้ที่ login อยู่ ฉันต้องการให้ session ของฉันยังทำงานอยู่โดยไม่ต้องใส่ password ซ้ำทุก action เพื่อที่การใช้แอปจะไม่ถูกขัดจังหวะตลอดเวลา
8. ในฐานะผู้ใช้ที่ login อยู่ ฉันต้องการให้ session ของฉัน refresh ตัวเองอย่างโปร่งใสใน background ในที่สุด เพื่อที่จะไม่ถูก log out เพียงเพราะเปิดแอปทิ้งไว้นาน
9. ในฐานะผู้ใช้ที่ login อยู่ ฉันต้องการ logout ได้อย่างชัดเจนบนอุปกรณ์หนึ่งๆ เพื่อที่จะจบ session บนอุปกรณ์นั้นได้ (เช่น บนคอมพิวเตอร์ที่ใช้ร่วมกันหรือยืมมา)
10. ในฐานะผู้ใช้ที่ login หลายอุปกรณ์ ฉันต้องการให้การ logout บนอุปกรณ์หนึ่งไม่ log out ฉันออกจากที่อื่นทั้งหมด เพื่อที่ session ที่ active อยู่บนอุปกรณ์อื่นจะไม่ถูกรบกวน
11. ในฐานะ client application ใดๆ ก็ตาม (ตอนนี้คือ web ต่อไปคือ mobile) ฉันต้องการวิธีมาตรฐานในการพิสูจน์ว่า request มาจากผู้ใช้ที่ login แล้ว เพื่อที่ทุก module อื่น (Notes, Organization, AI Categorization, Search) จะเชื่อถือตัวตนที่แนบมากับ request ได้
12. ในฐานะ developer ของ module อื่น ฉันต้องการวิธีที่มั่นคงในการดึง "user ID ของผู้ใช้ปัจจุบัน" จาก request ที่ authenticated แล้ว เพื่อที่ Notes/Topics/Categories จะถูกจำกัดขอบเขตให้เป็นของเจ้าของได้โดยไม่ต้องให้แต่ละ module implement auth ซ้ำเอง
13. ในฐานะผู้ใช้ ฉันต้องการให้ password ของฉันไม่ถูกเก็บหรือ log เป็น plain text ที่ไหนเลย เพื่อที่ data breach จะไม่เปิดเผย password จริงของฉัน
14. ในฐานะผู้ใช้ ฉันต้องการให้ session ที่หมดอายุหรือไม่ถูกต้องถูกปฏิเสธด้วย response แบบ "กรุณา login ใหม่" ที่ชัดเจน เพื่อที่แอปจะได้ prompt ฉันอย่างเหมาะสมแทนที่จะ fail แบบเงียบๆ หรือด้วย error ที่สับสน
15. ในฐานะระบบที่คำนึงถึงความปลอดภัย ฉันต้องการให้ refresh token ที่หลุดหรือถูกขโมยใช้การไม่ได้อีกหลังใช้งาน (rotation) หรือหลัง logout เพื่อที่การขโมย token จะมีผลกระทบจำกัด

## Implementation Decisions

- **Module**: spec นี้อยู่ภายใน module **Identity** ทั้งหมด (ADR-0001, ADR-0005) ไม่มี module อื่นถูกแก้ไข — Notes/Organization/AI Categorization/Search จะ consume "current user ID" ผ่าน standard ASP.NET Core auth pipeline ใน spec ในอนาคตของตัวเอง ไม่ใช่ผ่านการ dependency ตรงกับภายในของ Identity
- **Identity provider**: ASP.NET Core Identity จัดการ user record (email เป็น username, password hashing, ความ unique) Email คือตัวระบุ account ไม่มี field username แยกต่างหาก
- **Auth mechanism**: JWT access token + refresh token (ADR-0005)
  - **Access token**: อายุสั้น (เป้าหมาย: 15 นาที) พก user ID เป็น claim ใช้เป็น `Bearer` token ในทุก API request ไปยังทุก module
  - **Refresh token**: อายุยาวกว่า (เป้าหมาย: 30 วัน) เป็น opaque เก็บฝั่ง server (แบบ hash) ใน schema ของ Identity module พร้อม expiry และ flag revoked ใช้เพื่อขอ access token ใหม่เท่านั้น
  - **Refresh token rotation**: ทุกครั้งที่ refresh จะออก refresh token ใหม่และ revoke อันเก่า refresh token ที่ถูก revoke หรือถูกใช้ไปแล้วจะถูกปฏิเสธทันที และถือเป็นสัญญาณว่า token อาจถูกขโมย
- **Endpoints** (public HTTP API ของ Identity module — จุดเชื่อมที่ module อื่นและ client พึ่งพา):
  - `POST /api/auth/register` — `{ email, password }` → สร้าง account แล้วทำงานเหมือน login สำเร็จ (คืน access + refresh token ทันที; ดู User Story 4)
  - `POST /api/auth/login` — `{ email, password }` → `{ accessToken, refreshToken, expiresIn }` เมื่อสำเร็จ; error แบบ "invalid credentials" ทั่วไปเมื่อล้มเหลว (ไม่เปิดเผยว่า email มีอยู่หรือไม่)
  - `POST /api/auth/refresh` — `{ refreshToken }` → `{ accessToken, refreshToken, expiresIn }` ชุดใหม่ พร้อม rotate refresh token
  - `POST /api/auth/logout` — `{ refreshToken }` → revoke เฉพาะ refresh token นั้นตัวเดียว (อุปกรณ์/session อื่นไม่ได้รับผลกระทบ ตาม User Story 10) Idempotent: logout ด้วย token ที่ถูก revoke ไปแล้วไม่ถือเป็น error
- **Schema** (เป็นเจ้าของโดย Identity module เท่านั้น, schema `identity` — ADR-0003): ตาราง user มาตรฐานของ ASP.NET Core Identity บวกตาราง `RefreshTokens` (token hash, user ID, expiry, flag revoked, timestamp created/rotated)
- **Password policy**: กฎความซับซ้อน default ของ ASP.NET Core Identity เป็นจุดเริ่มต้น (ปรับได้ทีหลังโดยไม่ต้องมี spec ใหม่ ถ้า requirement เปลี่ยนไปอย่างมีนัยสำคัญ)
- **OAuth (Google/GitHub ฯลฯ)**: อยู่นอก scope ของ spec นี้อย่างชัดเจน — ระบุไว้เป็น fast-follow ใน ADR-0005, track เป็น spec แยกในอนาคต

## Testing Decisions

- **Seam**: integration test ต่อ public HTTP API ของ Identity module (4 endpoint ข้างบน) รันผ่าน ASP.NET Core test host (`WebApplicationFactory`) กับ PostgreSQL instance จริง (Testcontainers) — ไม่ใช่กับ mocked repository หรือ internal class ที่ unit test แยก นี่คือ test seam **แรก** ในโค้ดเบส มันวาง pattern ที่ spec ของทุก module อื่นควรทำตาม: test module ผ่าน public API ของมัน ไม่ใช่ internal ของมัน
- **ความหมายของ "external behavior" ที่นี่**: เมื่อมี request ไปยังหนึ่งใน 4 endpoint ให้ assert ที่ HTTP status code และรูปร่างของ response (มี/ไม่มี token, รูปร่างของ error) และ side effect ที่สังเกตได้จากภายนอก (การพยายาม login ครั้งที่สองด้วย email เดิมล้มเหลว; refresh token ที่ถูก rotate แล้วใช้ไม่ได้อีก; refresh token ที่ logout ไปแล้วใช้ไม่ได้อีก) — ไม่ใช่ assert ที่ internal method call หรือวิธีที่ ASP.NET Core Identity เก็บ password hash ภายใน
- **Coverage ของ spec นี้** (ผ่าน seam เดียวนั้น):
  - Register: success path, email ซ้ำ, password อ่อนเกินไป
  - Login: success path, password ผิด, email ที่ไม่มีในระบบ (รูปร่าง error เหมือนกับ password ผิดทุกประการ)
  - Refresh: success path (refresh token เก่าใช้ไม่ได้หลัง rotate), refresh token ที่หมดอายุถูกปฏิเสธ, refresh token ที่ถูกใช้/revoke ไปแล้วถูกปฏิเสธ
  - Logout: success path, logout อุปกรณ์หนึ่งไม่กระทบ refresh token ที่ active ของอุปกรณ์อื่น, logout ซ้ำสองครั้งไม่ถือเป็น error
  - Endpoint ทดสอบที่ protected (หรือ auth requirement ของ endpoint refresh เอง ถ้าเข้าเงื่อนไข) ปฏิเสธ request ที่ไม่มี token, token หมดอายุ และ token ที่ผิดรูปแบบ
- **Prior art**: ยังไม่มีในโค้ดเบสนี้ — test ของ spec นี้เป็นชุดแรก และควรถือเป็น reference pattern (Testcontainers Postgres + `WebApplicationFactory`) สำหรับ spec ของ module Notes, Organization, AI Categorization และ Search เอง

## Out of Scope

- OAuth / social login (Google, GitHub ฯลฯ) — fast-follow ในอนาคตตาม ADR-0005
- Flow reset password / "ลืมรหัสผ่าน" — ไม่ครอบคลุมใน spec นี้; จำเป็นก่อนเปิดใช้งานทั่วไปแต่ไม่ block core loop ของการจดและ sync
- การยืนยัน email ตอน register
- การลบ account / export ข้อมูล
- Multi-factor authentication
- Rate limiting / ป้องกัน brute-force ตอน login (คุ้มค่าที่จะทำเป็น security follow-up ก่อนขึ้น production แต่ไม่ใช่ spec นี้)
- การเปลี่ยนแปลงใดๆ ต่อ Notes, Organization, AI Categorization หรือ Search — พวกมันจะ consume "current user ID" จาก token ของ module นี้ใน spec ในอนาคตของตัวเอง ผ่าน standard ASP.NET Core auth pipeline ไม่ใช่ผ่านการเปลี่ยนแปลงที่ทำใน spec นี้

## Further Notes

- นี่คือ spec แรกของ project และวางรากฐาน 2 อย่างที่ spec ของ module ในอนาคตควรนำไปใช้ซ้ำ: **testing seam pattern** (public API + Testcontainers ตามที่อธิบายไว้ข้างบน) และ **auth contract** (`Bearer` JWT พร้อม claim user-ID) ที่ Notes/Organization/AI Categorization/Search จะพึ่งพาทั้งหมดเพื่อจำกัดขอบเขตข้อมูลต่อผู้ใช้
- ตาม `docs/agents/issue-tracker.md` สิ่งนี้ควรถูก publish เป็น GitHub issue บน `ghost08A/Jotwai` พร้อม label `ready-for-agent` เมื่อ `gh` CLI พร้อมใช้งานใน environment นี้ (ตอนนี้ยังไม่ได้ติดตั้งที่นี่)
