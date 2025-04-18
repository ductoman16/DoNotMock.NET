# Contributing to DoNotMock.NET

Thank you for your interest in contributing to DoNotMock.NET! This document provides guidelines and instructions for contributing.

## Code of Conduct

This project adheres to the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct). Please read it before contributing.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/DoNotMock.NET.git`
3. Create a branch: `git checkout -b your-feature-name`
4. Make your changes
5. Run tests: `dotnet test`
6. Commit your changes: `git commit -m "Description of changes"`
7. Push to your fork: `git push origin your-feature-name`
8. Create a Pull Request

## Development Guidelines

### Project Structure

- `DoNotMock/` - Core library containing the attribute and analyzer
- `DoNotMock.Tests/` - Unit tests

### Testing

- All new features must include unit tests
- No mocking libraries allowed - use real implementations or hand-written test doubles
- Test names should follow the pattern: `MethodName_ScenarioUnderTest_ExpectedResult`
- Tests should have clear Arrange, Act, and Assert sections
- Use FluentAssertions for assertions

### Code Style

- Use file-scoped namespaces
- Use guard clauses instead of nested conditionals
- Enable and respect all warnings
- Follow standard C# conventions and naming guidelines
- XML comments on all public APIs

### Pull Request Process

1. Update the README.md if your changes affect public APIs
2. Add/update XML comments for any public types or members
3. Ensure all tests pass
4. Request review from maintainers

### Commit Messages

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Reference issues and pull requests when relevant

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

## Need Help?

- Create an issue for bugs or feature requests
- Tag your issue appropriately
- Provide as much context as possible

## License

By contributing to DoNotMock.NET, you agree that your contributions will be licensed under the MIT License.
