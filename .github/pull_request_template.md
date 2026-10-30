## Summary

<!-- Explain what changed, why, and what is intentionally out of scope. -->

## Issue and dependencies

<!-- Link the issue (e.g. Closes #123). List prerequisite/dependent PRs or state none. -->

## Validation

<!-- List commands actually run and their results. Explain skipped/unavailable checks. -->

- .NET restore/build/tests:
- Release-note generator tests:
- Local NuGet pack:
- Workflow lint / visual MailDev checks, when applicable:

## Compatibility and release impact

<!-- Describe public API/behavior changes, breaking changes, migration steps, and privacy/security implications. State none when appropriate. -->

## Visual changes

<!-- Attach screenshots or describe desktop/mobile preview results when email output changed. -->

## Checklist

- [ ] This is a focused task branch, not direct development on `master`.
- [ ] The PR targets `master`, or its temporary prerequisite branch/dependency is explicitly documented.
- [ ] Conflicts were reviewed on both sides; no unrelated work was discarded.
- [ ] Tests and English documentation/changelog were updated where relevant.
- [ ] Only release-ready code is proposed for `master`; no secrets or generated output are included.
- [ ] Actual validation results and any limitations are recorded above.
- [ ] Before integration, another collaborator must approve, conversations must be resolved and `ci-validation` must pass on the current candidate.
- [ ] Integration will use **Create a merge commit**, not squash/rebase; package publication remains manual-only.
