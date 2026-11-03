using System.Net;
using FluentAssertions;

namespace WebApi.Tests.Support;

internal static class HttpAssertExtensions
{
    /// <summary>
    /// Asserts the HTTP status and that the ProblemDetails body carries the given
    /// error code (works whether extensions are flattened or nested).
    /// </summary>
    public static async Task ShouldFailWithAsync(this HttpResponseMessage response, HttpStatusCode status, string errorCode)
    {
        response.StatusCode.Should().Be(status);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain($"\"code\":\"{errorCode}\"",
            because: "the response must map the domain error code, body: {0}", body);
    }
}
