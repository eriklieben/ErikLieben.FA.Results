using System;
using System.Collections.Generic;
using System.Linq;
using ErikLieben.FA.Results;
using ErikLieben.FA.Results.Validations.Tests;
using ErikLieben.FA.Specifications;
using Xunit;

namespace ErikLieben.FA.Specifications.Tests;

public class SpecificationExtensionsTests
{
    private sealed class PositiveSpec : Specification<int>
    {
        public override bool IsSatisfiedBy(int entity) => entity > 0;
    }

    public class ToSpecification
    {
        [Fact]
        public void Should_create_spec_from_predicate_and_invoke_it()
        {
            // Arrange
            Func<int, bool> predicate = x => x == 42;

            // Act
            var sut = predicate.ToSpecification();
            var result = sut.IsSatisfiedBy(42);

            // Assert
            Assert.True(result);
        }
    }

    public class Filter
    {
        [Fact]
        public void Should_filter_entities_matching_specification()
        {
            // Arrange
            var sut = new PositiveSpec();
            var numbers = new[] { -2, -1, 0, 1, 2 };

            // Act
            var filtered = sut.Filter(numbers).ToArray();

            // Assert
            Assert.Equal(new[] { 1, 2 }, filtered);
        }

        [Fact]
        public void Should_throw_when_specification_is_null()
        {
            // Arrange
            Specification<int> sut = null!;
            var numbers = new[] { 1 };

            // Act
            Func<IEnumerable<int>> act = () => sut.Filter(numbers);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_throw_when_entities_is_null()
        {
            // Arrange
            var sut = new PositiveSpec();

            // Act
            Func<IEnumerable<int>> act = () => sut.Filter(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class ValidateAll
    {
        [Fact]
        public void Should_return_results_for_each_entity()
        {
            // Arrange
            var sut = new PositiveSpec();
            var numbers = new[] { -1, 0, 1 };

            // Act
            Result<int>[] results = sut.ValidateAll(numbers, "must be positive").ToArray();

            // Assert
            Assert.True(results[2].IsSuccess);
            Assert.Equal(1, results[2].Value);
            Assert.True(results[0].IsFailure);
            Assert.Equal("must be positive", results[0].Errors[0].Message);
        }

        [Fact]
        public void Should_throw_when_specification_is_null()
        {
            // Arrange
            Specification<int> sut = null!;
            var numbers = new[] { 1 };

            // Act
            Func<IEnumerable<Result<int>>> act = () => sut.ValidateAll(numbers, "err");

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_throw_when_entities_is_null()
        {
            // Arrange
            var sut = new PositiveSpec();

            // Act
            Func<IEnumerable<Result<int>>> act = () => sut.ValidateAll(null!, "err");

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_throw_when_error_message_is_null()
        {
            // Arrange
            var sut = new PositiveSpec();
            var numbers = new[] { 1 };

            // Act
            Func<IEnumerable<Result<int>>> act = () => sut.ValidateAll(numbers, null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class Validate
    {
        [Fact]
        public void Should_return_success_when_satisfied()
        {
            // Arrange
            var sut = new EvenSpec();

            // Act
            Result<int> result = sut.Validate(4, "must be even");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(4, result.Value);
        }

        [Fact]
        public void Should_return_failure_with_message_when_not_satisfied()
        {
            // Arrange
            var sut = new EvenSpec();

            // Act
            Result<int> result = sut.Validate(3, "must be even");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("must be even", result.Errors[0].Message);
            Assert.Equal(string.Empty, result.Errors[0].PropertyName);
        }
    }
}
