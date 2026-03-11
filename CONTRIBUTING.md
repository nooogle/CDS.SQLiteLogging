# Contributing to CDS.SQLiteLogging

Thank you for considering contributing to `CDS.SQLiteLogging`. This document outlines the basic process for contributing to the repository.

## Code of Conduct

Be respectful, constructive, and professional in all interactions.

## How Can I Contribute?

### Reporting Bugs

- Use the GitHub issue tracker
- Check if the issue already exists
- Include a clear title and description
- Provide code samples and expected vs. actual behavior
- Mention your .NET version and OS

### Suggesting Features

- Use the GitHub issue tracker with the "enhancement" label
- Explain the use case and why it would be valuable
- Keep the scope focused on SQLite-backed logging, export, diagnostics, or log viewing

### Pull Requests

1. **Fork the repository** and create your branch from `master`
2. **Follow the coding guidelines** (see below)
3. **Add tests** for any new functionality
4. **Ensure all tests pass** using the test project command shown below
5. **Update documentation** in README.md if needed
6. **Update workflow or setup docs** if your change affects automation or contributor guidance
7. **Submit a pull request**

## Development Setup

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/CDS.SQLiteLogging.git
cd CDS.SQLiteLogging

# Restore the library and tests
dotnet restore CDS.SQLiteLogging/CDS.SQLiteLogging.csproj
dotnet restore CDS.SQLiteLogging.Tests/CDS.SQLiteLogging.Tests.csproj

# Build the main package project
dotnet build CDS.SQLiteLogging/CDS.SQLiteLogging.csproj --configuration Release

# Run tests
dotnet test --project CDS.SQLiteLogging.Tests/CDS.SQLiteLogging.Tests.csproj --configuration Release

# Run console test app
dotnet run --project ConsoleTest/ConsoleTest.csproj
```

## Coding Guidelines

This project follows the guidelines in `.github/copilot-instructions.md`:

### Style
- Use Allman braces (opening brace on new line)
- File-scoped namespaces (`namespace X;`)
- Use `var` when type is obvious
- Enable nullable reference types
- PascalCase for public members
- `_camelCase` for private fields
- XML documentation for all public APIs

### Quality
- Write meaningful XML documentation comments
- Add inline comments for complex logic
- Validate inputs with guard clauses
- Use `async`/`await` for I/O operations
- Use parameterized queries and safe persistence practices

### Testing
- Follow the existing MSTest and assertion patterns already used in `CDS.SQLiteLogging.Tests`
- Follow Arrange-Act-Assert pattern
- Use descriptive test names
- Use `[TestCategory]` to organize tests
- Test both positive and negative scenarios

## Versioning

This project uses [MinVer](https://github.com/adamralph/minver) for automatic semantic versioning based on git tags:

- Version is derived from git tags (for example `v2.5.0`)
- Tag format: `v{major}.{minor}.{patch}`
- Maintainers create tags for releases

## Release Process

1. Update any relevant docs such as `README.md` or automation guidance when needed
2. Create and push a version tag: `git tag v2.5.0 && git push origin v2.5.0`
3. GitHub Actions automatically:
   - Builds and tests the package project
   - Creates a GitHub release with generated notes
   - Publishes the NuGet package

## Questions?

Open an issue or start a discussion.
