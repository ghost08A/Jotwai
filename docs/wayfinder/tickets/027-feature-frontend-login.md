---
id: 27
title: "Build: Frontend login flow (Identity/Auth)"
type: wayfinder:task
status: closed
assignee: voramatep
parent: ../map.md
blocked_by: []
---

## Question

Backend Auth API build และ test เสร็จสมบูรณ์แล้ว (`AuthController`: register/login/refresh/logout/me, JWT access + refresh token พร้อม rotation — ยืนยันแล้วใน ticket 019) แต่ฝั่ง client ยังไม่มีอะไร consume มันเลย (ยืนยันแล้วใน ticket 017: `axios` ติดตั้งแล้วแต่ไม่ได้ใช้, ไม่มีโค้ด Identity module, ไม่มี login UI ที่ไหนใน `client/src` เลย) สิ่งที่ต้อง build ตามโครงสร้าง module ของ `client/CLAUDE.md`:

- `modules/identity/` — หน้า/form login, หน้า/form register, `services/` (เรียก axios ดิบๆ ไปยัง `/auth/*` ผ่าน `core/http/httpClient`), `hooks/` (`useLogin`, `useRegister`, `useCurrentUser` ที่ห่อ service เหล่านั้นด้วย TanStack Query)
- การเก็บ access token และแนบไปกับ outgoing request (axios interceptor ใน `core/http`)
- Refresh-token flow: refresh access token ที่หมดอายุอย่างโปร่งใสแล้วลองใหม่ ตามพฤติกรรม rotation ที่ `AuthService.RefreshAsync` implement ไว้ฝั่ง server แล้ว
- Logout: เรียก `/auth/logout`, เคลียร์ local session state
- Wiring Protected Route ใน `core/router` (ตาม Project Structure ของ `client/CLAUDE.md`) — redirect ผู้ใช้ที่ยังไม่ login ไปที่ `/login`; redirect ผู้ใช้ที่ login อยู่แล้วออกจาก `/login`
- `core/pages/unauthorized.tsx` สำหรับ session ที่ล้มเหลว/หมดอายุ ตาม system page ที่ระบุชื่อไว้แล้วใน `client/CLAUDE.md`

นี่คือ build task ไม่ใช่ open decision — track ไว้ที่นี่เป็น backlog ตาม convention เดียวกับ ticket 020–026 ไม่ถูก block ด้วยอะไร: backend API ที่ต้องพึ่งพามีอยู่แล้วและถูก cover ด้วย `Jotwai.IntegrationTests/TokenRefreshTests.cs` และ test อื่นๆ ที่เกี่ยวข้อง

## Finding (implement และ verify กับ backend ที่รันจริงแล้ว)

สิ่งที่ build แล้ว:

- `modules/identity/` — `types/`, `services/authService.ts` (login/register/logout/getCurrentUser), `hooks/` (`useLogin`, `useRegister`, `useLogout`, `useCurrentUser`), `components/` (`LoginPage`, `RegisterPage`)
- `core/http/session.ts` — token store แบบธรรมดา (ไม่ใช่ React) เก็บใน localStorage พร้อม subscribe/notify hook เพื่อให้ React state และ axios interceptor sync กันโดยไม่ต้องใช้ state-management library
- `core/http/httpClient.ts` — request interceptor แนบ `Authorization: Bearer`; response interceptor refresh access token ที่หมดอายุอย่างโปร่งใสเมื่อเจอ 401 แล้วลอง request เดิมใหม่หนึ่งครั้ง เคลียร์ session ถ้าการ refresh เองล้มเหลว
- `shared/context/AuthContext.tsx` — React wrapper บางๆ ครอบ `core/http/session.ts` ผ่าน `useSyncExternalStore` เปิดเผย `isAuthenticated`
- `core/router/ProtectedRoute.tsx` / `PublicOnlyRoute.tsx` — การเข้าถึง `/` โดยไม่ login จะ render `core/pages/Unauthorized.tsx`; ผู้ใช้ที่ login อยู่แล้วเข้า `/login` หรือ `/register` จะถูก redirect ไปที่ `/`
- Backend: เพิ่ม `Cors` policy (`Program.cs` + `Cors:AllowedOrigins` ใน `appsettings.Development.json`, ตอนนี้คือ `http://localhost:5173`) — เดิมไม่มีเลย (gap ที่ ticket 016 เคยแจ้งไว้) ทำให้ SPA เรียก API แบบ cross-origin ได้

**บั๊กที่เจอระหว่าง verify จริง และแก้แล้ว**: ตอนทดสอบจริงครั้งแรก register/login ขึ้น `Network Error` ทุกครั้ง สาเหตุคือ `app.UseHttpsRedirection()` ใน `Program.cs` ถูกวางไว้**ก่อน** `app.UseCors(...)` — ทุก request (รวม CORS preflight `OPTIONS`) โดน redirect ไป `https://localhost:7088` ก่อนที่ CORS middleware จะได้ตอบ และ browser ไม่ยอม follow redirect ระหว่าง preflight เลยบล็อก request ทั้งหมด แก้โดย:
- ลบ `UseHttpsRedirection()` ออก — dev setup นี้ backend รับ http ตรงๆ ไม่มี reverse proxy บังคับ https ระหว่าง dev (ตรงกับ convention ของบริษัทที่ TLS terminate ที่ hosting/proxy ไม่ใช่ที่ Kestrel)
- เพิ่ม `UseRouting()` และเรียงลำดับใหม่เป็น `UseRouting → UseCors → UseAuthentication → UseAuthorization`
- ย้าย CORS setup ออกมาเป็น extension method `AddCustomCors` ใน `Jotwai.Service/CorsServiceRegistration.cs` (รูปแบบเดียวกับ `AddIdentityFeature`)

ยืนยันครบวงจรจริงแล้ว (Docker Postgres + API รันจริง + Vite dev server + click-through ในเบราว์เซอร์): เข้า `/` โดยไม่ login → เจอหน้า Unauthorized → link ไป `/login`; register ผู้ใช้ใหม่สำเร็จ (`201 Created`, ได้ access/refresh token จริง) → redirect เข้า `/` → เห็น "Signed in as `<userId>`" (ยืนยันว่า `GET /api/auth/me` ทำงานถูกต้องพร้อม token) → กด Log out → กลับไปหน้า Unauthorized → login ด้วย credential เดิมสำเร็จ → เห็น user ID เดิม ครบทุกขั้นตอนตาม Question ด้านบน

Ticket นี้ทำสำเร็จครบตาม scope ที่วางไว้ พร้อมปิดได้
