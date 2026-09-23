---
id: 27
title: "Build: Frontend login flow (Identity/Auth)"
type: wayfinder:task
status: open
assignee: null
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

## Finding (implement แล้ว ยังไม่ได้ verify กับ backend ที่รันจริง)

สิ่งที่ build แล้ว:

- `modules/identity/` — `types/`, `services/authService.ts` (login/register/logout/getCurrentUser), `hooks/` (`useLogin`, `useRegister`, `useLogout`, `useCurrentUser`), `components/` (`LoginPage`, `RegisterPage`)
- `core/http/session.ts` — token store แบบธรรมดา (ไม่ใช่ React) เก็บใน localStorage พร้อม subscribe/notify hook เพื่อให้ React state และ axios interceptor sync กันโดยไม่ต้องใช้ state-management library
- `core/http/httpClient.ts` — request interceptor แนบ `Authorization: Bearer`; response interceptor refresh access token ที่หมดอายุอย่างโปร่งใสเมื่อเจอ 401 แล้วลอง request เดิมใหม่หนึ่งครั้ง เคลียร์ session ถ้าการ refresh เองล้มเหลว
- `shared/context/AuthContext.tsx` — React wrapper บางๆ ครอบ `core/http/session.ts` ผ่าน `useSyncExternalStore` เปิดเผย `isAuthenticated`
- `core/router/ProtectedRoute.tsx` / `PublicOnlyRoute.tsx` — การเข้าถึง `/` โดยไม่ login จะ render `core/pages/Unauthorized.tsx`; ผู้ใช้ที่ login อยู่แล้วเข้า `/login` หรือ `/register` จะถูก redirect ไปที่ `/`
- Backend: เพิ่ม `Cors` policy (`Program.cs` + `Cors:AllowedOrigins` ใน `appsettings.Development.json`, ตอนนี้คือ `http://localhost:5173`) — เดิมไม่มีเลย (gap ที่ ticket 016 เคยแจ้งไว้) ทำให้ SPA เรียก API แบบ cross-origin ได้

ยืนยันแล้วในเบราว์เซอร์ (Vite dev server, click-through จริง): เข้า `/` โดยไม่ login → เจอหน้า Unauthorized → link ไป `/login`; `/login` และ `/register` render และ validate ได้ถูกต้อง; ตอน submit จะ degrade ผ่านลำดับความสำคัญของ error message ได้ถูกต้อง (เจอ body 404 จาก Vite ก่อน แล้วค่อยเจอ `Network Error` จริงหลังแก้ `VITE_API_BASE_URL` ใน `client/.env` ให้ชี้ไปที่ API) — ยืนยันว่า request path และ error handling ถูก wire ไว้ถูกต้องครบวงจร ยกเว้นแค่ response จริงจาก API ที่รันอยู่

**ยังไม่ได้ verify**: การ register/login/refresh/logout จริงกับ API + Postgres ที่รันอยู่จริง — Docker Desktop ใช้ไม่ได้ใน environment นี้ จึงเปิด dependency DB `identity` ของ API ไม่ได้ `dotnet build` ผ่าน และ CORS/route wiring ถูก review แล้ว แต่ยังไม่มีใครลอง response จริงแบบ 201/200 ผ่านโค้ด client ใหม่นี้เลย คนที่หยิบ ticket นี้ต่อควรรัน `docker-compose up` + API + `npm run dev` แล้ว click-through register → logout → login → refresh (รอให้ `AccessTokenMinutes` หมดอายุ หรือลดค่าลงชั่วคราวในเครื่อง) ก่อนปิด ticket นี้
