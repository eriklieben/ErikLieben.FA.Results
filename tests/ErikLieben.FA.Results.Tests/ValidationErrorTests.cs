using System;
using ErikLieben.FA.Results;
using Xunit;

namespace ErikLieben.FA.Results.Tests;

public class ValidationErrorTests
{
    public class Create
    {
        [Fact]
        public void Should_create_with_message()
        {
            // Arrange
            var message = "Oops";

            // Act
            var sut = ValidationError.Create(message);

            // Assert
            Assert.Equal(message, sut.Message);
            Assert.Null(sut.PropertyName);
        }

        [Fact]
        public void Should_create_with_message_and_property()
        {
            // Arrange
            var message = "Invalid";
            var property = "Name";

            // Act
            var sut = ValidationError.Create(message, property);

            // Assert
            Assert.Equal(message, sut.Message);
            Assert.Equal(property, sut.PropertyName);
        }
    }

    public class ToStringOverride
    {
        [Fact]
        public void Should_return_message_when_no_property()
        {
            // Arrange
            var sut = ValidationError.Create("Error only");

            // Act
            var result = sut.ToString();

            // Assert
            Assert.Equal("Error only", result);
        }

        [Fact]
        public void Should_return_property_and_message_when_property_present()
        {
            // Arrange
            var sut = ValidationError.Create("Bad", "Field");

            // Act
            var result = sut.ToString();

            // Assert
            Assert.Equal("Field: Bad", result);
        }
    }

    public class Ctor
    {
        [Fact]
        public void Should_throw_when_message_is_null()
        {
            // Arrange
            string? message = null;

            // Act
            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationError(message!, null));
        }
    }
}
