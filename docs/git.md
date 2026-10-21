# Git

> Bootstrap'ta karara bağlandı (2026-10-19). Repo: GitHub · CI: GitHub Actions (`.github/workflows/check.yml`).
> Varsayılan dal `main`; dal koruması açık, PR'da CI `check` ve `doctor` yeşili zorunlu.

## Branching
- `feature/<spec-no>-<short-name>` — **no branch without a spec.** Also for CHANGE-lane mini-specs.
- Refactors: `refactor/<spec-no>-<short-name>` (mini-spec with Changed behavior = none).
- Fixes: `fix/<report-id>-<short-name>` (bug fixes carry a report, not a spec number);
  incidents: `incident/<date>-<short-name>`; trivial changes: `trivial/<short-name>` (see below).
- Çalışma alanı kurulumu (bootstrap ve yeniden bootstrap): `chore/bootstrap` — davranış değiştirmez,
  yalnızca docs/ ve ANEW yapılandırması; PR + bir reviewer.

## Commits
- Conventional Commits, with a plan reference: `feat(catalog): paging endpoint [plan 0001/3]`.
  Scope = modül adı, kebab-case: `feat(leave-balance): advance limit [plan 0003/2]`.
- Agent commits follow the same standard: the agent writes the message, the human approves.

## Forbidden
- Direct commits to the default branch.
- Force push, history rewriting on shared branches. Undo = `git revert` (see recovery R-11).

## Trivial changes
<!-- Applies only to requests the change-request triage rubric (workflows/change-request.md)
     classifies as TRIVIAL: no acceptance criterion changes, no shared code. -->
- Policy: **(1) PR + one reviewer, no spec** — bootstrap'ta seçildi (2026-10-19). Dal:
  `trivial/<short-name>`; doğrudan `main`'e commit yine yasak.
- Always: one narrow commit, `scripts/check` green. A second file or a new test means re-triage.

## Pull requests
- PR template checklist completed; `scripts/check` green in CI; squash-merge.
