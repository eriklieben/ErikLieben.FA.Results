using System;
using ErikLieben.FA.Results;
using Xunit;

namespace ErikLieben.FA.Results.Tests;

public class ResultOfTTests
{
    private static ValidationError Err(string msg = "err", string? prop = null) => new(msg, prop);

    public class Success
    {
        [Fact]
        public void Should_create_success_with_value()
        {
            // Arrange
            var value = 42;

            // Act
            var sut = Result<int>.Success(value);

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.False(sut.IsFailure);
            Assert.Equal(value, sut.Value);
            Assert.Equal(0, sut.Errors.Length);
        }
    }

    public class Failure
    {
        [Fact]
        public void Should_create_failure_with_single_error()
        {
            // Arrange
            var error = Err("boom");

            // Act
            var sut = Result<int>.Failure(error);

            // Assert
            Assert.True(sut.IsFailure);
            Assert.False(sut.IsSuccess);
            Assert.Equal(1, sut.Errors.Length);
            Assert.Equal("boom", sut.Errors[0].Message);
        }

        [Fact]
        public void Should_create_failure_with_multiple_errors_array()
        {
            // Arrange
            var errors = new[] { Err("a"), Err("b") };

            // Act
            var sut = Result<int>.Failure(errors);

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(2, sut.Errors.Length);
        }

        [Fact]
        public void Should_create_failure_with_span()
        {
            // Arrange
            var errorsArray = new[] { Err("a"), Err("b") };
            ReadOnlySpan<ValidationError> span = errorsArray;

            // Act
            var sut = Result<int>.Failure(span);

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(2, sut.Errors.Length);
        }

        [Fact]
        public void Should_create_failure_with_message_overload()
        {
            // Arrange
            // Act
            var sut = Result<int>.Failure("bad");

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal("bad", sut.Errors[0].Message);
        }

        [Fact]
        public void Should_create_failure_with_message_and_property_name()
        {
            // Arrange
            // Act
            var sut = Result<int>.Failure("bad", "Field");

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal("Field", sut.Errors[0].PropertyName);
        }
    }

    public class Value
    {
        [Fact]
        public void Should_throw_when_accessing_value_on_failure()
        {
            // Arrange
            var sut = Result<int>.Failure(Err("boom"));

            // Act
            var act = () => { var _ = sut.Value; };

            // Assert
            Assert.Throws<InvalidOperationException>(act);
        }
    }

    public class Map
    {
        [Fact]
        public void Should_map_value_when_success()
        {
            // Arrange
            var sut = Result<int>.Success(10);

            // Act
            var mapped = sut.Map(x => x.ToString());

            // Assert
            Assert.True(mapped.IsSuccess);
            Assert.Equal("10", mapped.Value);
        }

        [Fact]
        public void Should_propagate_errors_when_failure()
        {
            // Arrange
            var sut = Result<int>.Failure(Err("e1"));

            // Act
            var mapped = sut.Map(x => x * 2);

            // Assert
            Assert.True(mapped.IsFailure);
            Assert.Equal("e1", mapped.Errors[0].Message);
        }

        [Fact]
        public void Should_throw_when_mapper_is_null()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act
            Func<Result<string>> act = () => sut.Map<string>(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class Bind
    {
        [Fact]
        public void Should_bind_when_success()
        {
            // Arrange
            var sut = Result<int>.Success(2);

            // Act
            var bound = sut.Bind(x => Result<string>.Success((x * 3).ToString()));

            // Assert
            Assert.True(bound.IsSuccess);
            Assert.Equal("6", bound.Value);
        }

        [Fact]
        public void Should_propagate_errors_when_failure()
        {
            // Arrange
            var sut = Result<int>.Failure(Err("no"));

            // Act
            var bound = sut.Bind(x => Result<string>.Success("ok"));

            // Assert
            Assert.True(bound.IsFailure);
            Assert.Equal("no", bound.Errors[0].Message);
        }

        [Fact]
        public void Should_throw_when_binder_null()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act
            Func<Result<string>> act = () => sut.Bind<string>(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class Tap
    {
        [Fact]
        public void Should_invoke_action_when_success()
        {
            // Arrange
            var sut = Result<int>.Success(7);
            var called = 0;

            // Act
            var returned = sut.Tap(x => called = x);

            // Assert
            Assert.Equal(7, called);
            Assert.True(ReferenceEquals(sut, returned));
        }

        [Fact]
        public void Should_not_invoke_action_when_failure()
        {
            // Arrange
            var sut = Result<int>.Failure(Err());
            var called = false;

            // Act
            var returned = sut.Tap(_ => called = true);

            // Assert
            Assert.False(called);
            Assert.True(ReferenceEquals(sut, returned));
        }

        [Fact]
        public void Should_throw_when_action_null()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act
            var act = () => sut.Tap(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class TapError
    {
        [Fact]
        public void Should_invoke_action_when_failure()
        {
            // Arrange
            var sut = Result<int>.Failure(Err("e"));
            ValidationError[] captured = Array.Empty<ValidationError>();

            // Act
            var returned = sut.TapError(errs => captured = errs.ToArray());

            // Assert
            Assert.True(captured.Length == 1 && captured[0].Message == "e");
            Assert.True(ReferenceEquals(sut, returned));
        }

        [Fact]
        public void Should_not_invoke_action_when_success()
        {
            // Arrange
            var sut = Result<int>.Success(1);
            var called = false;

            // Act
            var returned = sut.TapError(_ => called = true);

            // Assert
            Assert.False(called);
            Assert.True(ReferenceEquals(sut, returned));
        }

        [Fact]
        public void Should_throw_when_action_null()
        {
            // Arrange
            var sut = Result<int>.Failure(Err());

            // Act
            var act = () => sut.TapError(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class ValueOrDefault
    {
        [Fact]
        public void Should_return_value_when_success()
        {
            // Arrange
            var sut = Result<int>.Success(5);

            // Act
            var value = sut.ValueOrDefault(10);

            // Assert
            Assert.Equal(5, value);
        }

        [Fact]
        public void Should_return_default_when_failure()
        {
            // Arrange
            var sut = Result<int>.Failure(Err());

            // Act
            var value = sut.ValueOrDefault(10);

            // Assert
            Assert.Equal(10, value);
        }
    }

    public class ValueOr
    {
        [Fact]
        public void Should_return_value_when_success()
        {
            // Arrange
            var sut = Result<int>.Success(3);

            // Act
            var value = sut.ValueOr(_ => 99);

            // Assert
            Assert.Equal(3, value);
        }

        [Fact]
        public void Should_use_factory_when_failure()
        {
            // Arrange
            var sut = Result<int>.Failure(Err("e1"));

            // Act
            var value = sut.ValueOr(errs => errs.Length);

            // Assert
            Assert.Equal(1, value);
        }

        [Fact]
        public void Should_throw_when_factory_is_null()
        {
            // Arrange
            var sut = Result<int>.Failure(Err());

            // Act
            var act = () => sut.ValueOr(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }
}
