# Issue tracker: GitHub

Issues and specs for this repo live as GitHub issues. Use the `gh` CLI for all operations.

## Conventions

- **Create an issue**: `gh issue create --title "..." --body "..."`. Use a heredoc for multi-line bodies.
- **Read an issue**: `gh issue view <number> --comments`, filtering comments by `jq` and also fetching labels.
- **List issues**: `gh issue list --state open --json number,title,body,labels,comments --jq '[.[] | {number, title, body, labels: [.labels[].name], comments: [.comments[].body]}]'` with appropriate `--label` and `--state` filters.
- **Comment on an issue**: `gh issue comment <number> --body "..."`
- **Apply / remove labels**: `gh issue edit <number> --add-label "..."` / `--remove-label "..."`
- **Close**: `gh issue close <number> --comment "..."`

Infer the repo from `git remote -v`; `gh` does this automatically when run inside a clone.

## Pull requests as a triage surface

**PRs as a request surface: no.** _(Set to `yes` if this repo treats external PRs as feature requests; `/triage` reads this flag.)_

When set to `yes`, PRs run through the same labels and states as issues, using the `gh pr` equivalents:

- **Read a PR**: `gh pr view <number> --comments` and `gh pr diff <number>` for the diff.
- **List external PRs for triage**: `gh pr list --state open --json number,title,body,labels,author,authorAssociation,comments` then keep only `authorAssociation` of `CONTRIBUTOR`, `FIRST_TIME_CONTRIBUTOR`, or `NONE` (drop `OWNER`/`MEMBER`/`COLLABORATOR`).
- **Comment / label / close**: `gh pr comment`, `gh pr edit --add-label`/`--remove-label`, `gh pr close`.

GitHub shares one number space across issues and PRs, so a bare `#42` may be either: resolve with `gh pr view 42` and fall back to `gh issue view 42`.

## When a skill says "publish to the issue tracker"

Create a GitHub issue.

## When a skill says "fetch the relevant ticket"

Run `gh issue view <number> --comments`.

## Wayfinding operations

Used by `/wayfinder`. **Override, decided 2026-09-22**: wayfinder maps/tickets for this repo live in the **local-markdown tracker** under `docs/wayfinder/`, not GitHub Issues — this is narrower than the general "issues live in GitHub" convention above, which still governs everything else (bugs, features, PRs-as-request-surface). The first map (Jotwai Requirements v0.1) was originally charted as GitHub issues #7–#19 (ghost08A/Jotwai) and migrated to local files with full content preserved; those GitHub issues remain as an archive, not the live copy.

- **Map**: `docs/wayfinder/map.md`. Frontmatter: `title`, `label: wayfinder:map`. Body holds Destination / Notes / Decisions-so-far / Not yet specified / Out of scope, same structure the skill describes.
- **Child ticket**: `docs/wayfinder/tickets/<id>-<slug>.md`, one file per ticket. Frontmatter: `id` (numeric, monotonically increasing across the whole tracker), `title`, `type` (`wayfinder:research`/`prototype`/`grilling`/`task`), `status` (`open`/`closed`), `assignee` (`null` or a username — the claim), `parent` (relative path to the map), `blocked_by` (list of relative paths to blocker ticket files, empty when unblocked). Body: `## Question` (the ticket's question, fixed at creation) then `## Resolution`/`## Finding` (added when resolved).
- **Blocking**: the `blocked_by` frontmatter list, by file path. A ticket is unblocked when every file in its `blocked_by` list has `status: closed`.
- **Frontier query**: list files under `docs/wayfinder/tickets/` with `status: open`, `assignee: null`, and every entry in `blocked_by` already `status: closed`; first by `id` wins.
- **Claim**: edit the ticket file's frontmatter, `assignee: null` → `assignee: <username>` — the session's first write, before any other work on that ticket.
- **Resolve**: append a `## Resolution` (decision tickets) or `## Finding` (research tickets) section to the ticket body with the answer, set `status: closed` in frontmatter, then add a one-line pointer (gist + relative link to the ticket file) to the map's Decisions-so-far section. If a research/audit ticket surfaces a gap rather than confirming a decision, leave `status: open` and record the finding anyway — it becomes tracked backlog, not a re-litigated decision (see the current map's Notes for this project's specific convention on audit tickets).
