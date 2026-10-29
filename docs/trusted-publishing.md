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
- Pull requests and ordinary branch pushes validate and package code but do not request NuGet publishing credentials.
- The publish job uses `NuGet/login@v1` to obtain a temporary key immediately before `dotnet nuget push`.
- Only stable tag pushes or manual release runs from `master` can reach the publish job. Manual tag input must match `vMAJOR.MINOR.PATCH`.
- Validation has read-only repository permissions and must succeed before publishing.
- The workflow does not overwrite existing NuGet versions or force-update Git tags.

## Publish a new release

Commit and push the code to release, then create a new, unused stable tag:

```sh
git switch master
git push origin master
git tag -a v0.1.1 -m "Release QuestMail 0.1.1"
git push origin v0.1.1
```

The workflow checks out the tag, builds and tests the entire solution, creates the corresponding package version and publishes it using OIDC. No API-key rotation or secret update is needed for each release.

## Recover an existing tag

An existing tag can refer to an older workflow that still uses API-key authentication. Re-running that historical workflow uses its original definition; it does not pick up a fix committed later to `master`.

To publish `v0.1.0` with the current Trusted Publishing workflow without moving the tag:

1. Configure the NuGet trust policy described above.
2. Open GitHub **Actions → Build, Test and Publish → Run workflow**.
3. Select the `master` branch for the workflow definition.
4. Set `release_tag` to `v0.1.0`.
5. Start the workflow and check that both `validate` and `publish` succeed.

The workflow definition comes from `master`, while source code comes from `refs/tags/v0.1.0`. The tag is validated, compiled, tested and packaged again before publishing. The package version remains `0.1.0` and its repository commit remains the original tag commit.

This is intended for a package version that has not been published yet. If the version already exists on NuGet, create a new version instead of attempting to overwrite it.

## Troubleshooting

- **OIDC login denied:** verify the NuGet user, repository owner, repository name, workflow file name and optional environment. The workflow must match an active NuGet trust policy.
- **No matching trust policy owned by user:** create the matching policy under the profile specified by `NuGet/login` or change `user` to the policy creator's NuGet profile. Updating an API-key secret does not fix a missing trust policy.
- **Missing OIDC token permission:** ensure `id-token: write` is set on the publish job.
- **Old API-key error after migration:** use the manual recovery flow from `master`; retrying an old tagged workflow still runs the old authentication steps.
- **Duplicate package version:** choose a new release version. The workflow deliberately does not use `--skip-duplicate`.

For the official configuration reference, see [Trusted Publishing on nuget.org](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).
