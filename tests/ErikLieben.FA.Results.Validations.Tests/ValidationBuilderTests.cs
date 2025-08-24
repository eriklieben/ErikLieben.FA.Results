using System;
using System.Linq;
using ErikLieben.FA.Results;
using ErikLieben.FA.Results.Validations;
using ErikLieben.FA.Specifications;
using Xunit;

namespace ErikLieben.FA.Results.Validations.Tests;

public class ValidationBuilderTests
{
    private sealed class MinLenSpec : Specification<string>
    {
        private readonly int _min;
        public MinLenSpec(int min) => _min = min;
        public override bool IsSatisfiedBy(string entity) => entity?.Length >= _min;
    }

    private sealed class MinLen5Spec : Specification<string>
    {
        public override bool IsSatisfiedBy(string entity) => entity?.Length >= 5;
    }

    public class ValidateWith
    {
        [Fact]
        public void Should_add_error_when_spec_generic_not_satisfied()
        {
            // Arrange
            var sut = ValidationBuilder.For<object>();

            // Act
            sut.ValidateWith<PassesFalse>(new object(), "fail", "Obj");
            var result = sut.Build(123);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(1, result.Errors.Length);
            Assert.Equal("fail", result.Errors[0].Message);
            Assert.Equal("Obj", result.Errors[0].PropertyName);
        }

        private sealed class PassesFalse : Specification<object>
        {
            public override bool IsSatisfiedBy(object entity) => false;
        }

        [Fact]
        public void Should_add_error_when_typed_spec_not_satisfied()
        {
            // Arrange
            var sut = ValidationBuilder.For<string>();

            // Act
            sut.ValidateWith<MinLen5Spec, string>("abc", "too short", "Name");
            var result = sut.Build("ignored");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("too short", result.Errors[0].Message);
        }

        [Fact]
        public void Should_use_provided_specification_instance()
        {
            // Arrange
            var sut = ValidationBuilder.For<string>();
            var spec = new MinLenSpec(4);

            // Act
            sut.ValidateWith<MinLenSpec, string>("abc", spec, "too short", "Name");
            var result = sut.Build("ignored");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("too short", result.Errors[0].Message);
        }

        [Fact]
        public void Should_keep_no_errors_when_specifications_pass()
        {
            // Arrange
            var sut = ValidationBuilder.For<string>();

            // Act
            sut.ValidateWith<MinLen5Spec, string>("abcdef", "too short", "Name");
            var result = sut.Build("value");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("value", result.Value);
            Assert.Equal(0, result.Errors.Length);
        }
    }

    public class CustomAndPrimitives
    {
        [Fact]
        public void Should_add_error_on_custom_false()
        {
            // Arrange
            var sut = ValidationBuilder.For<int>();

            // Act
            sut.ValidateCustom(false, "custom", "P");
            var result = sut.Build(1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("custom", result.Errors[0].Message);
            Assert.Equal("P", result.Errors[0].PropertyName);
        }

        [Fact]
        public void Should_validate_not_null_and_not_empty_and_whitespace_and_range_and_length_and_collection()
        {
            // Arrange
            var sut = ValidationBuilder.For<string>();

            // Act
            sut
                .ValidateNotNull<string>(null, "req", "S")
                .ValidateNotNullOrEmpty("", "empty", "E")
                .ValidateNotNullOrWhiteSpace(" ", "ws", "W")
                .ValidateRange(0, 1, 2, "range", "R")
                .ValidateStringLength(null, 1, 2, "len", "L")
                .ValidateNotEmpty<int>(Array.Empty<int>(), "coll", "C");
            var result = sut.Build("ignored");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(6, result.Errors.Length);
            Assert.Contains(result.Errors.ToArray(), e => e.Message == "req" && e.PropertyName == "S");
            Assert.Contains(result.Errors.ToArray(), e => e.Message == "empty" && e.PropertyName == "E");
            Assert.Contains(result.Errors.ToArray(), e => e.Message == "ws" && e.PropertyName == "W");
            Assert.Contains(result.Errors.ToArray(), e => e.Message == "range" && e.PropertyName == "R");
            Assert.Contains(result.Errors.ToArray(), e => e.Message == "len" && e.PropertyName == "L");
            Assert.Contains(result.Errors.ToArray(), e => e.Message == "coll" && e.PropertyName == "C");
        }
    }

    public class BuildMethods
    {
        [Fact]
        public void Should_build_success_when_no_errors()
        {
            // Arrange
            var sut = ValidationBuilder.For<int>();

            // Act
            var result = sut.Build(10);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(10, result.Value);
        }

        [Fact]
        public void Should_build_failure_when_has_errors()
        {
            // Arrange
            var sut = ValidationBuilder.For<int>();
            sut.ValidateCustom(false, "bad");

            // Act
            var result = sut.Build(10);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("bad", result.Errors[0].Message);
        }

        [Fact]
        public void Should_build_generic_TResult_from_errors()
        {
            // Arrange
            var sut = ValidationBuilder.For<int>();
            sut.ValidateCustom(false, "bad");

            // Act
            var result = sut.Build("x");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("bad", result.Errors[0].Message);
        }

        [Fact]
        public void Should_build_using_factory_success()
        {
            // Arrange
            var sut = ValidationBuilder.For<int>();

            // Act
            var result = sut.Build(() => 7);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(7, result.Value);
        }

        [Fact]
        public void Should_throw_when_factory_null()
        {
            // Arrange
            var sut = ValidationBuilder.For<int>();

            // Act
            Func<Result<int>> act = () => sut.Build(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_build_non_generic_result()
        {
            // Arrange
            var sut = ValidationBuilder.For<int>();
            sut.ValidateCustom(false, "bad");

            // Act
            var result = sut.Build();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("bad", result.Errors[0].Message);
        }

        [Fact]
        public void Should_expose_errors_and_has_errors_properties()
        {
            // Arrange
            var sut = ValidationBuilder.For<int>();

            // Act
            sut.ValidateCustom(false, "bad1");
            sut.ValidateCustom(false, "bad2");

            // Assert
            Assert.True(sut.HasErrors);
            Assert.Equal(2, sut.Errors.Length);
        }
    }
}
