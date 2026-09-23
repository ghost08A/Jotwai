---
id: 19
title: "ตรวจสอบการนำไปใช้จริง: ADR-0011 เอนทิตี Identity แบบกำหนดเองพร้อมการแฮชรหัสผ่านของ ASP.NET Core Identity"
type: wayfinder:research
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
archived_from: https://github.com/ghost08A/Jotwai/issues/19
---

## Question

โค้ดเบสปัจจุบันนำการตัดสินใจที่บันทึกไว้ใน `docs/adr/0011-custom-identity-entities-with-password-hashing.md` ไปใช้จริงหรือไม่? ให้ค้นหาเอนทิตีที่เกี่ยวข้องกับ Identity, การใช้งานการแฮชรหัสผ่าน, โค้ดออก JWT, ตรรกะการหมุนเวียน refresh token และ Controller ของ Auth/Identity

## Finding (ทำตามแล้ว)

เอนทิตีเป็น POCO ธรรมดา (`User`, `RefreshToken` — ไม่ใช่ `IdentityUser`) ซึ่งตรงกับการตัดสินใจของ ADR ที่ไม่ใช้เอนทิตี Identity ของ framework โดยตรง การแฮชรหัสผ่านใช้ `IPasswordHasher<User>` เท่านั้น (ไม่ใช่ `UserManager`/`SignInManager`) พร้อมการตรวจสอบ dummy-hash แบบ constant-time เมื่ออีเมลไม่รู้จัก เพื่อป้องกันการรั่วไหลของข้อมูลผ่าน timing รวมถึง `PasswordPolicy.Validate` JWT access token ถูกออกเองด้วยมือ (`TokenService.GenerateAccessToken`, HMAC-SHA256) และการหมุนเวียน refresh token (`AuthService.RefreshAsync`) ใช้ `ExecuteUpdateAsync` แบบ atomic compare-and-swap เพื่อเพิกถอนทันทีที่ถูกใช้งาน ป้องกันการเล่นซ้ำของ token ที่ถูกหมุนเวียนไปแล้ว `AuthController` เปิดใช้งาน register/login/refresh/logout/me ครอบคลุมโดย `Jotwai.IntegrationTests/TokenRefreshTests.cs`

ไม่พบช่องว่างใด ๆ เมื่อเทียบกับขอบเขตจริงของ ADR นี้
