---
id: 33
title: ตัดสินใจว่า "AI Categorization รันไปแล้วครั้งเดียว" เช็คจากไหน
type: wayfinder:grilling
status: closed
assignee: null
parent: ../map.md
blocked_by: []
---

## Question

กฎ User Ownership (ticket 025) บอกว่า flow AI Categorization ต้องรันอัตโนมัติแค่ครั้งเดียวต่อ Note แต่ module `ai_categorization` ไม่มีสิทธิ์ query schema ของ module Notes โดยตรง (ADR-0003) คำถามคือจะรู้ได้ยังไงว่า Note นี้ "เคย trigger ไปแล้ว"

ตัวเลือก:
1. **เก็บ flag บนตาราง Notes เอง** (เช่น `AiCategorizationTriggeredAt`, module Notes เป็นเจ้าของ, อัปเดตตอนรับ event) — ตรงไปตรงมา แต่ทำให้ schema `notes` ต้อง "รู้จัก" concept ของ AI Categorization นิดหน่อย
2. **Infer จากประวัติของ `ai_categorization` schema เอง** (เช่น เคยมี Outbox/Suggestion row สำหรับ NoteId นี้ไหม) — module `ai_categorization` เป็นเจ้าของ logic เต็มๆ ไม่ผสมกับ schema อื่น แต่ query ซับซ้อนกว่านิดหน่อย

**คำแนะนำ**: ตัวเลือก 2 — สอดคล้องกับหลักการ "แต่ละ Module เป็นเจ้าของ schema ตัวเอง" ที่สุด ไม่ทำให้ schema `notes` ต้องผูกกับ concept ของ AI Categorization

## Resolution

เลือกตัวเลือก 1 (ขัดกับคำแนะนำ) — เก็บ flag บนตาราง `Notes` เอง (เช่น `Notes.AiCategorizationTriggeredAt`, timestamp nullable) module Notes เป็นเจ้าของคอลัมน์นี้และอัปเดตมันตอน subscribe event ที่เกี่ยวข้องจาก AI Categorization (เช่น หลัง publish suggestion ชุดแรก) เหตุผลที่เลือกตัวเลือกนี้แทน infer จาก `ai_categorization` schema: ผู้ใช้ตัดสินใจให้ query ตรงไปตรงมากว่า ไม่ต้องพึ่ง historical join ข้าม module ตอน enforce กฎ "รันครั้งเดียว" — schema `notes` รู้แค่ว่า "เคย trigger ไปแล้วหรือยัง" (boolean/timestamp) ไม่ได้รู้รายละเอียดของ suggestion/outbox ใดๆ ของ AI Categorization จึงยังไม่ถือว่าละเมิดขอบเขต schema-per-module อย่างมีนัยสำคัญ
