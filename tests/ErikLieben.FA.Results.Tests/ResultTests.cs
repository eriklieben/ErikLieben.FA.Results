using System;
using ErikLieben.FA.Results;
using Xunit;

namespace ErikLieben.FA.Results.Tests;

public class ResultTests
{
    private static ValidationError Err(string msg = "err", string? prop = null) => new(msg, prop);

    public class Success
    {
        [Fact]
        public void Should_create_success()
        {
            // Arrange

            // Act
            var sut = Result.Success();

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.False(sut.IsFailure);
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
            var sut = Result.Failure(error);

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal("boom", sut.Errors[0].Message);
        }

        [Fact]
        public void Should_create_failure_with_multiple_errors()
        {
            // Arrange
            var errors = new[] { Err("a"), Err("b") };

            // Act
            var sut = Result.Failure(errors);

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(2, sut.Errors.Length);
        }

        [Fact]
        public void Should_create_failure_with_message()
        {
            // Arrange
            // Act
            var sut = Result.Failure("bad");

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal("bad", sut.Errors[0].Message);
        }
    }

    public class Map
    {
        [Fact]
        public void Should_map_to_generic_success()
        {
            // Arrange
            var sut = Result.Success();

            // Act
            var mapped = sut.Map(123);

            // Assert
            Assert.True(mapped.IsSuccess);
            Assert.Equal(123, mapped.Value);
        }

        [Fact]
        public void Should_map_to_generic_failure()
        {
            // Arrange
            var sut = Result.Failure(Err("e1"));

            // Act
            var mapped = sut.Map(1);

            // Assert
            Assert.True(mapped.IsFailure);
            Assert.Equal("e1", mapped.Errors[0].Message);
        }
    }

    public class Bind
    {
        [Fact]
        public void Should_bind_non_generic_success_case()
        {
            // Arrange
            var sut = Result.Success();

            // Act
            var bound = sut.Bind(() => Result.Success());

            // Assert
            Assert.True(bound.IsSuccess);
        }

        [Fact]
        public void Should_skip_binder_when_failure_for_non_generic()
        {
            // Arrange
            var sut = Result.Failure(Err("no"));

            // Act
            var bound = sut.Bind(() => Result.Success());

            // Assert
            Assert.True(bound.IsFailure);
            Assert.Equal("no", bound.Errors[0].Message);
        }

        [Fact]
        public void Should_throw_when_non_generic_binder_is_null()
        {
            // Arrange
            var sut = Result.Success();

            // Act
            var act = () => sut.Bind(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }

        [Fact]
        public void Should_bind_generic_success_case()
        {
            // Arrange
            var sut = Result.Success();

            // Act
            var bound = sut.Bind(() => Result<int>.Success(7));

            // Assert
            Assert.True(bound.IsSuccess);
            Assert.Equal(7, bound.Value);
        }

        [Fact]
        public void Should_skip_generic_binder_when_failure()
        {
            // Arrange
            var sut = Result.Failure(Err("nope"));

            // Act
            var bound = sut.Bind(() => Result<int>.Success(1));

            // Assert
            Assert.True(bound.IsFailure);
            Assert.Equal("nope", bound.Errors[0].Message);
        }

        [Fact]
        public void Should_throw_when_generic_binder_is_null()
        {
            // Arrange
            var sut = Result.Success();

            // Act
            var act = () => sut.Bind<int>(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class Switch
    {
        [Fact]
        public void Should_wrap_function_result_in_success()
        {
            // Arrange
            var f = Result.Switch<int, string>(x => (x * 2).ToString());

            // Act
            var sut = f(5);

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal("10", sut.Value);
        }

        [Fact]
        public void Should_throw_when_function_null()
        {
            // Arrange
            // Act
            var act = () => Result.Switch<int, string>(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }
    }

    public class Try
    {
        [Fact]
        public void Should_return_success_when_no_exception()
        {
            // Arrange
            var f = Result.Try<int, string>(x => (x + 1).ToString(), ex => Err(ex.Message));

            // Act
            var sut = f(1);

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal("2", sut.Value);
        }

        [Fact]
        public void Should_return_failure_when_exception_is_thrown()
        {
            // Arrange
            var f = Result.Try<int, string>(_ => throw new InvalidOperationException("bad"), ex => Err(ex.Message));

            // Act
            var sut = f(0);

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal("bad", sut.Errors[0].Message);
        }

        [Fact]
        public void Should_throw_when_try_inputs_are_null()
        {
            // Arrange
            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => Result.Try<int, string>(null!, ex => Err("x")));
            Assert.Throws<ArgumentNullException>(() => Result.Try<int, string>(x => x.ToString(), null!));
        }
    }

    public class Compose
    {
        [Fact]
        public void Should_compose_two_result_functions()
        {
            // Arrange
            Result<int> f1(int x) => x > 0 ? Result<int>.Success(x * 2) : Result<int>.Failure(Err("neg"));
            Result<string> f2(int x) => x % 2 == 0 ? Result<string>.Success($"{x}") : Result<string>.Failure(Err("odd"));

            var composed = Result.Compose<int, int, string>(f1, f2);

            // Act
            var sut = composed(2);

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal("4", sut.Value);
        }

        [Fact]
        public void Should_propagate_failure_from_first_function()
        {
            // Arrange
            Result<int> f1(int x) => Result<int>.Failure(Err("bad1"));
            Result<string> f2(int x) => Result<string>.Success("ok");
            var composed = Result.Compose<int, int, string>(f1, f2);

            // Act
            var sut = composed(0);

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal("bad1", sut.Errors[0].Message);
        }

        [Fact]
        public void Should_throw_when_functions_are_null()
        {
            // Arrange
            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => Result.Compose<int, int, string>(null!, x => Result<string>.Success("")));
            Assert.Throws<ArgumentNullException>(() => Result.Compose<int, int, string>(x => Result<int>.Success(1), null!));
        }
    }

    public class Apply
    {
        [Fact]
        public void Should_apply_when_both_success()
        {
            // Arrange
            var rf = Result<Func<int, string>>.Success(x => (x * 3).ToString());
            var rx = Result<int>.Success(3);

            // Act
            var sut = Result.Apply(rf, rx);

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal("9", sut.Value);
        }

        [Fact]
        public void Should_accumulate_errors_when_failures()
        {
            // Arrange
            var rf = Result<Func<int, string>>.Failure(new[] { Err("e1"), Err("e2") });
            var rx = Result<int>.Failure(new[] { Err("e3") });

            // Act
            var sut = Result.Apply(rf, rx);

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(3, sut.Errors.Length);
        }
    }

    public class Lift
    {
        [Fact]
        public void Should_lift_two_args_when_both_success()
        {
            // Arrange
            var sut = Result.Lift<int, int, string>((a, b) => (a + b).ToString(), Result<int>.Success(1), Result<int>.Success(2));

            // Act + Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal("3", sut.Value);
        }

        [Fact]
        public void Should_accumulate_errors_in_two_arg_lift()
        {
            // Arrange
            var sut = Result.Lift<int, int, string>((a, b) => "x", Result<int>.Failure(Err("a")), Result<int>.Failure(Err("b")));

            // Act + Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(2, sut.Errors.Length);
        }

        [Fact]
        public void Should_lift_three_args_when_all_success()
        {
            // Arrange
            var sut = Result.Lift<int, int, int, string>((a, b, c) => (a + b + c).ToString(), Result<int>.Success(1), Result<int>.Success(2), Result<int>.Success(3));

            // Act + Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal("6", sut.Value);
        }

        [Fact]
        public void Should_accumulate_errors_in_three_arg_lift()
        {
            // Arrange
            var sut = Result.Lift<int, int, int, string>((a, b, c) => "x",
                Result<int>.Failure(Err("a")), Result<int>.Failure(Err("b")), Result<int>.Failure(Err("c")));

            // Act + Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(3, sut.Errors.Length);
        }

        [Fact]
        public void Should_throw_when_function_is_null()
        {
            // Arrange
            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => Result.Lift<int, int, string>(null!, Result<int>.Success(1), Result<int>.Success(2)));
            Assert.Throws<ArgumentNullException>(() => Result.Lift<int, int, int, string>(null!, Result<int>.Success(1), Result<int>.Success(2), Result<int>.Success(3)));
        }
    }
}
