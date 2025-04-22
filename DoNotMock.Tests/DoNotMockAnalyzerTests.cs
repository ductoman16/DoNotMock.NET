using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Moq;
using NSubstitute;
using FakeItEasy;

namespace DoNotMock.Tests;

/// <summary>
/// Tests for the <see cref="DoNotMockAnalyzer"/> class.
/// </summary>
public class DoNotMockAnalyzerTests
{
    /// <summary>
    /// Tests that the analyzer reports a diagnostic when a type marked with [DoNotMock] is mocked using Moq.
    /// </summary>
    [Fact]
    public async Task Analyze_WhenMoqMockUsedOnDoNotMockType_ReportsDiagnostic()
    {
        const string source = """
            using Moq;
            using DoNotMock;

            namespace TestNamespace
            {
                [DoNotMock("Use TestDouble instead")]
                public interface IService
                {
                    void DoSomething();
                }

                public class Tests
                {
                    public void TestMethod()
                    {
                        var mock = new Mock<IService>();
                        mock.Setup(x => x.DoSomething());
                    }
                }
            }
            """;

        var expected = new[]
        {
            new DiagnosticResult("DNMK001", DiagnosticSeverity.Error)
                .WithSpan(16, 24, 16, 44)
                .WithArguments("TestNamespace.IService")
        };

        await VerifyAnalyzerAsync(source, expected).ConfigureAwait(true);
    }

    /// <summary>
    /// Tests that the analyzer reports a diagnostic when a type marked with [DoNotMock] is mocked using NSubstitute.
    /// </summary>
    [Fact]
    public async Task Analyze_WhenNSubstituteUsedOnDoNotMockType_ReportsDiagnostic()
    {
        const string source = """
            using NSubstitute;
            using DoNotMock;

            namespace TestNamespace
            {
                [DoNotMock("Use TestDouble instead")]
                public interface IService
                {
                    string GetValue();
                }

                public class Tests
                {
                    public void TestMethod()
                    {
                        var mock = Substitute.For<IService>();
                        mock.GetValue().Returns("test");
                    }
                }
            }
            """;

        var expected = new[]
        {
            new DiagnosticResult("DNMK001", DiagnosticSeverity.Error)
                .WithSpan(16, 24, 16, 50)
                .WithArguments("TestNamespace.IService")
        };

        await VerifyAnalyzerAsync(source, expected).ConfigureAwait(true);
    }

    /// <summary>
    /// Tests that the analyzer reports a diagnostic when a type marked with [DoNotMock] is mocked using FakeItEasy.
    /// </summary>
    [Fact]
    public async Task Analyze_WhenFakeItEasyUsedOnDoNotMockType_ReportsDiagnostic()
    {
        const string source = """
            using FakeItEasy;
            using DoNotMock;

            namespace TestNamespace
            {
                [DoNotMock("Use TestDouble instead")]
                public interface IService
                {
                    void DoSomething();
                }

                public class Tests
                {
                    public void TestMethod()
                    {
                        var fake = A.Fake<IService>();
                        A.CallTo(() => fake.DoSomething()).DoesNothing();
                    }
                }
            }
            """;

        var expected = new[]
        {
            new DiagnosticResult("DNMK001", DiagnosticSeverity.Error)
                .WithSpan(16, 24, 16, 42)
                .WithArguments("TestNamespace.IService")
        };

        await VerifyAnalyzerAsync(source, expected).ConfigureAwait(true);
    }

    /// <summary>
    /// Tests that the analyzer does not report a diagnostic when a type without [DoNotMock] is mocked.
    /// </summary>
    [Fact]
    public async Task Analyze_WhenTypeHasNoDoNotMockAttribute_NoDiagnosticReported()
    {
        const string source = """
            using Moq;

            namespace TestNamespace
            {
                public interface IService
                {
                    void DoSomething();
                }

                public class Tests
                {
                    public void TestMethod()
                    {
                        var mock = new Mock<IService>();
                        mock.Setup(x => x.DoSomething());
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(source).ConfigureAwait(true);
    }

    /// <summary>
    /// Tests that the analyzer reports a diagnostic when a type inherits from a type marked with [DoNotMock].
    /// </summary>
    [Fact]
    public async Task Analyze_WhenInheritedTypeHasDoNotMockAttribute_ReportsDiagnostic()
    {
        const string source = """
            using Moq;
            using DoNotMock;

            namespace TestNamespace
            {
                [DoNotMock("Use TestDouble instead")]
                public interface IBaseService
                {
                    void DoSomething();
                }

                public interface IService : IBaseService
                {
                    void DoSomethingElse();
                }

                public class Tests
                {
                    public void TestMethod()
                    {
                        var mock = new Mock<IService>();
                        mock.Setup(x => x.DoSomething());
                    }
                }
            }
            """;

        var expected = new[]
        {
            new DiagnosticResult("DNMK001", DiagnosticSeverity.Error)
                .WithSpan(21, 24, 21, 44)
                .WithArguments("TestNamespace.IService")
        };

        await VerifyAnalyzerAsync(source, expected).ConfigureAwait(true);
    }

    private static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<DoNotMockAnalyzer, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                AdditionalReferences =
                {
                    typeof(Mock<>).Assembly,
                    typeof(Substitute).Assembly,
                    typeof(A).Assembly,
                    typeof(DoNotMockAttribute).Assembly
                }
            },
        };

        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }
} 