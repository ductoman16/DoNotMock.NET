using System;

namespace DoNotMock;

/// <summary>
/// Indicates that a type should not be mocked in tests.
/// </summary>
/// <remarks>
/// Use this attribute to mark types that have a better alternative available than 
/// using mocks for testing. This helps enforce better testing practices by 
/// preventing the creation of low-fidelity mocks, which tie unit tests to the 
/// specific implementation being tested.
/// 
/// For more information on the inspiration behind this, see:
/// 1) https://testing.googleblog.com/2024/02/increase-test-fidelity-by-avoiding-mocks.html
/// 2) https://android.googlesource.com/platform/frameworks/support/%2B/refs/heads/androidx-core-core-role-release/docs/do_not_mock.md
/// 3) https://www.baeldung.com/mockito-annotations#donotmock-annotation
/// </remarks>
/// <param name="message">A message explaining what alternative to use instead of mocking.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class DoNotMockAttribute(string message) : Attribute
{
    /// <summary>
    /// Gets the justification message explaining why this type should not be mocked.
    /// </summary>
    public string Message { get; } = message;
} 