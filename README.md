# DoNotMock.NET

A .NET implementation of the `[DoNotMock]` attribute with built-in Roslyn analyzer, inspired by Google's "Error Prone" tool. This attribute encourages usage of higher-fidelity test doubles by pointing developers to existing alternatives to mocking particular classes.

## Inspiration

1) <https://testing.googleblog.com/2024/02/increase-test-fidelity-by-avoiding-mocks.html>
2) <https://testing.googleblog.com/2013/05/testing-on-toilet-dont-overuse-mocks.html>
3) <https://android.googlesource.com/platform/frameworks/support/%2B/refs/heads/androidx-core-core-role-release/docs/do_not_mock.md>
4) <https://www.baeldung.com/mockito-annotations#donotmock-annotation>

## Why should you avoid Mocks?

Mocking is a powerful tool for unit testing, but it can cause some issues of its own:

- Mocks maintain **none** of the same behavior as the real code, and can allow bugs to hide in the interactions between your production classes, which don't get exercised during tests.
- They lock your tests into one particular implementation, making refactoring needlessly difficult.

Therefore, tests should use the [highest-fidelity dependencies](https://testing.googleblog.com/2024/02/increase-test-fidelity-by-avoiding-mocks.html) possible:

1) Prefer using the real implementation, because it is the highest fidelity.
2) Use a fake if you can't use the real implementation for logistical reasons.
3) Use a mock if you absolutely can't use the real implementation or a fake.

The `[DoNotMock]` attribute, combined with the built-in Roslyn analyzer, helps enforce these design decisions by:

1. Marking types that have an existing, better alternative to mocking.
2. Pointing developers to those mocking alternatives.
3. Providing compile-time errors when these types are mocked.

For a comprehensive strategy for testing without mocks, see James Shore's [Testing Without Mocks](https://www.jamesshore.com/v2/projects/nullables/testing-without-mocks).

## Installation

```shell
dotnet add package DoNotMock
```

The package includes both the attribute and its analyzer.

## Usage

Mark any class or interface that should not be mocked with the `[DoNotMock]` attribute:

```csharp
using DoNotMock;

[DoNotMock($"Use NullEmailSender instead")]
public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}

// The analyzer will prevent mocking this interface:
var mock = new Mock<IEmailSender>(); // Error DNMK001: Type 'IEmailSender' is marked with [DoNotMock] and should not be mocked
```

### Rule DNMK001: Type marked with [DoNotMock] should not be mocked

This rule is triggered when code attempts to mock a type that is marked with the `[DoNotMock]` attribute. The analyzer detects mock creation and setup patterns from popular mocking frameworks including:

- Moq
- NSubstitute
- FakeItEasy

#### Examples of violations

```csharp
// Using Moq
var mock = new Mock<IEmailSender>();
mock.Setup(x => x.SendEmailAsync(...));

// Using NSubstitute
var mock = Substitute.For<IEmailSender>();
mock.SendEmailAsync(...).Returns(Task.CompletedTask);

// Using FakeItEasy
var mock = A.Fake<IEmailSender>();
A.CallTo(() => mock.SendEmailAsync(...)).Returns(Task.CompletedTask);
```

#### How to fix violations

Instead of mocking, use one of these approaches:

1. Use the real implementation in your tests
2. Create a test-specific fake implementation:

```csharp
public class NullEmailSender : IEmailSender
{
    public static IEmailSender Instance { get; } = new NullEmailSender();
    public Task SendEmailAsync(string to, string subject, string body) => Task.CompletedTask;
}
```

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
