# ADR 0002: การสื่อสารระหว่าง Module ด้วย event/mediator แบบ in-process

## สถานะ
ยอมรับแล้ว

## บริบท
เมื่อพิจารณาจาก modular monolith (ADR 0001) แต่ละ Module ยังคงต้องตอบสนองต่อกันและกัน เช่น AI Categorization ต้องรู้เมื่อมีการสร้าง Note ใหม่; Search ต้องรู้เมื่อเนื้อหาของ Note หรือชื่อของ Topic/Category เปลี่ยนแปลง การเรียกเมธอดโดยตรงแบบ in-process ระหว่าง Module สามารถใช้งานได้ในตอนนี้ แต่จะทำให้เกิดการผูกติดกัน (coupling) แบบเงียบ ๆ ซึ่งขัดกับจุดประสงค์ของการวางขอบเขต

## การตัดสินใจ
Module สื่อสารกันผ่าน in-process Domain Events เท่านั้น โดยเผยแพร่ผ่าน mediator (สไตล์ MediatR) เมื่อสถานะของ Module นั้นเปลี่ยนแปลง: `NoteCreated`, `NoteUpdated`, `NoteDeleted`, `TopicRenamed`, `CategoryCreated`, `CategoryRenamed`, `CategoryDeleted` เป็นต้น ไม่มี Module ใดถือ reference ตรงไปยัง service หรือ repository ของ Module อื่น — นี่คือ **แนวปฏิบัติที่บังคับใช้ผ่าน code review** ไม่ใช่ขอบเขตระดับ project (ตาม ADR-0010 ทุก Module อยู่ใน project `Jotwai.Domain`/`Jotwai.Service` เดียวกัน จึงไม่มีสิ่งใดป้องกันการ reference ตรงในระดับ compile time)

## ผลที่ตามมา
- แต่ละ Module สามารถพัฒนา ทดสอบ และทำความเข้าใจได้โดยไม่ต้องรู้ว่า Module อื่นใด subscribe event ของตน
- ขอบเขตของ event นี้คือจุดที่การแยก microservice ในอนาคตจะ "ตัด" ออก — pub/sub แบบ in-process จะกลายเป็น message broker โดยแทบไม่ต้องเปลี่ยนรูปแบบโค้ดฝั่ง publish/subscribe
- Event เป็นแบบ in-process และซิงโครนัสโดยค่าเริ่มต้น (อยู่ใน request/transaction เดียวกัน) เว้นแต่ handler จะเลื่อนงานออกไปอย่างชัดเจน (เช่น AI Categorization ที่เขียนลง outbox ของตน — ดู ADR 0004) — จึงยังไม่มีเรื่อง distributed eventual-consistency ที่ต้องออกแบบในตอนนี้
