using System.IO;
using System.Text;
using System.Threading.Tasks;
using ErikLieben.FA.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ErikLieben.FA.Results.Tests;

public class ResultMinimalApiExtensionsTests
{
    private static DefaultHttpContext NewContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        // Minimal DI container to satisfy built-in Results serialization in tests
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging();
        services.AddSingleton<IOptions<JsonOptions>>(Options.Create(new JsonOptions()));
        ctx.RequestServices = services.BuildServiceProvider();
        return ctx;
    }

    private static async Task<(int StatusCode, string Body)> ExecuteAsync(IResult result)
    {
        var ctx = NewContext();
        await result.ExecuteAsync(ctx);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(ctx.Response.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        return (ctx.Response.StatusCode, body);
    }

    private static ValidationError Err(string msg = "err", string? prop = null) => new(msg, prop);

    public class ToHttpResultDefault
    {
        [Fact]
        public async Task Should_return_ok_when_success()
        {
            // Arrange
            var sut = Result<string>.Success("x");

            // Act
            var (code, body) = await ExecuteAsync(sut.ToHttpResult("ok"));

            // Assert
            Assert.Equal(200, code);
            Assert.Contains("ok", body);
        }

        [Fact]
        public async Task Should_return_bad_request_when_failure()
        {
            // Arrange
            var sut = Result<string>.Failure(Err("bad"));

            // Act
            var (code, body) = await ExecuteAsync(sut.ToHttpResult(null, "bad"));

            // Assert
            Assert.Equal(400, code);
            Assert.Contains("bad", body);
        }
    }

    public class ToHttpResultCustomStatus
    {
        [Fact]
        public async Task Should_return_custom_status_code()
        {
            // Arrange
            var sut = Result<string>.Success("x");

            // Act
            var (code, _) = await ExecuteAsync(sut.ToHttpResult(202, 418, "ok", "bad"));

            // Assert
            Assert.Equal(202, code);
        }
    }

    public class ToCreatedResults
    {
        [Fact]
        public async Task Should_return_created_when_success()
        {
            // Arrange
            var sut = Result<string>.Success("x");

            // Act
            var (code, _) = await ExecuteAsync(sut.ToCreatedHttpResult("/items/1", "ok"));

            // Assert
            Assert.Equal(201, code);
        }

        [Fact]
        public async Task Should_return_bad_request_when_failure()
        {
            // Arrange
            var sut = Result<string>.Failure(Err("bad"));

            // Act
            var (code, _) = await ExecuteAsync(sut.ToCreatedAtRouteHttpResult("route", new { id = 1 }, null, "bad"));

            // Assert
            Assert.Equal(400, code);
        }
    }

    public class ToHttpResultWithMapping
    {
        [Fact]
        public async Task Should_map_and_return_http_result()
        {
            // Arrange
            var sut = Result<int>.Success(5);

            // Act
            var (code, body) = await ExecuteAsync(sut.ToHttpResult(x => (x * 2).ToString(), "ok", "bad"));

            // Assert
            Assert.Equal(200, code);
            Assert.Contains("\"data\":\"10\"", body);
        }
    }
}
