# ADR 0011: Entity ของ Identity แบบกำหนดเองพร้อมการแฮชรหัสผ่านของ ASP.NET Core Identity (แทนที่ ADR-0005)

## สถานะ
ยอมรับแล้ว แทนที่ ADR-0005

## บริบท
ADR-0005 ระบุ "ASP.NET Core Identity" เป็นกลไกการยืนยันตัวตน ซึ่งอ่านแล้วเหมือนกับการนำ framework ทั้งหมดมาใช้ — `IdentityUser`, `IdentityDbContext<TUser>`, `UserManager`, `SignInManager` สิ่งที่ถูกสร้างขึ้นจริงสำหรับ Identity Module (Issue #2, #3) นั้นแคบกว่านั้น: entity `User`/`RefreshToken` ธรรมดาใน `Jotwai.Domain`, `IdentityDbContext : DbContext` ที่เขียนขึ้นเอง (ไม่ใช่ `IdentityDbContext<TUser>`) และการออก/ตรวจสอบ JWT ที่เขียนโดยตรงใน `TokenService`/`AuthService` การแฮชรหัสผ่านของ framework (`IPasswordHasher<User>`) และกฎ default password-policy ของมันถูกนำมาใช้ซ้ำ แต่ไม่มีสิ่งอื่นใดจาก ASP.NET Core Identity ที่ถูกเชื่อมต่อเข้ามา ADR-0005 ควรอธิบายสิ่งที่มีอยู่จริงในโค้ดแทนที่จะเป็นชื่อ framework ที่กว้างกว่านั้น

## การตัดสินใจ
คงไว้ซึ่ง entity `User`/`RefreshToken` แบบกำหนดเองและ `IdentityDbContext` ที่เขียนขึ้นเอง ไม่ใช้ `IdentityUser`/`IdentityDbContext<TUser>`/`UserManager`/`SignInManager` ของ ASP.NET Core Identity นำมาใช้ซ้ำเพียงสองส่วนจาก ASP.NET Core Identity เท่านั้น: `IPasswordHasher<User>` สำหรับการแฮช/ตรวจสอบรหัสผ่าน และการตรวจสอบ default password-policy ของมัน (ผ่าน `PasswordPolicy`) สถานะบัญชี/session (อีเมล, password hash, refresh token) ถูกจำลองและจัดเก็บทั้งหมดโดย entity และ `DbContext` ของ Identity Module เอง จำกัดขอบเขตอยู่ที่ schema `identity` (ADR-0003)

ทุกอย่างอื่นที่ ADR-0005 ตัดสินใจไว้ยังคงไม่เปลี่ยนแปลง: JWT access token พร้อม refresh token, Identity Module เป็นเจ้าของ credential ทั้งหมด, การหมุนเวียน/วันหมดอายุของ refresh token เป็นรายละเอียดการ implement และ OAuth ยังคงเป็นงานที่ตามมาทีหลัง

## ผลที่ตามมา
- แนวคิด "ASP.NET Core Identity" ของ ADR-0005 ถูกแก้ไขเป็น "ตัวแฮชรหัสผ่าน + password policy ของ ASP.NET Core Identity" ซึ่งตรงกับโค้ดที่ส่งมอบจริง (`Jotwai.Domain/Models/User.cs`, `Jotwai.Domain/Models/RefreshToken.cs`, `Jotwai.Service/IdentityDbContext.cs`)
- ไม่มีระบบ `UserManager`/`SignInManager`/claims-and-roles ของ ASP.NET Core Identity อยู่หรือมีแผนจะมี หากในอนาคตต้องการ roles/claims นั่นคือการตัดสินใจใหม่ ไม่ใช่สิ่งที่ครอบคลุมอยู่แล้วโดย "ASP.NET Core Identity" ตามที่ระบุใน ADR-0005
- การคงไว้ซึ่ง entity แบบกำหนดเอง (แทนที่จะเป็น `IdentityUser`) ทำให้ schema `identity` ยังคงเป็นเจ้าของและถูกกำหนดรูปร่างโดย Module นี้อย่างสมบูรณ์ สอดคล้องกับการแยก schema-per-module ของ ADR-0003 — ไม่มีตารางที่ framework เป็นเจ้าของอยู่นอกรูปร่างนี้
