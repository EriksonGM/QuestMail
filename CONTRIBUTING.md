# Contributing to QuestMail

## Required collaboration policy

QuestMail uses **GitHub Flow with a protected `master`, short-lived task branches, reviewed pull requests and merge commits**. This policy applies to code, tests, examples, workflows and documentation. Automated coding agents must also follow [AGENTS.md](AGENTS.md).

```text
Issue → task branch → reviewed PR → merge commit on master
      → validated SHA → NuGet package → matching version tag and GitHub release
```

`master` must remain release-ready. Do not push feature commits directly to it. There is no permanent `develop` branch, and ordinary work does not create release branches or version tags. Every change merged into `master` is eligible for the next release; a feature flag can disable behavior but does not exclude its code from the package.

Documentation, commit messages and release notes are maintained in English.

## 1. Start from an issue and an isolated branch

Create/link an issue describing the change, acceptance criteria and compatibility impact when possible. Keep a PR small enough to review; split independent features and unrelated refactors.

Inspect your working tree first. If it is clean, start a branch from the latest remote integration branch:

```sh
git status --short --branch
git fetch origin
git switch --no-track -c feature/123-short-description origin/master
```

Use `feature/`, `fix/`, `docs/`, `test/`, `ci/` or `chore/` prefixes, followed by the issue number and a short description. If there is no issue, use a meaningful description without inventing a number. Do not reset, discard or stash another contributor's changes to make these commands work.

Push work regularly to its own branch once you are ready to share it; do not store the only copy of important work locally. For a new branch:

```sh
git push -u origin HEAD
```

Agents must wait for user authorization before committing or pushing.

## 2. Collaborate without rewriting shared history

After fetching, bring `origin/master` into a shared task branch with a normal merge when its working tree is clean and collaborators are coordinated:

```sh
git fetch origin
git merge origin/master
```

- Do not force-push or rebase branches other people use. Private unpublished history is different, but agents still need explicit authorization before rewriting it.
- Review conflicts file by file. Understand both changes, preserve the intended behavior and rerun tests after resolving them. Do not resolve all conflicts using one side automatically.
- Avoid merging unrelated feature branches into one another. Integrate a small shared prerequisite into `master` first whenever possible.
- For a genuine stacked dependency, open the dependent PR against the prerequisite branch, state that dependency in both PRs and integrate the prerequisite first. Then deliberately retarget/update the remaining PR, inspect its diff against `master` and rerun CI. Do not merge the dependent PR as if it were independent.
- Keep incomplete work out of `master`, even if it compiles. Use draft PRs to share work before it is ready.

## 3. Implement and document the change

- Follow the existing .NET 10, nullable, naming, component and style patterns.
- Preserve immutable snapshots, fluent root settings, HTML/URL/CSS validation, accessibility and responsive behavior.
- Keep SMTP/preview tooling in `tests/QuestMail.MailDev`, not in `QuestMail.Core`. Do not introduce external network calls into rendering or automated tests.
- Add regression tests, update relevant English guides and record meaningful changes under `Unreleased` in [CHANGELOG.md](CHANGELOG.md).
- State breaking public API/behavior changes and required consumer migrations explicitly. Do not hide incompatible changes in a generic refactor.
- Do not commit secrets, `bin/`, `obj/`, `artifacts/`, `TestResults/`, Python caches or temporary preview output.
- Do not manually change the package version for ordinary development: release build/pack receive their version from the publishing workflow.

## 4. Validate locally

Prerequisites: Git, a .NET 10 SDK compatible with `global.json`, and Python 3.10 or newer. Docker is needed only for optional visual email delivery previews.

Run from the repository root:

```sh
dotnet restore QuestMail.slnx
dotnet build QuestMail.slnx --configuration Release --no-restore
dotnet test --solution QuestMail.slnx --configuration Release --no-build --no-restore
python -m unittest discover -s .github/scripts -p 'test_*.py'
dotnet pack src/QuestMail.Core/QuestMail.Core.csproj --configuration Release --no-build --no-restore --output artifacts
git diff --check
```

Use `python3` on systems where that is the Python command. The repository selects Microsoft.Testing.Platform: use the `--solution` form shown above. Python tests create temporary local Git repositories and never publish or contact GitHub.

For workflow changes, run `actionlint` when available. For template changes, follow the [MailDev preview instructions](tests/README.md), check desktop/mobile layouts and local image loading, and ensure there is no horizontal overflow. Never stop another project's containers or claim checks passed when they could not be run.

## 5. Commit and open a PR

Use focused English Conventional Commit subjects, for example:

```text
feat: add optional tracking pixel
fix: retain body settings during fluent copies
docs: clarify release workflow
test: cover embedded preview images
ci: validate pull request integration
```

Inspect both the working diff and staged diff before committing. Stage only intended changes; include another contributor's work only with clear authorization. If asked for separate commits, split by coherent responsibility rather than arbitrary file count.

Open a PR against `master` using the provided [PR template](.github/pull_request_template.md). Include:

- Linked issue and any prerequisite/dependent PRs.
- What changed, why, and what is intentionally out of scope.
- Public API/behavior impact, breaking changes and migration instructions.
- Commands actually run and their results; note any validation limitations.
- Screenshots or preview observations when visual output changed.
- Release impact and any maintainer attention required before publication.

Before integration, obtain approval from at least one other collaborator, resolve review conversations and pass the required CI check on the current integration candidate. New commits after approval must be reviewed again as configured in the repository's protection rules.

## 6. Integrate with a merge commit

Use **Create a merge commit**, not squash or rebase merging. This preserves the original commit identities, authors and branch relationships that the release changelog records. Never bypass reviews/checks or push a feature directly to `master` as a substitute for a PR.

Without a merge queue, bring the branch up to date with `master` and rerun required checks before integration. If a merge queue is enabled, use its **merge** method and wait for its checks on the combined merge group. A PR check passing before another PR merges is not proof the new combined state is valid.

Delete the task branch only after confirming it is merged and no remaining PR depends on it. Agents need authorization for PR creation/merging and remote branch deletion; a request to commit/push does not authorize those actions.

## 7. CI validates; releases publish manually

[`.github/workflows/ci.yml`](.github/workflows/ci.yml) runs on PRs targeting `master`, pushes to `master` and merge-queue candidates targeting `master`. It restores, builds and tests the solution, tests release-note generation and verifies local NuGet packing. Its stable required-check name is **`ci-validation`**.

CI runs on documentation-only PRs too: do not add path filters or skip directives that leave a required check pending. It uses `contents: read`, does not persist checkout credentials, and never requests OIDC publishing credentials, uses the `released` environment, creates tags/releases, uploads packages to a registry or publishes to NuGet. Its `0.1.0-ci.<run-number>` package is a local validation artifact, not a public release.

[`.github/workflows/publish.yml`](.github/workflows/publish.yml) remains the **only** publication path. An authorized maintainer starts it manually from `master`, selects Major/Minor/Patch and follows the [Trusted Publishing guide](docs/trusted-publishing.md). It validates the selected SHA, publishes the validated artifact through OIDC in environment `released`, and creates the matching tag/release with all commits since the previous stable tag. Later changes to `master` do not enter that already selected release.

Do not overwrite published NuGet versions, move/reuse stable tags, add automatic publication triggers or replace Trusted Publishing with a long-lived API-key secret. Release descriptions are generated automatically; the human-readable `CHANGELOG.md` is maintained separately. Hotfixes use reviewed `fix/...` PRs to `master` under this same policy; maintaining multiple supported release branches would require a separately reviewed versioning/publication design.

## Maintainer setup: activate enforcement in GitHub

**Files in this repository describe the policy but do not activate remote protection.** Apply these settings with repository-administrator authorization. Availability of rulesets, environment protections and merge queues depends on repository visibility and GitHub plan.

### Bootstrap safely

1. Introduce this policy and `ci.yml` through a bootstrap PR. Run/approve GitHub Actions as required by repository settings.
2. Once the CI check has reported, select the actual **`ci-validation`** check from the **CI** workflow in the branch rules. Do not require a check name that has never run; that can block every PR. Verify the displayed name/source in GitHub rather than inventing a different context.
3. Do not require the manual publish workflow as a PR check: publication is not a merge prerequisite.

### Repository merge settings

Under **Settings → General → Pull Requests**:

- Enable **Allow merge commits**.
- Disable **Allow squash merging** and **Allow rebase merging** for the current policy.
- Decide branch cleanup deliberately: do not auto-delete branches that still serve as dependent PR bases.

### Rules for `master`

Under **Settings → Rules → Rulesets** (or branch protection where rulesets are unavailable), target `master` and enforce:

- Require a PR before integration and at least **one approval by another collaborator**.
- Dismiss stale approvals after reviewable new commits; require approval of the latest reviewable push by someone other than its pusher where supported.
- Require resolution of review conversations.
- Require **`ci-validation`**, selecting GitHub Actions as the expected source when the UI supports it.
- Without a merge queue, require branches to be up to date before merging.
- Block force pushes and deletion; do not allow routine direct pushes or broad administrator bypasses.
- Keep any emergency bypass narrowly assigned, authorized and auditable; follow up with the missing review/record.
- **Do not require linear history:** that setting is incompatible with the project's merge-commit policy.

When team ownership is known, add a real `CODEOWNERS` mapping and require the appropriate owner reviews. Do not insert placeholder reviewers or assume unverified accounts own paths.

### Optional merge queue and release protection

- Enable a merge queue only when supported/needed and select the **merge** method. CI already listens to `merge_group: checks_requested`; this alone does not enable the queue remotely.
- Protect `v*` release tags against updates/deletion. If restricting tag creation too, first verify that the actual release workflow identity is authorized to create its version tag; do not accidentally block successful NuGet publication's subsequent GitHub release step.
- Configure environment **`released`** to permit publication from `master`, with required approvals where supported. Keep the NuGet trust policy's environment matching `released`.
- Consider immutable GitHub releases where available. Never treat a failed historical run's retry as a way to pick up a newer workflow definition.

Finally, verify with a test PR that missing approvals/checks actually block merging and a current successful check allows the intended merge-commit path. Record what has really been enabled; do not claim remote enforcement from documentation alone.
