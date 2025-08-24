using System;
using System.Linq;
using ErikLieben.FA.Results;
using ErikLieben.FA.Results.Validations;
using ErikLieben.FA.Specifications;
using Xunit;

namespace ErikLieben.FA.Results.Validations.Tests;

public class SpecificationValidationExtensionsTests
{
    public sealed class IsPositive : Specification<int>
    {
        public override bool IsSatisfiedBy(int entity) => entity > 0;
    }

    public class ValidateResult
    {
        [Fact]
        public void Should_throw_when_specification_null()
        {
            // Arrange
            Specification<int> sut = null!;

            // Act
            Func<Result<int>> act = () => sut.ValidateResult(1, "msg");

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_return_success_when_satisfied()
        {
            // Arrange
            var sut = new IsPositive();

            // Act
            var res = sut.ValidateResult(5, "bad", "P");

            // Assert
            Assert.True(res.IsSuccess);
            Assert.Equal(5, res.Value);
            Assert.Equal(0, res.Errors.Length);
        }

        [Fact]
        public void Should_return_failure_when_not_satisfied_and_set_property_name()
        {
            // Arrange
            var sut = new IsPositive();

            // Act
            var res = sut.ValidateResult(0, "bad", "P");

            // Assert
            Assert.True(res.IsFailure);
            Assert.Equal(1, res.Errors.Length);
            Assert.Equal("bad", res.Errors[0].Message);
            Assert.Equal("P", res.Errors[0].PropertyName);
        }
    }

    public class ValidateMany
    {
        [Fact]
        public void Should_throw_when_specification_null()
        {
            // Arrange
            Specification<int> sut = null!;

            // Act
            Func<System.Collections.Generic.IEnumerable<Result<int>>> act = () => sut.ValidateMany(new[] {1}, "msg");

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_throw_when_values_null()
        {
            // Arrange
            var sut = new IsPositive();

            // Act
            Func<System.Collections.Generic.IEnumerable<Result<int>>> act = () => sut.ValidateMany(null!, "msg");

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_validate_each_value_and_set_indexed_property_names()
        {
            // Arrange
            var sut = new IsPositive();
            var values = new[] { -1, 0, 2 };

            // Act
            var results = sut.ValidateMany(values, "not positive").ToArray();

            // Assert
            Assert.Equal(3, results.Length);
            Assert.True(results[2].IsSuccess);
            Assert.True(results[0].IsFailure);
            Assert.Equal("[0]", results[0].Errors[0].PropertyName);
            Assert.Equal("[1]", results[1].Errors[0].PropertyName);
        }
    }

    public class ValidateAll
    {
        [Fact]
        public void Should_throw_when_values_null()
        {
            // Arrange
            var sut = new IsPositive();

            // Act
            Func<Result<int[]>> act = () => sut.ValidateAll(null!, "msg");

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_aggregate_errors_when_any_fail()
        {
            // Arrange
            var sut = new IsPositive();

            // Act
            var result = sut.ValidateAll(new[] { 1, -1, 2 }, "bad");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(1, result.Errors.Length);
            Assert.Equal("bad", result.Errors[0].Message);
        }

        [Fact]
        public void Should_return_all_values_when_all_succeed()
        {
            // Arrange
            var sut = new IsPositive();

            // Act
            var result = sut.ValidateAll(new[] { 1, 2, 3 }, "bad");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { 1, 2, 3 }, result.Value);
        }
    }
}
