# DoNotMock.NET

A .NET implementation of the `[DoNotMock]` attribute with built-in Roslyn analyzer, inspired by Google's "Error Prone" tool. This attribute helps enforce proper test double usage by marking types that should not be mocked in tests.

## Why DoNotMock?

Mocking is a powerful tool for unit testing, but it can be misused. Some types should never be mocked because:

- They have complex invariants that mocks might violate
- They represent value objects or data structures
- They are already designed for testability with test doubles
- Their behavior is too fundamental to mock

The `[DoNotMock]` attribute, combined with the built-in Roslyn analyzer, helps enforce these design decisions by:

1. Marking types that should not be mocked
2. Providing compile-time errors when these types are mocked
3. Encouraging the use of real implementations or hand-written test doubles

## Installation

```shell
dotnet add package DoNotMock
```

The package includes both the attribute and its analyzer.

## Usage

Mark any class or interface that should not be mocked with the `[DoNotMock]` attribute:

TODO: Provide examples

The analyzer will then emit errors if these types are mocked in tests.

## Building from Source

Requirements:

- .NET SDK 8.0 or later

```shell
git clone https://github.com/ductoman16/DoNotMock.NET.git
cd DoNotMock.NET
dotnet build
dotnet test
```

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) before submitting a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
