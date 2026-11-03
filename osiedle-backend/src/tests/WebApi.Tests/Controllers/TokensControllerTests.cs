using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApi.Tests.Support;

namespace WebApi.Tests.Controllers;

public class TokensControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public TokensControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"tokens-{Guid.NewGuid():N}@example.com";

    private HttpRequestMessage Request(HttpMethod method, string url, Guid userId, string email,
        string? refreshToken = null, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        request.Headers.Authorization = new("Bearer", _factory.CreateAccessToken(userId, email));
        if (refreshToken is not null)
            request.Headers.Add("X-Refresh-Token", refreshToken);
        return request;
    }

    // ---------- GET ActiveSessions ----------

    [Fact]
    public async Task ActiveSessions_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/Tokens/ActiveSessions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActiveSessions_ReturnsOwnSessions_WithCurrentFlag()
    {
        var email = UniqueEmail();
        var userId = await _factory.RegisterUserAsync(_client, email, "Password1");
        var currentId = await _factory.SeedRefreshTokenAsync(userId, "current-token", email);
        await _factory.SeedRefreshTokenAsync(userId, "other-token", email);

        var response = await _client.SendAsync(
            Request(HttpMethod.Get, "/api/Tokens/ActiveSessions", userId, email, "current-token"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var sessions = body.RootElement.EnumerateArray().ToList();
        sessions.Should().HaveCount(2, "other users' sessions must not leak into the list");

        sessions.Single(s => s.GetProperty("tokenId").GetGuid() == currentId)
            .GetProperty("isCurrentSession").GetBoolean().Should().BeTrue();
        sessions.Single(s => s.GetProperty("tokenId").GetGuid() != currentId)
            .GetProperty("isCurrentSession").GetBoolean().Should().BeFalse();
    }

    // ---------- POST NewAccessToken ----------

    [Fact]
    public async Task NewAccessToken_RotatesTokens_AndRevokesTheOldRow()
    {
        var email = UniqueEmail();
        var userId = await _factory.RegisterUserAsync(_client, email, "Password1");
        await _factory.SeedRefreshTokenAsync(userId, "old-refresh", email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/Tokens/NewAccessToken");
        request.Headers.Add("X-Refresh-Token", "old-refresh");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var newRefresh = body.RootElement.GetProperty("refreshToken").GetString();
        newRefresh.Should().NotBe("old-refresh");
        body.RootElement.GetProperty("accessToken").GetString()!.Split('.').Should().HaveCount(3);

        var rows = await _factory.QueryUsersDbAsync(db =>
            db.RefreshTokens.AsNoTracking().Where(rt => rt.UserId == userId).ToListAsync());
        rows.Should().HaveCount(2);
        rows.Single(rt => rt.Token == "old-refresh").RevokedAt.Should().NotBeNull(
            "rotation must revoke the used token");
        rows.Single(rt => rt.Token == newRefresh).RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task NewAccessToken_UnknownToken_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/Tokens/NewAccessToken");
        request.Headers.Add("X-Refresh-Token", "missing-token");

        var response = await _client.SendAsync(request);

        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "Token.InvalidRefreshToken");
    }

    // ---------- POST RevokeRefreshToken (logout current) ----------

    [Fact]
    public async Task RevokeRefreshToken_Returns204_AndRevokesCurrentSession()
    {
        var email = UniqueEmail();
        var userId = await _factory.RegisterUserAsync(_client, email, "Password1");
        await _factory.SeedRefreshTokenAsync(userId, "logout-token", email);

        var response = await _client.SendAsync(
            Request(HttpMethod.Post, "/api/Tokens/RevokeRefreshToken", userId, email, "logout-token"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var stored = await _factory.QueryUsersDbAsync(db =>
            db.RefreshTokens.AsNoTracking().SingleAsync(rt => rt.Token == "logout-token"));
        stored.RevokedAt.Should().NotBeNull();
    }

    // ---------- POST RevokeSession (other sessions by id) ----------

    [Fact]
    public async Task RevokeSession_OtherSession_Returns204_AndRevokesOnlyIt()
    {
        var email = UniqueEmail();
        var userId = await _factory.RegisterUserAsync(_client, email, "Password1");
        await _factory.SeedRefreshTokenAsync(userId, "current-token", email);
        var otherId = await _factory.SeedRefreshTokenAsync(userId, "other-token", email);

        var response = await _client.SendAsync(Request(
            HttpMethod.Post, "/api/Tokens/RevokeSession", userId, email, "current-token", body: otherId));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var rows = await _factory.QueryUsersDbAsync(db =>
            db.RefreshTokens.AsNoTracking().Where(rt => rt.UserId == userId).ToListAsync());
        rows.Single(rt => rt.Token == "other-token").RevokedAt.Should().NotBeNull();
        rows.Single(rt => rt.Token == "current-token").RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task RevokeSession_CurrentSession_Returns409Conflict()
    {
        var email = UniqueEmail();
        var userId = await _factory.RegisterUserAsync(_client, email, "Password1");
        var currentId = await _factory.SeedRefreshTokenAsync(userId, "current-token", email);

        var response = await _client.SendAsync(Request(
            HttpMethod.Post, "/api/Tokens/RevokeSession", userId, email, "current-token", body: currentId));

        await response.ShouldFailWithAsync(HttpStatusCode.Conflict, "Token.CannotRevokeCurrentSession");
    }

    [Fact]
    public async Task RevokeSession_UnknownTokenId_Returns400()
    {
        var email = UniqueEmail();
        var userId = await _factory.RegisterUserAsync(_client, email, "Password1");

        var response = await _client.SendAsync(Request(
            HttpMethod.Post, "/api/Tokens/RevokeSession", userId, email, "whatever", body: Guid.NewGuid()));

        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "Token.InvalidRefreshToken");
    }

    [Fact]
    public async Task RevokeSession_WithoutToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync($"/api/Tokens/RevokeSession?tokenId={Guid.NewGuid()}",
            new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
