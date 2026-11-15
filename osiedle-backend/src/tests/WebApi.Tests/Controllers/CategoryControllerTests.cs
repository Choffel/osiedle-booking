using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using osiedle_booking.BuildingBlocks.Roles;
using WebApi.Tests.Support;

namespace WebApi.Tests.Controllers;

/// <summary>
/// HTTP contract of CategoryController: public read, Admin-only mutations,
/// ProblemDetails mapping for domain validation (ArgumentException -> 400)
/// and typed Result errors (409/404).
/// </summary>
public class CategoryControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public CategoryControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string UniqueName(string tag) => $"{tag}-{Guid.NewGuid():N}";

    private string AdminBearer() =>
        _factory.CreateAccessToken(Guid.NewGuid(), $"admin-{Guid.NewGuid():N}@example.com", UserRoles.Admin);

    private string UserBearer() =>
        _factory.CreateAccessToken(Guid.NewGuid(), $"user-{Guid.NewGuid():N}@example.com", UserRoles.User);

    private HttpRequestMessage CreateRequest(string bearer, string name, string slug, int order = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/Category/new")
        {
            Content = JsonContent.Create(new { name, slug, displayOrder = order }),
        };
        if (bearer != string.Empty)
            request.Headers.Authorization = new("Bearer", bearer);
        return request;
    }

    private HttpRequestMessage ArchiveRequest(string bearer, string slug)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/Category/archive?slug={Uri.EscapeDataString(slug)}");
        if (bearer != string.Empty)
            request.Headers.Authorization = new("Bearer", bearer);
        return request;
    }

    private async Task<string> CreateAsAdminAsync(string? name = null, string? slug = null, int order = 1)
    {
        name ??= UniqueName("manicure");
        slug ??= name.ToLowerInvariant();

        var response = await _client.SendAsync(CreateRequest(AdminBearer(), name, slug, order));
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        return name;
    }

    // ---------- create ----------

    [Fact]
    public async Task Create_Anonymous_Returns401()
    {
        var response = await _client.SendAsync(CreateRequest(string.Empty, UniqueName("anon"), "anon-slug"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithoutAdminRole_Returns403()
    {
        var response = await _client.SendAsync(CreateRequest(UserBearer(), UniqueName("user"), "user-slug"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsAdmin_Returns204AndCategoryVisibleInPublicList()
    {
        var name = await CreateAsAdminAsync();

        var list = await _client.GetAsync("/api/Category");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        (await list.Content.ReadAsStringAsync()).Should().Contain(name);
    }

    [Fact]
    public async Task Create_DuplicateName_Returns409WithDomainCode()
    {
        var name = await CreateAsAdminAsync();

        var duplicate = await _client.SendAsync(
            CreateRequest(AdminBearer(), name, name.ToLowerInvariant() + "-2"));

        await duplicate.ShouldFailWithAsync(HttpStatusCode.Conflict, "Category.AlreadyExists.");
    }

    [Fact]
    public async Task Create_InvalidSlug_Returns400()
    {
        var response = await _client.SendAsync(
            CreateRequest(AdminBearer(), UniqueName("nails"), "!!!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_NameTooLong_Returns400()
    {
        var response = await _client.SendAsync(
            CreateRequest(AdminBearer(), new string('a', 101), UniqueName("long")));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---------- archive ----------

    [Fact]
    public async Task Archive_Anonymous_Returns401()
    {
        var response = await _client.SendAsync(ArchiveRequest(string.Empty, "some-slug"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Archive_WithoutAdminRole_Returns403()
    {
        var response = await _client.SendAsync(ArchiveRequest(UserBearer(), "some-slug"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Archive_UnknownSlug_Returns404WithDomainCode()
    {
        var response = await _client.SendAsync(
            ArchiveRequest(AdminBearer(), $"missing-{Guid.NewGuid():N}"));

        await response.ShouldFailWithAsync(HttpStatusCode.NotFound, "Category.NotFound.");
    }

    [Fact]
    public async Task Archive_InvalidSlugValue_Returns400()
    {
        var response = await _client.SendAsync(ArchiveRequest(AdminBearer(), "-not a slug-"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Archive_Existing_Returns204_HidesFromList_AndRepeatReturns404()
    {
        var name = await CreateAsAdminAsync();

        var archive = await _client.SendAsync(
            ArchiveRequest(AdminBearer(), name.ToLowerInvariant()));
        archive.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await _client.GetAsync("/api/Category");
        (await list.Content.ReadAsStringAsync()).Should().NotContain(name);

        var again = await _client.SendAsync(
            ArchiveRequest(AdminBearer(), name.ToLowerInvariant()));
        await again.ShouldFailWithAsync(HttpStatusCode.NotFound, "Category.NotFound.");
    }
}
