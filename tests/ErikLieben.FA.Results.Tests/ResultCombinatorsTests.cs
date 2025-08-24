using ErikLieben.FA.Results;
using Xunit;

namespace ErikLieben.FA.Results.Tests;

public class ResultCombinatorsTests
{
    private static ValidationError Err(string msg = "err", string? prop = null) => new(msg, prop);

    public class CombineSpan
    {
        [Fact]
        public void Should_return_first_success_when_no_errors()
        {
            // Arrange
            var r1 = Result<int>.Success(1);
            var r2 = Result<int>.Success(2);
            var r3 = Result<int>.Success(3);
            var span = new[] { r1, r2, r3 }.AsSpan();

            // Act
            var sut = ResultCombinators.Combine(span);

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal(1, sut.Value);
        }

        [Fact]
        public void Should_return_all_errors_when_any_failure()
        {
            // Arrange
            var r1 = Result<int>.Failure(Err("a"));
            var r2 = Result<int>.Success(2);
            var r3 = Result<int>.Failure(Err("b"));
            var span = new[] { r1, r2, r3 }.AsSpan();

            // Act
            var sut = ResultCombinators.Combine(span);

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(2, sut.Errors.Length);
        }
    }

    public class CombineParamsSameType
    {
        [Fact]
        public void Should_delegate_to_span_overload()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result<int>.Success(10));

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal(10, sut.Value);
        }
    }

    public class Combine2
    {
        [Fact]
        public void Should_return_tuple_when_both_success()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result<int>.Success(1), Result<string>.Success("x"));

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal((1, "x"), sut.Value);
        }

        [Fact]
        public void Should_accumulate_errors_from_both()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result<int>.Failure(Err("a")), Result<string>.Failure(Err("b")));

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(2, sut.Errors.Length);
        }
    }

    public class Combine3
    {
        [Fact]
        public void Should_return_tuple_when_all_success()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result<int>.Success(1), Result<string>.Success("x"), Result<bool>.Success(true));

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal((1, "x", true), sut.Value);
        }

        [Fact]
        public void Should_accumulate_errors_from_all()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result<int>.Failure(Err("a")), Result<string>.Failure(Err("b")), Result<bool>.Failure(Err("c")));

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(3, sut.Errors.Length);
        }
    }

    public class Combine4
    {
        [Fact]
        public void Should_return_tuple_when_all_success()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result<int>.Success(1), Result<string>.Success("x"), Result<bool>.Success(true), Result<double>.Success(1.2));

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal((1, "x", true, 1.2), sut.Value);
        }

        [Fact]
        public void Should_accumulate_errors()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result<int>.Failure(Err("a")), Result<string>.Failure(Err("b")), Result<bool>.Failure(Err("c")), Result<double>.Failure(Err("d")));

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(4, sut.Errors.Length);
        }
    }

    public class Combine5
    {
        [Fact]
        public void Should_return_tuple_when_all_success()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result<int>.Success(1), Result<string>.Success("x"), Result<bool>.Success(true), Result<double>.Success(1.2), Result<long>.Success(9L));

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal((1, "x", true, 1.2, 9L), sut.Value);
        }

        [Fact]
        public void Should_accumulate_errors()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result<int>.Failure(Err("a")), Result<string>.Failure(Err("b")), Result<bool>.Failure(Err("c")), Result<double>.Failure(Err("d")), Result<long>.Failure(Err("e")));

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(5, sut.Errors.Length);
        }
    }

    public class CombineNonGeneric
    {
        [Fact]
        public void Should_return_success_when_no_failures()
        {
            // Arrange
            var span = new[] { Result.Success(), Result.Success() }.AsSpan();

            // Act
            var sut = ResultCombinators.Combine(span);

            // Assert
            Assert.True(sut.IsSuccess);
        }

        [Fact]
        public void Should_return_failure_with_all_errors()
        {
            // Arrange
            var sut = ResultCombinators.Combine(Result.Failure(Err("a")), Result.Failure(Err("b")));

            // Assert
            Assert.True(sut.IsFailure);
            Assert.Equal(2, sut.Errors.Length);
        }
    }
}
