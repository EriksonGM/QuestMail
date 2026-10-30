# QuestMail agent instructions

These instructions apply to the entire repository. Read [CONTRIBUTING.md](CONTRIBUTING.md) before changing code, tests, workflows or documentation. Repository documentation, commit messages and release notes are written in English; communicate with the user in their preferred language.

## 1. Inspect the request and working tree

1. Confirm whether the user requested an implementation, an explanation, a commit, a push or a release. Do not turn a request for advice into edits.
2. Read this file and any more-specific `AGENTS.md` instructions affecting the files you will touch.
3. Run `git status --short --branch` and inspect existing diffs before editing or staging. Treat unfamiliar changes as user work.
4. Read the relevant implementation, tests and documentation. Keep changes scoped to the requested task and preserve existing behavior unless the task changes it.
5. Do not reset, discard, overwrite or stash user changes, delete unrelated files, stop unrelated services or rewrite shared history. Ask if safe isolation is unclear. Do not delegate to subagents unless explicitly requested by the user or applicable instructions.

## 2. Work on an isolated branch

- `master` is the integration/release branch, not a development workspace. Do not make task commits or push directly to it.
- Use one short-lived branch per task: `feature/<issue>-<description>`, `fix/...`, `docs/...`, `test/...`, `ci/...` or `chore/...`. Omit the issue number only when none exists; do not invent one.
- If the working tree is clean and the user has not selected an existing task branch, fetch the remote and create a branch from `origin/master` without tracking `master`:

  ```sh
  git fetch origin
  git switch --no-track -c feature/123-short-description origin/master
  ```

- If the user already has a task branch, preserve it. Do not switch another person's dirty work onto a different base or include unrelated work silently.
- Update a shared task branch by merging `origin/master` after coordinating and checking that the working tree is safe. Do not rebase or force-push shared branches. Rewriting a private unpublished branch requires explicit user authorization.
- Do not merge unrelated feature branches into each other. For a genuine dependency, use a declared dependent PR or merge a small prerequisite PR into `master` first. After integration of a dependency, review the remaining PR diff and retarget it deliberately.

## 3. Implement a focused, reviewable change

1. Identify public API, compatibility, data/privacy and release impact before implementation. State material assumptions or blockers to the user.
2. Follow existing C# naming/style and .NET 10 settings in `global.json`. Avoid unrelated refactors or new dependencies without a task-specific reason.
3. Keep `QuestMail.Core` a content-composition/rendering library: do not add SMTP delivery, image downloading, analytics endpoints or preview-only dependencies to it.
4. Preserve immutable component/style snapshots. Every body-copy/fluent operation must retain root settings, including `TrackingPixelUrl`, unless that setting is explicitly changed.
5. Preserve HTML/attribute encoding, URL validation, restricted custom CSS, component nesting rules, accessibility and desktop/mobile behavior. Do not introduce raw HTML/CSS or weaken validation to make an example render.
6. Keep tracking opt-in and disabled by default. Do not add tracking endpoints, real recipient data or external requests to tests/templates without explicit permission.
7. Add regression tests with behavior changes. Put new email previews in `tests/QuestMail.MailDev/Templates`; use embedded local assets and meaningful alt text when imagery is needed.
8. Update affected English documentation and `CHANGELOG.md` under `Unreleased` for meaningful changes. Do not manually bump the package version or create a release tag for ordinary feature work.

## 4. Validate before handing off or committing

Run from the repository root:

```sh
dotnet restore QuestMail.slnx
dotnet build QuestMail.slnx --configuration Release --no-restore
dotnet test --solution QuestMail.slnx --configuration Release --no-build --no-restore
python -m unittest discover -s .github/scripts -p 'test_*.py'
dotnet pack src/QuestMail.Core/QuestMail.Core.csproj --configuration Release --no-build --no-restore --output artifacts
git diff --check
```

- Use `python3` instead of `python` where required by the local environment. `global.json` selects Microsoft.Testing.Platform: retain the `dotnet test --solution` syntax.
- For workflow changes, run `actionlint` when available and review triggers, permissions and required-check names. Never execute publishing steps as a validation shortcut.
- For visual template changes, follow `tests/README.md`, inspect desktop/mobile previews and check that local images load without horizontal overflow. Do not disturb another project's containers or ports; use the documented port overrides.
- Report what actually ran and its result. If a dependency, credential, service or tool prevents validation, state the limitation; do not claim unrun checks passed.

## 5. Commit and push only when requested

1. Review `git diff`, `git status` and the intended branch again. Stage task-related paths; include other user changes only when explicitly requested.
2. Group changes into focused commits when appropriate/requested. Use English Conventional Commit subjects: `feat:`, `fix:`, `docs:`, `test:`, `ci:`, `chore:` or `refactor:`. Document breaking changes in the commit/PR and migration notes.
3. Inspect `git diff --cached` and run `git diff --cached --check` before each commit. Never commit credentials, generated build/test output, Python caches or temporary preview artifacts.
4. When asked to push, push the task branch, not `master`. For a new branch use `git push -u origin HEAD`. Never use force push, move release tags or bypass review/checks.
5. Do not open or merge a PR, change remote repository settings, delete remote branches or publish a release unless that action was requested. A generic request to commit/push is not authorization to merge into `master` or publish.

## 6. Integration must use a reviewed PR

- Target `master` (or an explicitly declared prerequisite branch for a dependent PR), fill in `.github/pull_request_template.md`, link the issue/dependencies and provide tests and release/migration impact.
- Require approval from another collaborator, resolved conversations and the successful `ci-validation` check on the current integration candidate. Do not self-approve, manufacture status checks or override protections.
- Integrate with **Create a merge commit** to preserve original commit IDs/authors and branch history. Do not squash or rebase merges under the current project policy.
- If conflicts occur, inspect both sides, preserve the intended behavior and rerun validation after resolution. Never resolve the entire conflict set blindly with “ours” or “theirs”.
- If a merge queue is enabled, use its merge-commit method and wait for the merge-group checks. The CI workflow already supports `merge_group`.
- Only release-ready work belongs in `master`. Feature flags can disable behavior but do not remove code from the published package.

## 7. Release safeguards

- `.github/workflows/ci.yml` is validation-only: no publishing credentials, `id-token: write`, release environment, tag creation or package publication.
- `.github/workflows/publish.yml` remains the only publishing workflow. It must be manual-only, run from `master`, require Major/Minor/Patch, serialize releases, use environment `released` and NuGet Trusted Publishing/OIDC, and publish only the validated artifact.
- Keep the exact validated SHA, package version, version tag and GitHub release consistent. Never rebuild from a later `master` commit for the same release.
- Release descriptions must include every commit since the previous stable tag, with commit/comparison links. Preserve the generator's ancestry check, first-release behavior and tests. `CHANGELOG.md` remains a separately maintained human-readable file.
- Do not overwrite a NuGet version, force-update a release tag, or add automatic/tag/API-key publishing alternatives. Follow `docs/trusted-publishing.md` for authorized releases and recovery.

## 8. Final handoff

Summarize the outcome, current task branch, validation, remaining risks and any requested commits/push/PR. Distinguish local files from GitHub settings: adding this policy or a workflow does **not** activate remote branch protection. Do not claim reviews, merge queues, rulesets or environment approvals are enabled without verifying/applying them with explicit authorization. Maintainer setup is documented in `CONTRIBUTING.md`.
