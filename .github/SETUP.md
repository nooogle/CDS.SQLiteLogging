# GitHub Actions Setup

This repository uses GitHub Actions for CI, release packaging, dependency maintenance, and security scanning.

## Workflows

### CI

Workflow: `.github/workflows/ci.yml`

Triggers:
- Push to `master`, `main`, or `develop`
- Pull requests targeting `master` or `main`
- Manual runs

What it does:
- Builds `CDS.SQLiteLogging` on `ubuntu-latest`, `windows-latest`, and `macos-latest`
- Runs `CDS.SQLiteLogging.Tests` on each platform
- Uploads `.trx` test reports for every OS
- Uploads Cobertura coverage output from Ubuntu
- Builds Windows-specific projects on `windows-latest`
  - `CDS.SQLiteLogging.Views`
  - `WinFormsTest`
  - `ConsoleTest`
  - `Benchmarking`

Status badge:

```markdown
[![CI](https://github.com/nooogle/CDS.SQLiteLogging/actions/workflows/ci.yml/badge.svg)](https://github.com/nooogle/CDS.SQLiteLogging/actions/workflows/ci.yml)
```

### CodeQL

Workflow: `.github/workflows/codeql.yml`

Triggers:
- Push to `master` or `main`
- Pull requests targeting `master` or `main`
- Weekly schedule

What it does:
- Restores and builds all repository projects on `windows-latest`
- Runs GitHub CodeQL analysis for C#

Status badge:

```markdown
[![CodeQL](https://github.com/nooogle/CDS.SQLiteLogging/actions/workflows/codeql.yml/badge.svg)](https://github.com/nooogle/CDS.SQLiteLogging/actions/workflows/codeql.yml)
```

### Release

Workflow: `.github/workflows/release.yml`

Triggers:
- Push of tags matching `v*.*.*`
- Manual runs

What it does:
1. Restores and builds `CDS.SQLiteLogging`
2. Runs `CDS.SQLiteLogging.Tests`
3. Packs the `CDS.SQLiteLogging` NuGet package
4. Creates a GitHub Release for tag builds
5. Pushes the package to NuGet.org for tag builds

## Repository Setup

### Required secret

For NuGet publishing, add `NUGET_API_KEY` to:

`https://github.com/nooogle/CDS.SQLiteLogging/settings/secrets/actions`

### Optional secret

If you want Codecov uploads to succeed, add `CODECOV_TOKEN` in the same repository secrets page.

### Optional discussions

If you want the issue template support link to work, enable GitHub Discussions for:

`https://github.com/nooogle/CDS.SQLiteLogging/settings`

## Release Usage

Tag-based release example:

```bash
git add .
git commit -m "Prepare v2.5.0 release"
git push origin master

git tag v2.5.0
git push origin v2.5.0
```

## Monitoring

- Actions: `https://github.com/nooogle/CDS.SQLiteLogging/actions`
- Releases: `https://github.com/nooogle/CDS.SQLiteLogging/releases`
- NuGet: `https://www.nuget.org/packages/CDS.SQLiteLogging`
- Codecov: `https://codecov.io/gh/nooogle/CDS.SQLiteLogging`

## Notes

- This repository includes `CDS.SQLiteLogging.sln`, but the workflows target specific project files so cross-platform jobs avoid Windows-only projects and do not depend on the solution's current Release build mapping.
- Release versioning is driven by Git tags via MinVer.
- Issue templates and PR templates are tailored for `CDS.SQLiteLogging` and `CDS.SQLiteLogging.Views`.
