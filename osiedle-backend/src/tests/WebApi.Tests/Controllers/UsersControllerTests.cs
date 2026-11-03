using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Identity.Models;
using WebApi.Tests.Support;

namespace WebApi.Tests.Controllers;

public class UsersControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public UsersControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"users-{Guid.NewGuid():N}@example.com";

    private async Task<(Guid Id, string Email)> RegisterConfirmedUserAsync(string? firstName = null)
    {
        var email = UniqueEmail();
        var id = await _factory.RegisterUserAsync(_client, email, "Password1", firstName);
        await _factory.ConfirmEmailAsync(_client, email);
        return (id, email);
    }

    // ---------- GET me ----------

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/Users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_ReturnsProfile_ForRegisteredUser()
    {
        var (id, email) = await RegisterConfirmedUserAsync(firstName: "Jan");
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", _factory.CreateAccessToken(id, email));

        var response = await _client.GetAsync("/api/Users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("firstName").GetString().Should().Be("Jan");
        body.RootElement.GetProperty("lastName").GetString().Should().Be("Kowalski");
    }

    [Fact]
    public async Task Me_UnknownUserId_Returns404()
    {
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", _factory.CreateAccessToken(Guid.NewGuid(), UniqueEmail()));

        var response = await _client.GetAsync("/api/Users/me");

        await response.ShouldFailWithAsync(HttpStatusCode.NotFound, "User.NotFound");
    }

    // ---------- PUT Update ----------

    [Fact]
    public async Task UpdateName_PersistsDomainAndIdentityName()
    {
        var (id, email) = await RegisterConfirmedUserAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", _factory.CreateAccessToken(id, email));

        var response = await _client.PutAsJsonAsync("/api/Users/UpdateName",
            new { firstName = "Nowanazwa", lastName = "Kowalski" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var stored = await _factory.QueryUsersDbAsync(db =>
            db.Set<ApplicationUser>().AsNoTracking().SingleAsync(u => u.Id == id));
        stored.UserName.Should().Be("Nowanazwa");

        var me = await _client.GetAsync("/api/Users/me");
        me.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("firstName").GetString().Should().Be("Nowanazwa");
        body.RootElement.GetProperty("lastName").GetString().Should().Be("Kowalski");
    }

    [Fact]
    public async Task UpdateName_UnknownEmailClaim_Returns404()
    {
        var (id, _) = await RegisterConfirmedUserAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", _factory.CreateAccessToken(id, UniqueEmail()));

        var response = await _client.PutAsJsonAsync("/api/Users/UpdateName",
            new { firstName = "Nowanazwa", lastName = "Kowalski" });

        await response.ShouldFailWithAsync(HttpStatusCode.NotFound, "User.NotFound");
    }

    [Fact]
    public async Task UpdateName_WithoutToken_Returns401()
    {
        var response = await _client.PutAsJsonAsync("/api/Users/UpdateName",
            new { firstName = "Nowanazwa", lastName = "Kowalski" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateEmail_PersistsDomainAndIdentityEmail()
    {
        var (id, email) = await RegisterConfirmedUserAsync();
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", _factory.CreateAccessToken(id, email));
        var newEmail = $"renamed-{Guid.NewGuid():N}@example.com";

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/Users/UpdateEmail")
        {
            Content = JsonContent.Create(newEmail),
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var storedIdentity = await _factory.QueryUsersDbAsync(db =>
            db.Set<ApplicationUser>().AsNoTracking().SingleAsync(u => u.Id == id));
        storedIdentity.Email.Should().Be(newEmail);

        var storedDomain = await _factory.QueryUsersDbAsync(db =>
            db.Users.AsNoTracking().SingleAsync(u => u.Id == osiedle_booking.BuildingBlocks.ValueObjects.UserId.From(id)));
        storedDomain.Email.Should().Be(newEmail);
    }

    [Fact]
    public async Task UpdateEmail_UnknownUserId_Returns404()
    {
        _client.DefaultRequestHeaders.Authorization =
            new("Bearer", _factory.CreateAccessToken(Guid.NewGuid(), UniqueEmail()));

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/Users/UpdateEmail")
        {
            Content = JsonContent.Create("renamed@example.com"),
        };
        var response = await _client.SendAsync(request);

        await response.ShouldFailWithAsync(HttpStatusCode.NotFound, "User.NotFound");
    }
}
