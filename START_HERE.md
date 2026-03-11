# GitHub Automation Quick Start

This repository now has GitHub automation tailored for `CDS.SQLiteLogging`.

## First-Time Setup

### 1. Commit and push the workflow changes

```bash
git add .
git commit -m "Refresh GitHub automation for CDS.SQLiteLogging"
git push origin master
```

### 2. Add the NuGet API key

This is required for tag-based releases.

1. Create or reuse a NuGet API key at `https://www.nuget.org/account/apikeys`
2. Scope it for package `CDS.SQLiteLogging`
3. Add it as repository secret `NUGET_API_KEY` at:
   `https://github.com/nooogle/CDS.SQLiteLogging/settings/secrets/actions`

### 3. Optional repository setup

- Add `CODECOV_TOKEN` if you want Codecov uploads
- Enable GitHub Discussions if you want the issue template support link to work
- Configure branch protection for `master`

## What the workflows do

### CI

`/.github/workflows/ci.yml`

- Builds `CDS.SQLiteLogging` on Ubuntu, Windows, and macOS
- Runs `CDS.SQLiteLogging.Tests` on each OS
- Uploads `.trx` test results
- Uploads Cobertura coverage from Ubuntu
- Builds Windows-only or Windows-focused projects on `windows-latest`

### CodeQL

`/.github/workflows/codeql.yml`

- Restores and builds all projects on Windows
- Runs CodeQL analysis for C#

### Release

`/.github/workflows/release.yml`

- Runs when pushing a tag like `v2.5.0`
- Builds `CDS.SQLiteLogging`
- Runs `CDS.SQLiteLogging.Tests`
- Packs and publishes the `CDS.SQLiteLogging` NuGet package
- Creates a GitHub Release for tag builds

## Creating a release

```bash
git add .
git commit -m "Prepare v2.5.0 release"
git push origin master

git tag v2.5.0
git push origin v2.5.0
```

## Useful links

- Actions: `https://github.com/nooogle/CDS.SQLiteLogging/actions`
- Releases: `https://github.com/nooogle/CDS.SQLiteLogging/releases`
- NuGet: `https://www.nuget.org/packages/CDS.SQLiteLogging`
- Setup details: `.github/SETUP.md`

## Notes

- The repository includes `CDS.SQLiteLogging.sln`, but the workflows still target project files directly so cross-platform jobs avoid Windows-only projects and do not rely on the solution's current Release build mapping.
- MinVer uses Git tags with the `v` prefix.
- The copied workflow and setup references have been aligned with this repository.
