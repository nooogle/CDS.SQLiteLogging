# CI/CD Setup Complete

The repository automation has been aligned with `CDS.SQLiteLogging`.

## What is configured

### Workflows

- `.github/workflows/ci.yml`
  - Builds `CDS.SQLiteLogging` on Ubuntu, Windows, and macOS
  - Runs `CDS.SQLiteLogging.Tests`
  - Uploads `.trx` test results
  - Uploads Cobertura coverage from Ubuntu
  - Builds Windows-specific projects on Windows

- `.github/workflows/release.yml`
  - Runs for tags like `v2.5.0`
  - Builds and tests the package
  - Packs `CDS.SQLiteLogging`
  - Creates a GitHub release
  - Pushes the NuGet package to NuGet.org

- `.github/workflows/codeql.yml`
  - Restores and builds all projects on Windows
  - Runs CodeQL security analysis for C#

### Templates and docs

- `.github/ISSUE_TEMPLATE/*`
- `.github/pull_request_template.md`
- `.github/SETUP.md`
- `START_HERE.md`
- `CONTRIBUTING.md`
- `RELEASE.md`

## Required follow-up

### Add the NuGet secret

Add `NUGET_API_KEY` at:

`https://github.com/nooogle/CDS.SQLiteLogging/settings/secrets/actions`

Scope the key for package `CDS.SQLiteLogging`.

### Optional setup

- Add `CODECOV_TOKEN` if you want Codecov uploads
- Enable GitHub Discussions if you want the issue template support link to work

## First validation run

```bash
git add .
git commit -m "Refresh GitHub automation for CDS.SQLiteLogging"
git push origin master
```

Then check:

`https://github.com/nooogle/CDS.SQLiteLogging/actions`

## Release example

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
- Setup guide: `.github/SETUP.md`

## Notes

- The repository includes `CDS.SQLiteLogging.sln`, but the workflows target project files directly so cross-platform jobs avoid Windows-only projects and do not depend on the solution's current Release build mapping.
- Versioning is driven by MinVer and `v`-prefixed Git tags.
- The copied automation docs now reference `CDS.SQLiteLogging` instead of the previous project.
