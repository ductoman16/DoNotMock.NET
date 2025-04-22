# DoNotMock.NET

A .NET implementation of the `[DoNotMock]` attribute with built-in Roslyn analyzer, inspired by Google's "Error Prone" tool. This attribute helps enforce proper test double usage by marking types that should not be mocked in tests.

## Inspiration

1) <https://testing.googleblog.com/2024/02/increase-test-fidelity-by-avoiding-mocks.html>
2) <https://testing.googleblog.com/2013/05/testing-on-toilet-dont-overuse-mocks.html>
3) <https://android.googlesource.com/platform/frameworks/support/%2B/refs/heads/androidx-core-core-role-release/docs/do_not_mock.md>
4) <https://www.baeldung.com/mockito-annotations#donotmock-annotation>

## Why should you avoid Mocks?

Mocking is a powerful tool for unit testing, but it can cause some issues of its own:

- Mocks maintain **none** of the same behavior as the real code.
- They lock your tests into one particular implementation.

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

```csharp
using DoNotMock;

[DoNotMock("Use NullEmailSender instead")]
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
2. Create a test-specific implementation:

```csharp
public class TestEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> SentEmails { get; } = new();

    public Task SendEmailAsync(string to, string subject, string body)
    {
        SentEmails.Add((to, subject, body));
        return Task.CompletedTask;
    }
}

// In your test:
var emailSender = new TestEmailSender();
await emailSender.SendEmailAsync("test@example.com", "Test", "Hello");
emailSender.SentEmails.Should().ContainSingle()
    .Which.Should().BeEquivalentTo(
        (To: "test@example.com", Subject: "Test", Body: "Hello"));
```

1. Use a null implementation if the dependency is optional:

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
