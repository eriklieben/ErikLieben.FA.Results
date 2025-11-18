using System;
using ErikLieben.FA.Results;
using Xunit;

namespace ErikLieben.FA.Results.Tests;

public class ResultExtensionsTests
{
    private static ValidationError Err(string msg = "err", string? prop = null) => new(msg, prop);

    public class MatchGeneric
    {
        [Fact]
        public void Should_invoke_success_action()
        {
            // Arrange
            var sut = Result<int>.Success(5);
            var successCalled = 0;
            var failureCalled = 0;

            // Act
            var returned = sut.Match(x => successCalled = x, _ => failureCalled++);

            // Assert
            Assert.Equal(5, successCalled);
            Assert.Equal(0, failureCalled);
            Assert.True(ReferenceEquals(sut, returned));
        }

        [Fact]
        public void Should_invoke_failure_action()
        {
            // Arrange
            var sut = Result<int>.Failure(Err("bad"));
            var successCalled = 0;
            var failureLen = -1;

            // Act
            var returned = sut.Match(x => successCalled = x, errs => failureLen = errs.Length);

            // Assert
            Assert.Equal(0, successCalled);
            Assert.Equal(1, failureLen);
            Assert.True(ReferenceEquals(sut, returned));
        }

        [Fact]
        public void Should_throw_when_actions_null()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => sut.Match(null!, _ => { }));
            Assert.Throws<ArgumentNullException>(() => sut.Match(_ => { }, null!));
        }
    }

    public class MatchGenericWithResult
    {
        [Fact]
        public void Should_return_from_success_func()
        {
            // Arrange
            var sut = Result<int>.Success(2);

            // Act
            var value = sut.Match(x => x.ToString(), errs => errs.Length.ToString());

            // Assert
            Assert.Equal(2, value.ValueOrDefault());
        }

        [Fact]
        public void Should_return_from_failure_func()
        {
            // Arrange
            var sut = Result<int>.Failure(Err("e"));

            // Act
            var value = sut.Match(x => x.ToString(), errs => errs.Length.ToString());

            // Assert
            Assert.Equal(1, value.ValueOrDefault());
        }

        [Fact]
        public void Should_throw_when_function_is_null()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act
            Action<int> onSuccessNull = null!;
            Action<ReadOnlySpan<ValidationError>> onFailure = null!;
            var act1 = () => sut.Match(onSuccessNull!, onFailure);

            Action<int> onSuccess = x => { };
            var act2 = () => sut.Match(onSuccess, onFailure!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act1());
            Assert.Throws<ArgumentNullException>(() => act2());
        }
    }

    public class MatchNonGeneric
    {
        [Fact]
        public void Should_invoke_success_action()
        {
            // Arrange
            var sut = Result.Success();
            var successCalled = 0;
            var failureCalled = 0;

            // Act
            var returned = sut.Match(() => successCalled = 1, _ => failureCalled++);

            // Assert
            Assert.Equal(1, successCalled);
            Assert.Equal(0, failureCalled);
            Assert.True(ReferenceEquals(sut, returned));
        }

        [Fact]
        public void Should_invoke_failure_action()
        {
            // Arrange
            var sut = Result.Failure(Err("bad"));
            var successCalled = 0;
            var failureLen = -1;

            // Act
            var returned = sut.Match(() => successCalled = 1, errs => failureLen = errs.Length);

            // Assert
            Assert.Equal(0, successCalled);
            Assert.Equal(1, failureLen);
            Assert.True(ReferenceEquals(sut, returned));
        }

        [Fact]
        public void Should_throw_when_actions_null()
        {
            // Arrange
            var sut = Result.Success();

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => sut.Match(null!, _ => { }));
            Assert.Throws<ArgumentNullException>(() => sut.Match(() => { }, null!));
        }
    }

    public class MatchNonGenericWithResult
    {
        [Fact]
        public void Should_return_from_success_func()
        {
            // Arrange
            var sut = Result.Success();

            // Act
            var value = sut.Match(() => 1, errs => errs.Length);

            // Assert
            Assert.Equal(1, value);
        }

        [Fact]
        public void Should_return_from_failure_func()
        {
            // Arrange
            var sut = Result.Failure(Err("x"));

            // Act
            var value = sut.Match(() => 1, errs => errs.Length);

            // Assert
            Assert.Equal(1, value);
        }

        [Fact]
        public void Should_throw_when_funcs_null()
        {
            // Arrange
            var sut = Result.Success();

            // Act
            Func<int> onSuccessNull = null!;
            Func<ReadOnlySpan<ValidationError>, int> onFailure = _ => 0;
            Func<int> act1 = () => sut.Match(onSuccessNull!, onFailure);

            Func<int> onSuccess = () => 0;
            Func<ReadOnlySpan<ValidationError>, int> onFailureNull = null!;
            Func<int> act2 = () => sut.Match(onSuccess, onFailureNull!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act1());
            Assert.Throws<ArgumentNullException>(() => act2());
        }
    }

    public class MapErrors
    {
        [Fact]
        public void Should_return_same_when_success()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act
            var result = sut.MapErrors(e => new ValidationError("mapped"));

            // Assert
            Assert.True(ReferenceEquals(sut, result));
        }

        [Fact]
        public void Should_map_each_error_when_failure()
        {
            // Arrange
            var sut = Result<int>.Failure(new[] { Err("a"), Err("b") });

            // Act
            var result = sut.MapErrors(e => new ValidationError(e.Message + "!", e.PropertyName));

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("a!", result.Errors[0].Message);
            Assert.Equal("b!", result.Errors[1].Message);
        }

        [Fact]
        public void Should_map_multiple_errors_correctly()
        {
            // Arrange - Test optimized array iteration with multiple errors
            var sut = Result<int>.Failure(new[]
            {
                Err("error1", "Field1"),
                Err("error2", "Field2"),
                Err("error3", "Field3"),
                Err("error4", "Field4")
            });

            // Act
            var result = sut.MapErrors(e => new ValidationError($"[MAPPED] {e.Message}", e.PropertyName));

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(4, result.Errors.Length);
            Assert.Equal("[MAPPED] error1", result.Errors[0].Message);
            Assert.Equal("Field1", result.Errors[0].PropertyName);
            Assert.Equal("[MAPPED] error2", result.Errors[1].Message);
            Assert.Equal("Field2", result.Errors[1].PropertyName);
            Assert.Equal("[MAPPED] error3", result.Errors[2].Message);
            Assert.Equal("Field3", result.Errors[2].PropertyName);
            Assert.Equal("[MAPPED] error4", result.Errors[3].Message);
            Assert.Equal("Field4", result.Errors[3].PropertyName);
        }

        [Fact]
        public void Should_throw_when_mapper_null()
        {
            // Arrange
            var sut = Result<int>.Failure(Err());

            // Act
            var act = () => sut.MapErrors(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class FilterErrors
    {
        [Fact]
        public void Should_return_same_when_success()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act
            var result = sut.FilterErrors(_ => true);

            // Assert
            Assert.True(ReferenceEquals(sut, result));
        }

        [Fact]
        public void Should_return_success_default_when_no_errors_left()
        {
            // Arrange
            var sut = Result<int>.Failure(new[] { Err("a"), Err("b") });

            // Act
            var result = sut.FilterErrors(_ => false);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(default, result.Value);
        }

        [Fact]
        public void Should_keep_filtered_errors_when_any_left()
        {
            // Arrange
            var sut = Result<int>.Failure(new[] { Err("a"), Err("b") });

            // Act
            var result = sut.FilterErrors(e => e.Message == "b");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Single(result.Errors.ToArray());
            Assert.Equal("b", result.Errors[0].Message);
        }

        [Fact]
        public void Should_filter_multiple_errors_with_proper_capacity()
        {
            // Arrange - Test optimized List with proper initial capacity
            var sut = Result<int>.Failure(new[]
            {
                Err("keep1", "Field1"),
                Err("remove", "Field2"),
                Err("keep2", "Field3"),
                Err("remove", "Field4"),
                Err("keep3", "Field5")
            });

            // Act
            var result = sut.FilterErrors(e => e.Message.StartsWith("keep"));

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(3, result.Errors.Length);
            Assert.Equal("keep1", result.Errors[0].Message);
            Assert.Equal("keep2", result.Errors[1].Message);
            Assert.Equal("keep3", result.Errors[2].Message);
        }

        [Fact]
        public void Should_throw_when_predicate_null()
        {
            // Arrange
            var sut = Result<int>.Failure(Err());

            // Act
            var act = () => sut.FilterErrors(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(() => act());
        }
    }

    public class ToResult
    {
        [Fact]
        public void Should_convert_success_to_non_generic()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act
            var result = sut.ToResult();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Should_convert_failure_to_non_generic()
        {
            // Arrange
            var sut = Result<int>.Failure(Err("x"));

            // Act
            var result = sut.ToResult();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("x", result.Errors[0].Message);
        }
    }

    public class GetErrorMessages
    {
        [Fact]
        public void Should_return_empty_for_success_generic()
        {
            // Arrange
            var sut = Result<int>.Success(1);

            // Act
            var result = sut.GetErrorMessages();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Should_use_fast_path_for_single_error_generic()
        {
            // Arrange - Test optimized single-error fast path
            var sut = Result<int>.Failure(Err("single error", "Field"));

            // Act
            var result = sut.GetErrorMessages();

            // Assert
            Assert.Equal("Field: single error", result);
        }

        [Fact]
        public void Should_join_errors_for_failure_generic()
        {
            // Arrange
            var sut = Result<int>.Failure(new[] { Err("a", "A"), Err("b", "B") });

            // Act
            var result = sut.GetErrorMessages(", ");

            // Assert
            Assert.Equal("A: a, B: b", result);
        }

        [Fact]
        public void Should_return_empty_for_success_nongeneric()
        {
            // Arrange
            var sut = Result.Success();

            // Act
            var result = sut.GetErrorMessages();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Should_use_fast_path_for_single_error_nongeneric()
        {
            // Arrange - Test optimized single-error fast path
            var sut = Result.Failure(Err("single error", "Field"));

            // Act
            var result = sut.GetErrorMessages();

            // Assert
            Assert.Equal("Field: single error", result);
        }

        [Fact]
        public void Should_join_errors_for_failure_nongeneric()
        {
            // Arrange
            var sut = Result.Failure(new[] { Err("a", "A"), Err("b", "B") });

            // Act
            var result = sut.GetErrorMessages(" | ");

            // Assert
            Assert.Equal("A: a | B: b", result);
        }

        [Fact]
        public void Should_handle_error_without_property_name()
        {
            // Arrange
            var sut = Result<int>.Failure(Err("error without property"));

            // Act
            var result = sut.GetErrorMessages();

            // Assert
            Assert.Equal("error without property", result);
        }
    }
}
