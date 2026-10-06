# Research: วิธีกัน LLM เปลี่ยนโครงสร้าง Markdown ตอนจัดรูปแบบ

- Ticket: [052](../tickets/052-research-ai-markdown-preservation.md)
- วันที่เก็บข้อมูล: 2026-10-05
- ขอบเขต: **รายงานข้อเท็จจริงจากแหล่ง primary เท่านั้น ไม่เลือก/ไม่แนะนำตัวเลือกแทนเจ้าของโปรเจกต์**
- หมายเหตุวิธีเก็บ: เนื้อหาจากหน้า docs ของ Google ส่วนใหญ่ดึงผ่านเครื่องมือที่สรุปหน้าเว็บให้ (ไม่ใช่การอ่านดิบทั้งหน้า) ยกเว้นตาราง thinking ที่ดึง HTML ดิบมาตรวจเอง ข้อความในเครื่องหมายคำพูดมาจากผลสรุปนั้น ถ้าจะใช้ตัดสินใจจริงควรเปิดอ่านหน้าต้นทางซ้ำอีกครั้ง

## 1. สถานะปัจจุบันของ Content Polish ในโค้ด (ข้อเท็จจริงจาก repo)

ที่มา: `backend/Jotwai.Service/AiProviders/GeminiAiProvider.cs` เมธอด `PolishNoteContentAsync` และ `BuildPolishPrompt`

- เรียก Gemini แบบ `generateContent` ตรงๆ ส่ง prompt เป็น `user` content ก้อนเดียว (ไม่ได้ใช้ `system_instruction` แยก) ตั้งค่า generation แค่ `responseMimeType = "text/plain"` ไม่ได้ตั้ง temperature, seed หรือ thinking
- Output คือข้อความ Markdown ล้วน ไม่ใช่ JSON/structured output ผลถูกส่งกลับเป็น preview ให้ผู้ใช้กดยืนยัน (ARCHITECTURE.md หัวข้อ Content Polish flow) ในโค้ดที่ตรวจดูไม่พบขั้นตรวจโครงสร้างก่อน-หลัง
- Code block ถูกป้องกันด้วยการแทนเป็น placeholder `%%JOTWAI_CODE_BLOCK_<number>%%` แล้วสั่งใน prompt ข้อ 5 ให้คงไว้ (เทคนิค "ถอดส่วนที่ห้ามแตะออกก่อนส่ง แล้วใส่กลับ") ส่วนอื่นไม่มีการถอดแบบนี้
- กฎใน prompt ที่เกี่ยวกับโครงสร้าง:
  - ข้อ 2: "Reformat long paragraphs into bullet lists where it improves readability"
  - ข้อ 6: "Preserve the Markdown structure (headings, lists, blockquotes) unless rule 2 asks you to restructure a paragraph into a list."
  - prompt ไม่ได้ระบุ ordered list (`1.` `2.`), nested list หรือ checkbox (`- [ ]`) ไว้ตรงๆ และไม่มี few-shot example
- อาการที่ผู้ใช้พบ: `1. 2. 3.` กลายเป็น `- 1. - 2.` (ตาม ticket ยังไม่ได้พิสูจน์สาเหตุในงานวิจัยนี้ เป็นแค่การวางข้อเท็จจริงเคียงกัน)

## 2. โมเดลที่ใช้: Gemini 2.5 Flash-Lite

ที่มา: https://ai.google.dev/gemini-api/docs/models/gemini-2.5-flash-lite, https://ai.google.dev/gemini-api/docs/models

- Model code `gemini-2.5-flash-lite` Google อธิบายว่าเป็น "The fastest and most budget-friendly multimodal model in the 2.5 family."
- Input สูงสุด 1,048,576 tokens, output สูงสุด 65,536 tokens, knowledge cutoff มกราคม 2025
- รองรับ: Structured outputs, Thinking, Function calling, Caching, Code execution ฯลฯ
- หน้า models ระบุว่าจำกัดการเข้าถึงรุ่น 2.5 ให้เฉพาะผู้ที่เคยใช้งานมาก่อน และแนะนำรุ่นใหม่กว่าสำหรับโปรเจกต์ใหม่ (ผลสรุปที่ดึงมาระบุ 3.5 Flash-Lite / 3.8 Flash)
- Thinking (โมเดล "คิดก่อนตอบ"): ตารางในหน้า https://ai.google.dev/gemini-api/docs/thinking ระบุ `gemini-2.5-flash-lite` เป็น Default Thinking = **Off** รองรับระดับ low / medium / high (2.5 Flash และ 2.5 Pro เป็น On) หน้าที่ดึงมาอธิบายการควบคุมผ่าน `thinking_level` ไม่พบการกล่าวถึง `thinkingBudget` ในหน้านี้
- เอกสารของ Google ที่ตรวจไม่มีตัวเลข benchmark เรื่องความสามารถรักษาโครงสร้าง Markdown ของรุ่นนี้

## 3. เทคนิคที่เอกสาร primary รองรับ

### 3.1 การออกแบบ prompt

ที่มา: https://ai.google.dev/gemini-api/docs/prompting-strategies, https://ai.google.dev/gemini-api/docs/text-generation

- "We recommend to always include few-shot examples in your prompts." (few-shot = ใส่ตัวอย่าง input/output ให้โมเดลดูเป็นแบบ) เอกสารบอกว่าตัวอย่างช่วยให้โมเดลจับ pattern แล้วนำไปใช้กับคำตอบ
- ให้ระบุข้อจำกัดว่าทำอะไร/หลีกเลี่ยงอะไร และสั่งรูปแบบ output ได้ในคำสั่ง
- System instruction (คำสั่งตั้งต้นที่แยกจากข้อความผู้ใช้): เอกสารแนะนำ (ในส่วนของ Gemini 3) ให้วางข้อกำหนดพฤติกรรม/รูปแบบ output ไว้ใน System Instruction หรือต้น prompt และ API มีพารามิเตอร์ `system_instruction` สำหรับตั้งพฤติกรรมโมเดล
- Temperature: "Lower temperatures are good for prompts that require a more deterministic or less open-ended response" (ยิ่งต่ำ ผลยิ่งคงที่ ไม่ใช่การรับประกันว่าเหมือนเดิมเสมอ) เอกสารแนะนำให้คงค่า default สำหรับ Gemini 3.x ซึ่งไม่ใช่รุ่นที่โปรเจกต์ใช้
- เอกสารที่ตรวจไม่ได้พูดถึงการรักษา Markdown syntax โดยเฉพาะ

### 3.2 Structured output (บังคับ output เป็น JSON ตาม schema)

ที่มา: https://ai.google.dev/gemini-api/docs/structured-output

- รองรับ JSON Schema แบบ subset: string, number, integer, boolean, object, array, null
- รับประกันเฉพาะ "syntactically correct JSON" **ไม่รับประกันความถูกต้องเชิงความหมาย** เอกสารเขียนว่า "While output is syntactically correct JSON, always validate values in your application"
- schema ซับซ้อน/ซ้อนลึกมากอาจถูกปฏิเสธ
- ผลต่อโจทย์นี้ (อนุมานจากโครงสร้างของ API): schema บังคับรูปทรง JSON ไม่ได้บังคับ syntax Markdown ที่อยู่ในค่า string ข้างใน
- โค้ดปัจจุบันของโปรเจกต์ใช้ `responseMimeType`/`responseSchema` อยู่แล้วกับ flow Categorization และ Topic Suggestion (ไม่ใช่ Content Polish)

### 3.3 ตรวจ/เทียบโครงสร้างก่อน-หลังแบบ deterministic (ไม่ใช้ AI ตัดสิน)

เป็นการทำฝั่งโค้ดของเรา ไม่ใช่ฟีเจอร์ของ Gemini จึงอ้างเฉพาะข้อกำหนดของ Markdown ที่ parser ใช้ตัดสินโครงสร้าง:

- **CommonMark spec** (https://spec.commonmark.org/0.31.2/):
  - bullet list marker คือ `-`, `+` หรือ `*`
  - ordered list marker คือเลขอาระบิก 1-9 หลัก ตามด้วย `.` หรือ `)`
  - เมื่อ ordered list ขัดจังหวะย่อหน้า ต้องขึ้นต้นที่เลข 1
  - การเปลี่ยนชนิด marker (เลข <-> bullet) ทำให้เป็นคนละ list
  - ข้อความรูป `- 1. foo` จึงถูกอ่านเป็น bullet list ที่ข้างในมี ordered list (อนุมานจากกฎ list item ซ้อนกัน ไม่ได้ยกตัวอย่างตรงจาก spec ที่ดึงมา)
- **GFM spec** (https://github.github.com/gfm/): task list item = list item ที่ block แรกเป็น paragraph ขึ้นต้นด้วย `[ ]` หรือ `[x]` (ไม่สนตัวพิมพ์) ตามด้วยช่องว่างอย่างน้อยหนึ่งตัว; fenced code block ขึ้นต้นด้วย backtick/tilde อย่างน้อย 3 ตัว และเนื้อหาข้างในถูกถือเป็น "literal text, not parsed"
- **mdast** (https://github.com/syntax-tree/mdast): spec ของ syntax tree (ต้นไม้โครงสร้างเอกสาร) ที่ parser Markdown ใช้ มี node ตรงกับโครงสร้างที่ ticket ถามครบ: `List` (field `ordered`, `start`, `spread`), `ListItem` (`checked` สำหรับ checkbox ตามส่วนขยาย GFM), `Heading` (`depth` 1-6), `Code` (`lang`, `meta`), `Blockquote` ทำให้ในเชิงทฤษฎีเทียบ "ลำดับชนิด node + ค่า ordered/depth/checked" ก่อน-หลังได้
- ที่ยังไม่ได้ตรวจ: เอกสารของ library parser เฉพาะตัว (เช่น remark, markdown-it, Markdig) และการวัดจริงว่าวิธีนี้จับอาการ `- 1.` ได้หรือไม่

### 3.4 ถอดส่วนที่ห้ามแตะออกก่อน แล้วใส่กลับ (placeholder)

- เป็นเทคนิคที่โค้ดของโปรเจกต์ใช้กับ code block อยู่แล้ว (ข้อ 1) ไม่พบหน้าเอกสาร primary ของ Google ที่อธิบายเทคนิคนี้โดยตรง ข้อเท็จจริงที่มีคือพฤติกรรมในโค้ดปัจจุบัน และ GFM ที่ระบุว่าเนื้อหา fenced code ถูกถือเป็น literal

## 4. สรุปข้อจำกัดที่ยืนยันได้จากแหล่ง

| หัวข้อ | ข้อเท็จจริง | แหล่ง |
| --- | --- | --- |
| Structured output | การันตีแค่ JSON ถูกไวยากรณ์ ไม่การันตีความหมาย ต้อง validate เอง | Gemini structured-output docs |
| Structured output กับ Markdown | schema คุมรูปทรง JSON ไม่ได้คุม syntax Markdown ในค่า string | อนุมานจาก docs เดียวกัน |
| Temperature ต่ำ | ทำให้ผล "deterministic มากขึ้น" ตามคำของเอกสาร ไม่ใช่การรับประกัน | Prompting strategies |
| Few-shot | เอกสารแนะนำให้ใส่เสมอ ไม่มีตัวเลขผลเฉพาะกับ Flash-Lite | Prompting strategies |
| Flash-Lite thinking | Default Off | Gemini thinking docs |
| Flash-Lite กับ Markdown | ไม่พบ benchmark/คำรับรองในเอกสาร primary ที่ตรวจ | Gemini models docs |
| Gemini 2.5 availability | จำกัดการเข้าถึงสำหรับผู้ใช้ใหม่ | Gemini models docs |

## 5. ช่องว่างของข้อมูล

- ไม่ได้ทดลองรัน prompt/เทคนิคใดกับ Gemini จริง จึงไม่ทราบว่าอะไรแก้อาการ `- 1.` ได้
- ไม่ได้ตรวจ API reference ฉบับเต็มของ `generationConfig` (ผลสรุปที่ดึงมาระบุเพียงว่ามี `temperature`, `topP`, `topK`, `seed`, `responseMimeType`, `responseSchema`)
- ไม่ได้ตรวจเอกสาร parser Markdown ฝั่ง .NET/JS และไม่ได้ตรวจฝั่ง client (Tiptap) ว่าแปลง Markdown <-> editor อย่างไร ซึ่งอาจมีผลต่อ Markdown ที่ส่งเข้า AI
