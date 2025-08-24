using System;
using ErikLieben.FA.Results;
using Xunit;

namespace ErikLieben.FA.Results.Tests;

public class ApiResponseTests
{
    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private readonly DateTimeOffset _now = now;
        public override long GetTimestamp() => _now.UtcTicks;
        public override DateTimeOffset GetUtcNow() => _now;
    }

    public class Success
    {
        [Fact]
        public void Should_wrap_data_and_set_timestamp()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero);
            ApiResponseTimeProvider.SharedTimeProvider = new FixedTimeProvider(now);
            var data = new { Name = "Erik" };

            // Act
            var sut = ApiResponse<object>.Success(data, "ok");

            // Assert
            Assert.True(sut.IsSuccess);
            Assert.Equal(data, sut.Data);
            Assert.Equal("ok", sut.Message);
            Assert.Equal(now, sut.Timestamp);
        }
    }

    public class Failure
    {
        [Fact]
        public void Should_wrap_errors_and_set_timestamp()
        {
            // Arrange
            var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            ApiResponseTimeProvider.SharedTimeProvider = new FixedTimeProvider(now);
            var errors = new[] { new ApiError("e1", "P1"), new ApiError("e2") };

            // Act
            var sut = ApiResponse<string>.Failure(errors, "bad");

            // Assert
            Assert.False(sut.IsSuccess);
            Assert.Null(sut.Data);
            Assert.Equal(errors, sut.Errors);
            Assert.Equal("bad", sut.Message);
            Assert.Equal(now, sut.Timestamp);
        }
    }

    public class ApiResponseTimeProviderTests
    {
        [Fact]
        public void Should_use_system_when_setting_null()
        {
            // Arrange
            ApiResponseTimeProvider.SharedTimeProvider = null!; // should reset to system

            // Act
            var tp = ApiResponseTimeProvider.SharedTimeProvider;

            // Assert
            Assert.NotNull(tp);
        }
    }
}
