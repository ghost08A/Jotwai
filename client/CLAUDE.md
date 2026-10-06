# CLAUDE.md

ไฟล์นี้ให้คำแนะนำแก่ AI coding agent (Claude Code, Codex, ฯลฯ) เมื่อทำงานกับโค้ดใน repository นี้

กฎที่ใช้ร่วมกันทั้ง repo (security, Definition of Done) อยู่ที่
[../CLAUDE.md](../CLAUDE.md) — ไฟล์นี้ครอบคลุมเฉพาะสิ่งที่เจาะจงกับฝั่ง client เท่านั้น

## Tech Stack

- React 19 + TypeScript, Vite
- Routing: react-router-dom
- Server state: TanStack Query (`@tanstack/react-query`)
- HTTP: axios
- Styling: Tailwind CSS v4
- Lint: oxlint (ไม่ใช่ ESLint)
- ยังไม่มี test runner ตั้งค่าไว้ (ไม่มี Vitest/Jest ใน `package.json`) — ดู Test Runner ด้านล่าง

## Architecture

- ดูส่วน "Frontend" ใน [../ARCHITECTURE.md](../ARCHITECTURE.md) สำหรับภาพรวมทั้งหมด
- Server state (ข้อมูลจาก API) อยู่ใน TanStack Query เสมอ ห้าม duplicate server data ไปไว้ใน client-side state
- ไม่มี dedicated client-state library — `useState`/React Context จัดการ UI-only state (filter ที่กำลังใช้งาน, draft ของ editor, สถานะ sidebar) อย่าเพิ่ม Zustand/Redux/ฯลฯ โดยไม่ปรึกษาผู้ใช้ก่อน
- แยก component ออกจาก business/data logic ให้ชัดเจน — ดู Project Structure rules ด้านล่าง
- เนื้อหา Note เป็น Markdown; Tiptap เป็นเพียง view ของเนื้อหานั้น ไม่ใช่ source of truth (ดู root [CLAUDE.md](../CLAUDE.md))

## Project Structure

`src/` ปัจจุบันแทบว่างเปล่า มีแค่ `assets/` — โครงสร้างด้านล่างคือโครงสร้างเป้าหมายที่จะสร้างขึ้น ไม่ใช่ผลสำรวจของสิ่งที่มีอยู่ Module จะสะท้อนตาม 5 Module ฝั่ง backend (ดู [../ARCHITECTURE.md](../ARCHITECTURE.md) "Backend modules") เพื่อให้คำศัพท์ฝั่ง frontend และ backend สอดคล้องกับ [../CONTEXT.md](../CONTEXT.md)

```
.editorconfig
src/
    core/
        layouts/              # app layout -> public, private
        pages/                 # system pages: authorized, not-found, unauthorized
        providers/             # incl. QueryClientProvider (TanStack Query)
        query/                 # TanStack Query: queryClient config, default options, shared query keys
        router/                # route setup, Protected Route
        http/                  # axios client
    modules/                   # หนึ่งโฟลเดอร์ต่อหนึ่ง backend Module
        notes/
        organization/          # Topics (hierarchy) + Categories (flat)
        ai-categorization/     # Suggestion UI/state
        search/
        identity/              # login, register, session
            components/        # component ของ module
            hooks/              # TanStack Query hooks (useQuery/useMutation) ที่ห่อ services
            services/           # เรียก API (axios) — ห้ามมี TanStack Query ในนี้
            utils/               # pure function สำหรับ test
            types/
    shared/                    # ใช้ร่วมกันข้าม module
        components/            # dropdown, text-box, number-box, check-box, ...
        constants/
        routes/                 # route path constants
        context/                # shared state ผ่าน React Context (เบา, ไม่ใช่ global store)
        hooks/
        utils/                  # pure function สำหรับ test
        types/                  # type, class, interface
    assets/
    .env.example               # แค่ template ของ environment เท่านั้น **ใช้ credential/secret ปลอมเท่านั้น
    App.tsx
    main.tsx
```

### Rules

- ห้ามใส่ side effect ไว้ใน presentational component
- Module ใหม่เพิ่มไว้ใต้ `modules/`; หนึ่ง module ต้องตรงกับ backend Module แบบ 1:1 — ห้ามสร้าง module ที่มีแค่ฝั่ง frontend โดยไม่มี Module ฝั่ง backend รองรับ
- ห้ามสร้าง abstraction ใหม่สำหรับการใช้งานครั้งเดียว
- ชื่อไฟล์ component ต้องตรงกับชื่อที่ export
- **TanStack Query**: `services/` = เรียก API ดิบๆ; `hooks/` = `useQuery`/`useMutation` ที่ห่อ services เหล่านั้น การตั้งค่า `QueryClient` อยู่ใน `core/query/`, provider อยู่ใน `core/providers/`
- UI state ที่อยู่ในระดับ module หรือข้าม module (ที่ไม่ใช่ server data) ใช้ `useState`/React Context ไม่ใช่ global store library

## Coding Convention

### Naming

| Pattern              | ใช้กับ                                                      |
| --------------------- | ----------------------------------------------------------- |
| `PascalCase`          | Component, type, interface — ชื่อไฟล์ต้องตรงกับ component |
| `camelCase`           | Function, variable                                          |
| `use` + `camelCase`   | Custom hook (เช่น `useAuth`) — ชื่อไฟล์ต้องตรงกัน              |
| `kebab-case`          | ชื่อไฟล์ที่ไม่ใช่ component (เช่น `date-utils.ts`)                |
| `UPPER_SNAKE_CASE`    | Constant ระดับ module                                          |

### Coding Rules

- Component ควรเรียบง่าย ไม่เกิน 200 บรรทัด **เว้นแต่ความซับซ้อนจำเป็นจริงๆ**
- ตั้งชื่อตัวแปรให้สื่อความหมาย ห้ามย่อแบบ `qty` แทน `quantity`
- ห้ามมี dead code โดยไม่มี comment อธิบาย
- Comment เฉพาะตอนที่ intent ไม่ชัดเจนเท่านั้น
- แยก logic ที่ซ้ำกันออกเป็น hook
- จัดการ error ที่จุดเรียกใช้งาน (call site)
- **แยก state**: server data → TanStack Query (ห้าม copy server data ไปไว้ใน local/context state); `useState`/Context ใช้เฉพาะ client/UI state (filter, modal, ขั้นตอนของ wizard, ค่า preference ของ session)

### For Typescript

- เปิด strict mode, หลีกเลี่ยง `any` — ใช้ `unknown` + narrowing แทนเมื่อยังไม่รู้ type จริง
- Export แบบ named เท่านั้น — เข้ากันได้ดีกับการ refactor และ auto-import มากกว่า default export
- ใช้ `undefined` สำหรับ "ยังไม่มีค่า" (optional prop, state ที่ยังไม่ initialize — เป็นค่า absence ตามธรรมชาติของ TS/JS); ใช้ `null` เฉพาะตอนที่ค่านั้นว่างเปล่าโดยตั้งใจ (เช่น สะท้อน field ที่เป็น nullable จาก API/DB) — ห้ามใช้สองอย่างนี้สลับกันไปมา
- ใช้ `async/await` แทน `.then` chain
- หลีกเลี่ยง non-null assertion (`!`) — narrow type แทน; `!` compile ผ่านแต่เป็นสาเหตุอันดับต้นๆ ของบั๊กแบบ "compile ผ่านแต่ crash ตอนรัน"

### Editor Config

Repo นี้บังคับ formatting ผ่าน `.editorconfig` ที่ root ของ repo AI agent ไม่มีกลไก auto-load editor config file แบบที่ IDE ทำได้ — กฎที่แท้จริงจึงถูกเขียนซ้ำไว้ที่นี่เพื่อให้อยู่ใน context จริงๆ ต้องทำตามในทุกไฟล์ที่คุณเขียนหรือแก้ไข อย่าเดา indentation จากการดูโค้ดรอบๆ

- Indent: 4 spaces (ค่า default ของทั้ง repo)
- Line ending: LF, UTF-8, ต้องมี final newline
- ตัด trailing whitespace (ยกเว้น Markdown ที่ trailing space มีความหมายสำหรับ line break)
- Line ending ถูกบังคับโดย `.gitattributes` ที่ root ไม่ใช่ Git setting ในเครื่องของคุณ — ห้าม "แก้" มันเองด้วยมือ และห้ามเปลี่ยน `core.autocrlf` เพื่อหนี complaint จาก formatter
- `.vscode/settings.json` ถูก commit ไว้และ pin กฎเดียวกันสำหรับ editor มันไม่ใช่ personal configuration — ห้ามเพิ่ม theme, font หรือ path เฉพาะเครื่องเข้าไป และห้ามผ่อนปรน formatting key ให้ตรงกับไฟล์ที่ผิดอยู่แล้ว

ถ้า formatter รายงาน error เรื่อง line-ending หรือ whitespace ในไฟล์ที่คุณไม่ได้แตะ นั่นคือปัญหา configuration ของ repo ไม่ใช่ปัญหาโค้ด รายงานแล้วปล่อยไว้ ห้ามแก้ด้วยการ reformat ทั้ง repo และห้ามให้ไฟล์ที่ถูก reformat โดยไม่เกี่ยวข้องติดไปกับการเปลี่ยนแปลงของคุณ

### Formatting Display

- Date: `YYYY-MM-DD`
- DateTime: `YYYY-MM-DD HH:mm:ss`
- Time only: `HH:mm` / `HH:mm:ss`

### UI & Design System

- ใช้ Tailwind utility class เท่านั้น — ห้ามเขียน CSS rule เอง ห้ามใช้ inline `style`
- Design token ถูกประกาศครั้งเดียว (Tailwind v4: `@theme {}`) เพื่อให้ Tailwind generate utility ให้ แล้วใช้งานผ่าน utility: `text-ink`, `hover:bg-ink`, `rounded-card` ห้ามอ้างถึง raw variable ตรงๆ — ห้ามใช้ `var(--color-ink)`, ห้ามใช้ `text-[var(--color-ink)]` Token ที่ไม่มี utility รองรับคือ token ที่อยู่ผิดที่
- ทุก interactive element ต้องมี state `hover`, `focus`, `active`, `disabled`
- Element ที่คลิกได้ใส่ class `cursor-pointer` และ `cursor-not-allowed` เมื่อ disabled
- Form ต้องอ่านง่ายและใช้งานบนมือถือได้ดี
- ต้องผ่าน WCAG 2.1 AA ด้าน contrast และ color blindness
- รองรับการใช้งานผ่านคีย์บอร์ด
- ปุ่ม CTA: ใช้ solid primary เท่านั้น — ห้ามใช้ ghost button สำหรับ action หลัก
- **Transition**: ใช้ transition class ที่เจาะจงที่สุด หลีกเลี่ยง `transition-all` ถ้ามีแค่สี animate ให้ตัด transition ออกไปเลย — ห้ามใช้ `transition-colors`

#### Rules

- **Import**: สำหรับ library ที่มี barrel export หนักๆ ให้ import จากไฟล์เฉพาะ ไม่ใช่จาก package root — เพื่อให้ bundle tree-shakeable ได้:
  ```ts
  // ✓
  import Button from "@mui/material/Button";
  // ✗
  import { Button } from "@mui/material";
  ```
- **Error handling**: ใน `catch` block ให้ดึงข้อความ error ตามลำดับความสำคัญ — API error body ก่อน แล้วค่อย `Error.message` แล้วค่อย fallback string คงที่ ใช้ `axios.isAxiosError()` เพื่อ narrow type ไม่ใช่ manual cast:

  ```ts
  function getErrorMessage(err: unknown, fallback = "An error occurred"): string {
    if (axios.isAxiosError(err)) {
      return err.response?.data?.message ?? err.message ?? fallback;
    }
    if (err instanceof Error) {
      return err.message;
    }
    return fallback;
  }

  try {
    await saveNote(note);
  } catch (err) {
    const message = getErrorMessage(err, "Failed to save note");
    toast.error(message);
  }
  ```

### Comment Code

- `// Note:` — อนุญาตให้ใช้สำหรับ context ที่ช่วยให้เข้าใจง่ายขึ้น
- `// TODO:` — ถ้าเจอ **ในไฟล์ที่คุณกำลังแก้ไข** ให้แจ้งผู้ใช้ก่อนดำเนินการต่อ

### Before Creating New Code

ก่อนสร้าง component, hook, util หรือ type ใหม่ — ค้นหาก่อนว่ามี implementation ที่เทียบเท่าอยู่แล้วหรือไม่ และนำกลับมาใช้ซ้ำเมื่อเป็นไปได้ ก่อนตั้ง type ใหม่ ให้ถามผู้ใช้เพื่อยืนยันชื่อ เว้นแต่ requirement จะระบุชื่อไว้ชัดเจนแล้ว

### Loading Indicator

จำแนกตามสิ่งที่ปรากฏบนหน้าจอ ไม่ใช่ตามระยะเวลาที่ call อาจใช้ — agent สามารถดูจาก markup/template ได้เสมอว่าสิ่งนั้น static, fetch มา หรือเป็น action ที่ถูก trigger; แต่ไม่มีทางรู้ latency จริงล่วงหน้า

| รูปแบบ UI                                                                                                    | วิธีจัดการ                                                       |
| ------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------- |
| ข้อความ/เนื้อหา static ที่ไม่ขึ้นกับการ fetch ใดๆ                                                                | ไม่ต้องมี indicator                                                           |
| View ที่ populate ด้วยข้อมูลจาก fetch — list, detail page, option ของ dropdown, field แบบ `[label]: [input]` ใดๆ ที่มาจาก API | Skeleton UI                                                            |
| Action ที่ผู้ใช้ trigger — submit, save, delete, ปุ่มใดๆ ที่ mutate ข้อมูล                                           | Spinner — disable control และแสดง spinner inline/บนปุ่ม |

#### Rules

- Dropdown/select ที่ option มาจาก API นับเป็นข้อมูลจาก fetch แม้จะเป็น control เล็กๆ — ใส่ skeleton ให้ ห้ามมองว่า static เพียงเพราะมันดูกะทัดรัด
- ใช้ full-screen/backdrop spinner เฉพาะตอนที่ action นั้น block ทั้งหน้าจอ (เช่น การ submit ของ wizard หลายขั้นตอน) — นอกนั้นให้ spinner อยู่แค่ใน control ที่ trigger เท่านั้น
- ถ้ามี case คล้ายกันที่มี loading treatment อยู่แล้วใน repo ให้ทำตามแบบเดิม — ความสม่ำเสมอสำคัญกว่าการคิด pattern ใหม่จากศูนย์

### Commands

```bash
npm run dev          # Start dev server (Vite)
npm run build         # TypeScript check + Vite build
npm run preview       # Preview production build
npm run lint          # oxlint check
```

## Testing & Quality

ก่อนถือว่างานเสร็จ:

1. รัน `npm run lint` — แก้ oxlint error ให้หมด
2. รัน `npm run build` — แก้ TypeScript + build error ให้หมด
3. รัน test suite เมื่อมีการตั้งค่าไว้แล้ว (ดู Test Runner ด้านล่าง)

### Test Runner

ยังไม่ได้ตั้งค่า — `package.json` ไม่มี Vitest/Jest และไม่มี script `test` เมื่อถึงเวลาที่ต้องการ test runner ให้ตั้งค่าขึ้นมา (Vitest เหมาะกับ project ที่ใช้ Vite ที่สุด) แล้วอัปเดต section นี้แทนที่จะปล่อยให้ test ไม่ถูก track

### Unit Test Rules

ต้องมี unit test สำหรับ:

- Calculation logic
- Form validation

ไม่ต้องเขียน unit test สำหรับ:

- Presentational component ที่ไม่มี logic
- พฤติกรรมของ third-party library

ถ้าต้องการ test logic ที่ฝังอยู่ใน component ให้แยกออกมาเป็น pure function ก่อน (เช่นใน `/utils/`)

### Rules

- **ห้ามแก้ test เพื่อให้มันผ่าน** เมื่อ test พังหลังจากแก้โค้ด ให้หยุดแล้วรายงานว่า test ไหนพังและเพราะอะไร — ตัดสินใจว่า _โค้ด_ regress จริงหรือ expected behavior เปลี่ยนไปจริงๆ สรุปผลกระทบ แล้วรอการอนุมัติอย่างชัดเจนก่อนแตะ test ใดๆ อย่าคิดเอาเองว่า test ผิดเพียงเพราะมันขึ้นสีแดง
