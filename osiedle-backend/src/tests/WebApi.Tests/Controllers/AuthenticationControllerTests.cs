using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using WebApi.Tests.Support;

namespace WebApi.Tests.Controllers;

public class AuthenticationControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"auth-{Guid.NewGuid():N}@example.com";
    private static string UniqueName() => "u" + Guid.NewGuid().ToString("N")[..8];

    private Task<HttpResponseMessage> RegisterAsync(string email, string password = "Password1", string? firstName = null) =>
        _client.PostAsJsonAsync("/api/Authentication/register", new
        {
            firstName = firstName ?? UniqueName(),
            lastName = "Kowalski",
            email,
            password,
        });

    // ---------- register ----------

    [Fact]
    public async Task Register_ReturnsNewUserId()
    {
        var response = await RegisterAsync(UniqueEmail());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userId = await response.Content.ReadFromJsonAsync<Guid>();
        userId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400WithIdentityCode()
    {
        var response = await RegisterAsync(UniqueEmail(), password: "abc");

        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "IdentityError");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = UniqueEmail();
        (await RegisterAsync(email)).EnsureSuccessStatusCode();

        var duplicate = await RegisterAsync(email);

        await duplicate.ShouldFailWithAsync(HttpStatusCode.BadRequest, "IdentityError");
    }

    // ---------- login ----------

    [Fact]
    public async Task Login_UnknownEmail_Returns400WithInvalidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/api/Authentication/login",
            new { email = UniqueEmail(), password = "Password1" });

        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "Identity.InvalidCredentials");
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsSameErrorAsUnknownEmail()
    {
        var email = UniqueEmail();
        (await RegisterAsync(email)).EnsureSuccessStatusCode();
        await _factory.ConfirmEmailAsync(_client, email);

        var response = await _client.PostAsJsonAsync("/api/Authentication/login",
            new { email, password = "WrongPassword1" });

        // Indistinguishable from the unknown-email answer: no account enumeration.
        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "Identity.InvalidCredentials");
    }

    [Fact]
    public async Task Login_BeforeEmailConfirm_Returns400()
    {
        var email = UniqueEmail();
        (await RegisterAsync(email)).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/Authentication/login",
            new { email, password = "Password1" });

        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "Identity.InvalidCredentials");
    }

    [Fact]
    public async Task Login_AfterConfirm_ReturnsJwtPair()
    {
        var email = UniqueEmail();
        (await RegisterAsync(email)).EnsureSuccessStatusCode();
        await _factory.ConfirmEmailAsync(_client, email);

        var response = await _client.PostAsJsonAsync("/api/Authentication/login",
            new { email, password = "Password1" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("accessToken").GetString()!.Split('.').Should().HaveCount(3);
        body.RootElement.GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace();
    }

    // ---------- forgot password ----------

    [Fact]
    public async Task ForgotPassword_Returns204_ForUnknownEmail()
    {
        var response = await _client.PostAsJsonAsync("/api/Authentication/forgot-password",
            new { email = UniqueEmail() });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ForgotPassword_ReturnsSame204_ForKnownEmail()
    {
        var email = UniqueEmail();
        (await RegisterAsync(email)).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/Authentication/forgot-password",
            new { email });

        // The answer must not reveal whether the account exists.
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ---------- confirm email ----------

    [Fact]
    public async Task ConfirmEmail_UnknownEmail_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/api/Authentication/confirm-email",
            new { email = UniqueEmail(), token = "any-token" });

        await response.ShouldFailWithAsync(HttpStatusCode.NotFound, "User.EmailNotRegistered");
    }

    [Fact]
    public async Task ConfirmEmail_WrongToken_Returns400()
    {
        var email = UniqueEmail();
        (await RegisterAsync(email)).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/Authentication/confirm-email",
            new { email, token = "garbage-token" });

        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "User.EmailConfirmationFailed");
    }

    [Fact]
    public async Task ConfirmEmail_Success_ThenLoginWorks()
    {
        var email = UniqueEmail();
        (await RegisterAsync(email)).EnsureSuccessStatusCode();

        await _factory.ConfirmEmailAsync(_client, email);

        var login = await _client.PostAsJsonAsync("/api/Authentication/login",
            new { email, password = "Password1" });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ---------- reset password ----------

    [Fact]
    public async Task ResetPassword_UnknownEmail_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/api/Authentication/reset-password",
            new { email = UniqueEmail(), token = "any", newPassword = "BrandNew1" });

        await response.ShouldFailWithAsync(HttpStatusCode.NotFound, "User.EmailNotRegistered");
    }

    [Fact]
    public async Task ResetPassword_FullCycle_SwitchesThePassword()
    {
        var email = UniqueEmail();
        (await RegisterAsync(email)).EnsureSuccessStatusCode();
        await _factory.ConfirmEmailAsync(_client, email);
        var resetToken = await _factory.GeneratePasswordResetTokenAsync(email);

        var reset = await _client.PostAsJsonAsync("/api/Authentication/reset-password",
            new { email, token = resetToken, newPassword = "BrandNew1" });
        reset.StatusCode.Should().Be(HttpStatusCode.OK);
        // The message endpoint answers text/plain, not a JSON string.
        (await reset.Content.ReadAsStringAsync())
            .Should().Be("The password was successfully recovered.");

        var oldPasswordLogin = await _client.PostAsJsonAsync("/api/Authentication/login",
            new { email, password = "Password1" });
        await oldPasswordLogin.ShouldFailWithAsync(HttpStatusCode.BadRequest, "Identity.InvalidCredentials");

        var newPasswordLogin = await _client.PostAsJsonAsync("/api/Authentication/login",
            new { email, password = "BrandNew1" });
        newPasswordLogin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WrongToken_Returns400()
    {
        var email = UniqueEmail();
        (await RegisterAsync(email)).EnsureSuccessStatusCode();
        await _factory.ConfirmEmailAsync(_client, email);

        var response = await _client.PostAsJsonAsync("/api/Authentication/reset-password",
            new { email, token = "garbage", newPassword = "BrandNew1" });

        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "User.PasswordResetFailed");
    }
}
