using ErikLieben.FA.Results;
using Xunit;

namespace ErikLieben.FA.Results.Tests;

public class ApiErrorTests
{
    public class Ctor
    {
        [Fact]
        public void Should_set_message_and_property()
        {
            // Arrange
            // Act
            var sut = new ApiError("msg", "Prop");

            // Assert
            Assert.Equal("msg", sut.Message);
            Assert.Equal("Prop", sut.PropertyName);
        }

        [Fact]
        public void Should_allow_null_property_name()
        {
            // Arrange
            // Act
            var sut = new ApiError("msg");

            // Assert
            Assert.Equal("msg", sut.Message);
            Assert.Null(sut.PropertyName);
        }
    }
}
