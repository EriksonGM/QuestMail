# NuGet Trusted Publishing

QuestMail publishes packages through GitHub Actions using NuGet Trusted Publishing. GitHub issues an OIDC identity token, and NuGet exchanges it for a short-lived publishing key. The workflow no longer reads a long-lived `NUGET_API_KEY` repository secret.

## One-time NuGet configuration

Sign in to nuget.org as **EriksonG**, the NuGet profile expected to create the policy, and open **Trusted Publishing** from the account menu. This account must be authorized to publish `QuestMail`. Create a GitHub trust policy with these exact values:

| Field | Value |
| --- | --- |
| Repository owner | `EriksonGM` |
| Repository | `QuestMail` |
| Workflow file | `publish.yml` |
| Environment | Leave empty |

Enter only the workflow file name, not `.github/workflows/publish.yml`. The current publish job does not use a GitHub environment, so setting an environment in the policy would prevent the identity from matching.

The policy must permit publishing versions of the `QuestMail` package. The `user` input of `NuGet/login` must be the NuGet profile that **created the policy**, not the GitHub login, an email address or a package-owning organization name. The current workflow expects the creator to be `EriksonG`. If a different NuGet profile created your policy, update `user` to that profile and verify its permission to publish `QuestMail`.

This configuration must be created on nuget.org by an authorized account owner. A GitHub repository secret cannot replace a NuGet trust policy. The old API-key secret is not used by this workflow; it can be removed after the migration is verified.

## Workflow security

- `.github/workflows/publish.yml` grants `id-token: write` only to the publish job.
- The only trigger is `workflow_dispatch`; pull requests, branch/tag pushes and GitHub release events do not start this workflow.
- The publish job uses `NuGet/login@v1` to obtain a temporary key immediately before `dotnet nuget push`.
- Only manual release runs from `master` can reach the publish job. The required release type is restricted to `Maior`, `Minor` or `Fix`; the version is calculated automatically.
- Validation has read-only repository permissions and must succeed before publishing.
- Only the publish job has `contents: write`, to create the version tag and GitHub release after NuGet publication succeeds.
- A shared concurrency group serializes release runs, so two runs cannot calculate and publish a version simultaneously.
- The workflow does not overwrite existing NuGet versions or force-update Git tags.

## Publish a new release

1. Configure the NuGet trust policy described above.
2. Commit and push the code to release to `master`.
3. Open GitHub **Actions → Build, Test and Publish → Run workflow**.
4. Select the `master` branch and the required release type:
   - **Maior**: `1.2.3` → `2.0.0`.
   - **Minor**: `1.2.3` → `1.3.0`.
   - **Fix**: `1.2.3` → `1.2.4`.
5. Start the workflow and check that both `validate` and `publish` succeed.

The workflow reads all Git tags and increments the highest stable `vMAJOR.MINOR.PATCH` version using numeric ordering, ignoring prerelease and unrelated tags. If no stable tag exists, the version in `QuestMail.Core.csproj` is the baseline. There is no manual version/tag input.

The commit selected when starting the workflow is built, tested and packaged with the calculated version. After validation, `NuGet/login@v1` obtains the short-lived key through OIDC and the package is pushed to nuget.org. The workflow then creates the corresponding Git tag and GitHub release pointing to that same commit. No API-key secret or alternative publishing mode is supported.

If NuGet publication succeeds but GitHub release creation fails, create the matching tag/release at the validated commit before starting another release, so the version baseline stays synchronized. Do not try to overwrite the package already published on NuGet.

## Troubleshooting

- **OIDC login denied:** verify the NuGet user, repository owner, repository name, workflow file name and optional environment. The workflow must match an active NuGet trust policy.
- **No matching trust policy owned by user:** create the matching policy under the profile specified by `NuGet/login` or change `user` to the policy creator's NuGet profile. Updating an API-key secret does not fix a missing trust policy.
- **Missing OIDC token permission:** ensure `id-token: write` is set on the publish job.
- **Old API-key error after migration:** start a new manual release from `master`; retrying a historical workflow uses its original definition, not this workflow.
- **Duplicate package version:** ensure the latest published version has a matching stable Git tag, then start a new manual release. The workflow deliberately does not use `--skip-duplicate`.

For the official configuration reference, see [Trusted Publishing on nuget.org](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).
