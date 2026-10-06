---
id: 52
title: "Research: วิธีกัน LLM เปลี่ยนโครงสร้าง Markdown ตอนจัดรูปแบบ"
type: wayfinder:research
status: closed
assignee: null
parent: ../map-v2.md
blocked_by: []
---

## Question

เทคนิคที่มีหลักฐานรองรับสำหรับให้ LLM จัดรูปแบบเนื้อหาโดยรักษาโครงสร้าง Markdown เดิม (ordered/unordered/nested list, heading, code block, checkbox, blockquote) เช่น การออกแบบ prompt, structured output, ตรวจ/เทียบโครงสร้างก่อน-หลังแบบ deterministic, ให้ LLM แก้เฉพาะข้อความไม่แตะ syntax — ข้อเท็จจริง ข้อดีข้อเสีย และข้อจำกัดโดยเฉพาะกับโมเดลระดับ Gemini Flash-Lite ที่ Content Polish ใช้อยู่ — **รายงานข้อเท็จจริงเท่านั้น ห้ามเลือกแทนเจ้าของโปรเจกต์** เก็บที่ `docs/wayfinder/research/ai-markdown-preservation.md`

## Finding

รวบรวมเสร็จ (2026-10-05) ที่ [research/ai-markdown-preservation.md](../research/ai-markdown-preservation.md) — ข้อเท็จจริงล้วน ไม่เลือกแทนเจ้าของโปรเจกต์ ประเด็นหลัก: โค้ดปัจจุบัน (`GeminiAiProvider.PolishNoteContentAsync`) ส่ง prompt ก้อนเดียว ไม่มี few-shot ไม่ระบุ ordered list/checkbox ตรงๆ (ข้อ 2 สั่งแปลงย่อหน้าเป็น bullet) และไม่มีตรวจโครงสร้างก่อน-หลัง; Gemini structured output การันตีแค่ JSON ถูกไวยากรณ์ ไม่คุม syntax Markdown ในค่า string; CommonMark/GFM/mdast ให้เกณฑ์เทียบโครงสร้างแบบ deterministic ได้

ข้อจำกัดของ research: เนื้อหา Google docs ส่วนใหญ่ได้จากสรุปหน้าเว็บ ไม่พบ thinkingBudget ในหน้าที่ดึง ไม่ได้ทดลองรันกับ Gemini จริง ไม่ได้ตรวจเอกสาร parser Markdown/Tiptap และไม่พบ benchmark เรื่องรักษา Markdown ในแหล่ง primary
