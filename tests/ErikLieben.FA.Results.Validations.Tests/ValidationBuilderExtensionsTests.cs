using System;
using ErikLieben.FA.Results;
using ErikLieben.FA.Results.Validations;
using ErikLieben.FA.Specifications;
using Xunit;

namespace ErikLieben.FA.Results.Validations.Tests;

public class ValidationBuilderExtensionsTests
{
    public sealed class IsShort : Specification<string>
    {
        public override bool IsSatisfiedBy(string entity) => entity.Length <= 5;
    }

    public class For
    {
        [Fact]
        public void Should_create_builder_for_type()
        {
            // Arrange

            // Act
            var sut = ValidationBuilder.For<int>();

            // Assert
            Assert.NotNull(sut);
            Assert.False(sut.HasErrors);
        }

        [Fact]
        public void Should_create_builder_for_type_with_value()
        {
            // Arrange

            // Act
            var sut = ValidationBuilder.For("value");

            // Assert
            Assert.NotNull(sut);
        }
    }

    public class ValidateSingle
    {
        [Fact]
        public void Should_return_success_when_spec_satisfied()
        {
            // Arrange
            var input = "abc";

            // Act
            var res = ValidationBuilder.ValidateSingle<string, IsShort>(input, "too long", "Name");

            // Assert
            Assert.True(res.IsSuccess);
            Assert.Equal("abc", res.Value);
        }

        [Fact]
        public void Should_return_failure_when_spec_not_satisfied()
        {
            // Arrange
            var input = "abcdef";

            // Act
            var res = ValidationBuilder.ValidateSingle<string, IsShort>(input, "too long", "Name");

            // Assert
            Assert.True(res.IsFailure);
            Assert.Equal(1, res.Errors.Length);
            Assert.Equal("too long", res.Errors[0].Message);
            Assert.Equal("Name", res.Errors[0].PropertyName);
        }
    }

    public class PrimitiveValidators
    {
        [Fact]
        public void Should_validate_not_null_reference()
        {
            // Arrange
            string value = "x";

            // Act
            var res = ValidationBuilder.ValidateNotNull(value, "req", "P");

            // Assert
            Assert.True(res.IsSuccess);
        }

        [Fact]
        public void Should_fail_when_null_reference()
        {
            // Arrange
            string? value = null;

            // Act
            var res = ValidationBuilder.ValidateNotNull(value, "req", "P");

            // Assert
            Assert.True(res.IsFailure);
            Assert.Equal("req", res.Errors[0].Message);
            Assert.Equal("P", res.Errors[0].PropertyName);
        }

        [Fact]
        public void Should_validate_not_null_or_empty()
        {
            // Arrange
            string? value = "hi";

            // Act
            var res = ValidationBuilder.ValidateNotNullOrEmpty(value, "req", "P");

            // Assert
            Assert.True(res.IsSuccess);
        }

        [Fact]
        public void Should_fail_not_null_or_empty()
        {
            // Arrange
            string? value = "";

            // Act
            var res = ValidationBuilder.ValidateNotNullOrEmpty(value, "req", "P");

            // Assert
            Assert.True(res.IsFailure);
            Assert.Equal("req", res.Errors[0].Message);
            Assert.Equal("P", res.Errors[0].PropertyName);
        }

        [Fact]
        public void Should_validate_not_null_or_whitespace()
        {
            // Arrange
            string? value = " x ";

            // Act
            var res = ValidationBuilder.ValidateNotNullOrWhiteSpace(value, "req", "P");

            // Assert
            Assert.True(res.IsSuccess);
        }

        [Fact]
        public void Should_fail_not_null_or_whitespace()
        {
            // Arrange
            string? value = "  ";

            // Act
            var res = ValidationBuilder.ValidateNotNullOrWhiteSpace(value, "req", "P");

            // Assert
            Assert.True(res.IsFailure);
            Assert.Equal("req", res.Errors[0].Message);
            Assert.Equal("P", res.Errors[0].PropertyName);
        }

        [Fact]
        public void Should_validate_range_inclusive()
        {
            // Arrange
            var value = 5;

            // Act
            var res = ValidationBuilder.ValidateRange(value, 1, 10, "out", "P");

            // Assert
            Assert.True(res.IsSuccess);
        }

        [Fact]
        public void Should_fail_when_out_of_range()
        {
            // Arrange
            var value = 0;

            // Act
            var res = ValidationBuilder.ValidateRange(value, 1, 10, "out", "P");

            // Assert
            Assert.True(res.IsFailure);
            Assert.Equal("out", res.Errors[0].Message);
            Assert.Equal("P", res.Errors[0].PropertyName);
        }
    }
}
