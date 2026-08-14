# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Repository-specific GitHub automation for `CDS.SQLiteLogging`
- Project-aware issue templates, PR template, and setup guides
- `Housekeeper(dbPath, options, databaseOptions, dateTimeProvider)` constructor overload so callers can match the journal mode of an already-running writer (e.g. WAL) instead of forcing the library default
- `Housekeeper.CanOpenForWrite(dbPath)` static helper to test, without side effects, whether a database can currently be opened for writing

### Fixed
- Stale references to `CDS.FluentHtmlReports` in copied workflow and repository docs
- `Reader` could fail to open with `SQLite Error 5: 'database is locked'` whenever another process already had the database open (e.g. a live WAL writer). `Reader` now opens the connection read-only and no longer issues the journal-mode/synchronous-mode PRAGMAs, which previously required exclusive access it didn't actually need
- `ConnectionManager` threw when constructed with a bare filename (no directory component), because `Directory.CreateDirectory("")` is invalid; directory creation is now skipped when there's no folder path to create
- Housekeeping's `VACUUM` step could throw `SQLITE_BUSY`/`SQLITE_LOCKED` and abort the cleanup cycle if another process held the database (e.g. a live WAL writer); it's now skipped (with the already-committed deletes retained) instead of failing the whole cycle
- `RELEASE.md`, `CONTRIBUTING.md`, and `.github/SETUP.md` documented lowercase `v*.*.*` release tags, which MinVer does not recognize (`MinVerTagPrefix` is configured as uppercase `V`); examples now match the actual required tag format
