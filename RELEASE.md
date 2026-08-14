# Release Process Guide

This document describes how to create and publish new releases of `CDS.SQLiteLogging`.

## Prerequisites

### First-Time Setup

1. **NuGet API Key**
   - Go to https://www.nuget.org/account/apikeys
   - Create a new API key with "Push" permissions for this package
   - Add it as a GitHub secret named `NUGET_API_KEY`:
     - Go to: https://github.com/nooogle/CDS.SQLiteLogging/settings/secrets/actions
     - Click "New repository secret"
     - Name: `NUGET_API_KEY`
     - Value: Your NuGet API key

2. **Codecov Token (Optional for code coverage)**
   - Go to https://codecov.io and sign in with GitHub
   - Add your repository
   - Copy the upload token
   - Add it as a GitHub secret named `CODECOV_TOKEN`

## Creating a Release

The project uses [MinVer](https://github.com/adamralph/minver) for automatic semantic versioning based on git tags.

### Version Format

Follow [Semantic Versioning](https://semver.org/):
- `V{MAJOR}.{MINOR}.{PATCH}` (for example `V2.5.0`, `V2.5.1`) — the leading `V` is required; it's
  configured via `MinVerTagPrefix` in `Directory.Build.props`, and a lowercase `v` tag will not be
  recognized by MinVer

**When to increment:**
- **MAJOR**: Breaking API changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Branch Protection

`master` is protected (see `.github/workflows` hardening and the repo's Branches settings):

- Direct pushes to `master` are rejected, including for admins — all changes land via a pull request.
- **0 approvals are required**, so as the sole maintainer you can merge your own PR — you just need the required status checks to go green first.
- Required checks: `Build Core Library & Test` (ubuntu-latest / windows-latest / macos-latest) and `Build Windows Projects`.
- Force-pushes and branch deletion are blocked.

This does **not** affect tagging: pushing a `V*.*.*` tag (step 5 below) works exactly as before, since branch protection only governs the `master` branch ref, not tags.

### Step-by-Step Release Process

1. **Push your changes on a branch and open a PR into `master`**
   ```bash
   git checkout -b prep-v2.5.0
   git push -u origin prep-v2.5.0
   gh pr create --base master --fill
   ```

2. **Wait for the required checks to pass, then merge the PR**
   - Check: https://github.com/nooogle/CDS.SQLiteLogging/actions
   - No second reviewer is needed — merge it yourself once it's green:
     ```bash
     gh pr merge --merge
     ```

3. **Update any relevant docs**
   - Update `README.md` if public usage changed
   - Update workflow or setup docs if release behavior changed
   - (Include these doc updates in the same PR as step 1 where practical, rather than a separate round-trip)

4. **Pull the merged `master` locally**
   ```bash
   git checkout master
   git pull origin master
   ```

5. **Create and push the version tag**
   ```bash
   git tag V2.5.0
   git push origin V2.5.0
   ```

6. **GitHub Actions will automatically:**
   - Build the project
   - Run all tests
   - Create NuGet package
   - Create GitHub release with auto-generated notes
   - Publish to NuGet.org
   - Attach the `.nupkg` file to the GitHub release

7. **Verify the release**
   - Check GitHub releases: https://github.com/nooogle/CDS.SQLiteLogging/releases
   - Check NuGet.org: https://www.nuget.org/packages/CDS.SQLiteLogging
   - The package may take 5-10 minutes to appear in search after publishing

## Pre-release Versions

For pre-release versions (alpha, beta, RC):

```bash
# Create a tag with pre-release identifier
git tag V2.5.0-alpha.1
git push origin V2.5.0-alpha.1

# Or
git tag V2.5.0-beta.1
git push origin V2.5.0-beta.1

# Or
git tag V2.5.0-rc.1
git push origin V2.5.0-rc.1
```

MinVer will automatically mark these as pre-release versions in NuGet.

## Manual Release (Emergency)

If the automatic workflow fails, you can manually release:

```bash
# Restore, build, and test
dotnet restore CDS.SQLiteLogging/CDS.SQLiteLogging.csproj
dotnet restore CDS.SQLiteLogging.Tests/CDS.SQLiteLogging.Tests.csproj
dotnet build CDS.SQLiteLogging/CDS.SQLiteLogging.csproj --configuration Release
dotnet test --project CDS.SQLiteLogging.Tests/CDS.SQLiteLogging.Tests.csproj --configuration Release

# Push to NuGet
# The package is created in bin/Release during the build step (GeneratePackageOnBuild is enabled)
dotnet nuget push CDS.SQLiteLogging/bin/Release/CDS.SQLiteLogging.*.nupkg --api-key YOUR_NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
```

> **Note for Windows/PowerShell users:** PowerShell does not expand wildcard patterns for native commands. Use the following instead:
> ```powershell
> $pkg = (Get-ChildItem -Path CDS.SQLiteLogging/bin/Release -Filter "CDS.SQLiteLogging.*.nupkg").FullName
> dotnet nuget push $pkg --api-key YOUR_NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
> ```

## Rolling Back a Release

**NuGet packages cannot be deleted**, but you can unlist them:

1. Go to https://www.nuget.org/packages/CDS.SQLiteLogging
2. Click "Manage Package"
3. Select the version and click "Unlist"
4. The version will no longer appear in search or package manager UIs
5. Create a new patch version with the fix

## Checking Current Version

The version is calculated from git tags by MinVer:

```bash
# See the current calculated version in build output
dotnet build CDS.SQLiteLogging/CDS.SQLiteLogging.csproj --verbosity normal
```

## Troubleshooting

### Build fails on release
- Check the GitHub Actions log
- Ensure all dependencies are properly referenced
- Verify the project builds locally: `dotnet build CDS.SQLiteLogging/CDS.SQLiteLogging.csproj --configuration Release`

### NuGet push fails
- Verify your API key is correctly set in GitHub secrets
- Check if the version already exists on NuGet.org
- Ensure the API key has "Push" permissions

### Version is not what you expected
- MinVer calculates version from git tags
- Ensure you've fetched all tags: `git fetch --tags`
- Check tag format follows `V{major}.{minor}.{patch}` (uppercase `V`)
- View all tags: `git tag -l`

### Tests fail during release
- Fix the tests first!
- Never bypass failing tests to create a release
- If urgent, fix and create a patch release

## Additional Resources

- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [MinVer Documentation](https://github.com/adamralph/minver)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
