# Jotwai Domain Glossary

Shared vocabulary for the product domain and the architecture. Use these exact terms in code, comments, commits, and specs — don't invent synonyms.

## Product domain

**Note**
The core unit of capture. Freeform content (plain text, bulleted list, numbered list, code blocks) stored as Markdown. Can exist with no Topic and no Category. Soft-deletable. No version history — edits overwrite.

**Topic**
A hierarchical classification a Note can optionally belong to. A Note belongs to at most one Topic. Topics can have child Topics (unlimited depth). Sibling Topic names are unique (case-insensitive) under the same parent. A Topic with Notes or child Topics directly inside it cannot be deleted until emptied.

**Category**
A flat, non-hierarchical label. A Note can have multiple Categories. Category names are globally unique (case-insensitive). Freely creatable by the user, or proposed by AI Categorization.

**AI Categorization**
The background process that analyzes a newly created Note and suggests candidate Categories. Runs exactly once, the first time a Note is saved with real content. Never re-triggered automatically by edits (see "User Ownership" rule below). Can be re-triggered manually by the user via an on-demand action.

**Suggestion (Category Suggestion)**
A ranked list of candidate Categories produced by AI Categorization for a Note, including possibly a brand-new Category name not yet in the system. A Note has at most one active suggestion set at a time. The user may accept any subset, edit, or ignore; acting on the set resolves and discards the rest.

**User Ownership**
The rule that once a Note's Topic/Category is set (by the user or by accepting an AI suggestion), the system never moves or re-categorizes it automatically again. Only an explicit user action changes it.

**Search**
Case-insensitive substring match across a Note's content (including code block bodies), a code block's declared language, its Topic, and its Categories.

## Architecture domain

**Module**
A bounded, independently-reasoned-about slice of the backend modular monolith. See `ARCHITECTURE.md` for the module map. Each Module owns its own database schema; no other Module's code may query it directly.

**Domain Event**
An in-process notification (e.g. `NoteCreated`, `TopicRenamed`, `CategoryDeleted`) published by the Module that owns the change, and consumed by any other Module that needs to react — the only sanctioned way Modules talk to each other.

**Read Model (Projection)**
A denormalized, event-synced copy of data a Module needs but doesn't own — e.g. the Search Module's copy of Note content and Topic/Category names, kept current via Domain Events rather than cross-schema SQL joins.

**Outbox**
The durable table backing the AI Categorization background job: a row is written transactionally alongside the Note that needs processing, and a `BackgroundService` polls and processes it, so pending work survives an app restart.

**Modular Monolith**
The chosen starting architecture: a single deployable ASP.NET Core application internally split into Modules with strict boundaries (own schema, communicate only via Domain Events), designed so any Module could later be extracted into its own service without a rewrite.
