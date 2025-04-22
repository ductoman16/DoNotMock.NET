using FluentAssertions;

namespace DoNotMock.Tests;

public class DoNotMockAttributeTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var attribute = new DoNotMockAttribute();

        // Assert
        attribute.Should().NotBeNull();
    }
}
