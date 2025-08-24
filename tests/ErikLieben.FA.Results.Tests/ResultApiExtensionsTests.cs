using ErikLieben.FA.Results;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ErikLieben.FA.Results.Tests;

public class ResultApiExtensionsTests
{
    private static ValidationError Err(string msg = "err", string? prop = null) => new(msg, prop);

    public class ToApiResponse
    {
        [Fact]
        public void Should_wrap_success_into_api_response()
        {
            // Arrange
            var sut = Result<int>.Success(7);

            // Act
            var response = sut.ToApiResponse("ok");

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal(7, response.Data);
            Assert.Equal("ok", response.Message);
        }

        [Fact]
        public void Should_wrap_failure_into_api_response()
        {
            // Arrange
            var sut = Result<int>.Failure(new[] { Err("a", "A") });

            // Act
            var response = sut.ToApiResponse(null, "bad");

            // Assert
            Assert.False(response.IsSuccess);
            Assert.NotNull(response.Errors);
            Assert.Single(response.Errors!);
            Assert.Equal("A", response.Errors![0].PropertyName);
            Assert.Equal("bad", response.Message);
        }
    }

    public class ToActionResult
    {
        [Fact]
        public void Should_return_ok_object_result_when_success()
        {
            // Arrange
            var sut = Result<string>.Success("x");

            // Act
            var actionResult = sut.ToActionResult("ok");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var payload = Assert.IsType<ApiResponse<string>>(ok.Value);
            Assert.Equal("x", payload.Data);
        }

        [Fact]
        public void Should_return_bad_request_object_result_when_failure()
        {
            // Arrange
            var sut = Result<string>.Failure(Err("bad"));

            // Act
            var actionResult = sut.ToActionResult(null, "no");

            // Assert
            var br = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.IsType<ApiResponse<string>>(br.Value);
        }

        [Fact]
        public void Should_return_custom_status_codes()
        {
            // Arrange
            var sut = Result<string>.Success("x");

            // Act
            var actionResult = sut.ToActionResult(202, 418, "ok", "bad");

            // Assert
            var obj = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(202, obj.StatusCode);
            Assert.IsType<ApiResponse<string>>(obj.Value);
        }
    }

    public class ToCreatedResults
    {
        [Fact]
        public void Should_return_created_when_success()
        {
            // Arrange
            var sut = Result<string>.Success("x");

            // Act
            var actionResult = sut.ToCreatedResult("/items/1", "ok");

            // Assert
            var created = Assert.IsType<CreatedResult>(actionResult);
            Assert.Equal("/items/1", created.Location);
            Assert.IsType<ApiResponse<string>>(created.Value);
        }

        [Fact]
        public void Should_return_created_at_action_when_success()
        {
            // Arrange
            var sut = Result<string>.Success("x");

            // Act
            var actionResult = sut.ToCreatedAtActionResult("Get", "Items", new { id = 1 }, "ok");

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(actionResult);
            Assert.Equal("Get", created.ActionName);
            Assert.Equal("Items", created.ControllerName);
            Assert.IsType<ApiResponse<string>>(created.Value);
        }

        [Fact]
        public void Should_return_bad_request_when_failure()
        {
            // Arrange
            var sut = Result<string>.Failure(Err("bad"));

            // Act
            var actionResult = sut.ToCreatedResult("/items", null, "bad");

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }
    }
}
