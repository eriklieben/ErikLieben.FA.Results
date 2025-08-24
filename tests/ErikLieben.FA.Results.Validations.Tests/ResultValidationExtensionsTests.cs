using System;
using ErikLieben.FA.Results;
using ErikLieben.FA.Results.Validations;
using ErikLieben.FA.Specifications;
using Xunit;

namespace ErikLieben.FA.Results.Validations.Tests;

public class ResultValidationExtensionsTests
{
    public sealed class IsEvenSpec : Specification<int>
    {
        public override bool IsSatisfiedBy(int entity) => entity % 2 == 0;
    }

    public class ValidateWith
    {
        [Fact]
        public void Should_not_add_error_when_spec_satisfied()
        {
            // Arrange
            var sut = Result<int>.Success(4);

            // Act
            var validated = sut.ValidateWith<int, IsEvenSpec>("Must be even", "Value");

            // Assert
            Assert.True(validated.IsSuccess);
            Assert.Equal(4, validated.Value);
            Assert.Equal(0, validated.Errors.Length);
        }

        [Fact]
        public void Should_accumulate_error_when_spec_not_satisfied()
        {
            // Arrange
            var sut = Result<int>.Success(3);

            // Act
            var validated = sut.ValidateWith<int, IsEvenSpec>("Must be even", "Value");

            // Assert
            Assert.True(validated.IsFailure);
            Assert.Equal(1, validated.Errors.Length);
            Assert.Equal("Must be even", validated.Errors[0].Message);
            Assert.Equal("Value", validated.Errors[0].PropertyName);
        }

        [Fact]
        public void Should_return_original_failure_without_running_spec()
        {
            // Arrange
            var original = Result<int>.Failure(ValidationError.Create("err1"));

            // Act
            var validated = original.ValidateWith<int, IsEvenSpec>("Must be even", "Value");

            // Assert
            Assert.True(validated.IsFailure);
            Assert.Equal(1, validated.Errors.Length);
            Assert.Equal("err1", validated.Errors[0].Message);
        }

        [Fact]
        public void Should_not_add_error_when_predicate_true()
        {
            // Arrange
            var sut = Result<string>.Success("abc");

            // Act
            var validated = sut.ValidateWith(s => s.Length == 3, "Bad", "Name");

            // Assert
            Assert.True(validated.IsSuccess);
            Assert.Equal("abc", validated.Value);
            Assert.Equal(0, validated.Errors.Length);
        }

        [Fact]
        public void Should_add_error_when_predicate_false()
        {
            // Arrange
            var sut = Result<string>.Success("a");

            // Act
            var validated = sut.ValidateWith(s => s.Length == 3, "Bad", "Name");

            // Assert
            Assert.True(validated.IsFailure);
            Assert.Equal(1, validated.Errors.Length);
            Assert.Equal("Bad", validated.Errors[0].Message);
            Assert.Equal("Name", validated.Errors[0].PropertyName);
        }

        [Fact]
        public void Should_throw_when_condition_null()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act
            Func<Result<int>> act = () => sut.ValidateWith(null!, "msg");

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class ValidateAndMap
    {
        [Fact]
        public void Should_throw_when_validator_null()
        {
            // Arrange
            var sut = Result<int>.Success(2);

            // Act
            Func<Result<string>> act = () => sut.ValidateAndMap<int, string>(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_propagate_errors_when_input_is_failure()
        {
            // Arrange
            var sut = Result<int>.Failure(ValidationError.Create("e1"));

            // Act
            var mapped = sut.ValidateAndMap<int, string>(_ => Result<string>.Success("ok"));

            // Assert
            Assert.True(mapped.IsFailure);
            Assert.Equal(1, mapped.Errors.Length);
            Assert.Equal("e1", mapped.Errors[0].Message);
        }

        [Fact]
        public void Should_combine_errors_from_validator()
        {
            // Arrange
            var sut = Result<int>.Success(5);

            // Act
            var mapped = sut.ValidateAndMap<int, string>(_ => Result<string>.Failure([
                new ValidationError("bad1", "A"),
                new ValidationError("bad2", "B")
            ]));

            // Assert
            Assert.True(mapped.IsFailure);
            Assert.Equal(2, mapped.Errors.Length);
            Assert.Equal("bad1", mapped.Errors[0].Message);
            Assert.Equal("bad2", mapped.Errors[1].Message);
        }

        [Fact]
        public void Should_return_transformed_value_when_validator_succeeds()
        {
            // Arrange
            var sut = Result<int>.Success(3);

            // Act
            var mapped = sut.ValidateAndMap<int, string>(x => Result<string>.Success((x * 2).ToString()));

            // Assert
            Assert.True(mapped.IsSuccess);
            Assert.Equal("6", mapped.Value);
        }
    }

    public class ThrowIfFailure
    {
        [Fact]
        public void Should_return_value_when_success()
        {
            // Arrange
            var sut = Result<string>.Success("ok");

            // Act
            var value = sut.ThrowIfFailure();

            // Assert
            Assert.Equal("ok", value);
        }

        [Fact]
        public void Should_throw_validation_exception_with_errors_when_failure()
        {
            // Arrange
            var sut = Result<int>.Failure([
                new ValidationError("errA", "P1"),
                new ValidationError("errB", "P2")
            ]);

            // Act
            var ex = Assert.Throws<ValidationException>(() => sut.ThrowIfFailure());

            // Assert
            Assert.Equal(2, ex.Errors.Length);
            Assert.Equal("P1", ex.Errors[0].PropertyName);
            Assert.Contains("Validation failed with 2 errors", ex.Message);
        }

        [Fact]
        public void Should_throw_custom_exception_when_factory_provided()
        {
            // Arrange
            var sut = Result<int>.Failure(ValidationError.Create("x"));

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() =>
                sut.ThrowIfFailure(errors => new InvalidOperationException($"{errors.Length} errs")));

            // Assert
            Assert.Equal("1 errs", ex.Message);
        }
    }
}
