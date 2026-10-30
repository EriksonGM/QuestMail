# CI and releases

- Releases run only through **Actions → Build, Test and Publish → Run workflow**, using the `master` branch. Pull requests, pushes, tags and GitHub release events do not trigger this workflow.
- Select the required release type: **Major** increments the major version and resets minor/patch; **Minor** increments minor and resets patch; **Patch** increments patch.
- The workflow increments the highest stable `vMAJOR.MINOR.PATCH` Git tag numerically, ignoring prerelease and unrelated tags. If no stable tags exist, it increments the project version instead. For example, from `v0.1.0`, Major produces `1.0.0`, Minor produces `0.2.0` and Patch produces `0.1.1`.
- The selected commit is restored, built, tested and packed on .NET 10 with the calculated version. Only the validated package is published; publication runs are serialized to avoid concurrent version allocation.
- Publishing uses [NuGet Trusted Publishing](docs/trusted-publishing.md) and GitHub OIDC. Configure the NuGet trust policy for `EriksonGM/QuestMail`, workflow file `publish.yml` and environment `released`; no long-lived `NUGET_API_KEY` repository secret is required.
- After NuGet publication succeeds, the workflow creates the corresponding Git tag and GitHub release at the validated commit. There is no existing-tag recovery input or API-key publishing alternative; duplicate NuGet versions fail rather than being silently skipped.
- The project version remains `0.1.0` for local builds; the workflow overrides it consistently during release build and pack.
- Project documentation, changelog entries, release notes and release messages are maintained in English.
