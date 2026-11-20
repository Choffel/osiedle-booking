using System.Net;
using System.Net.Http.Json;
using Business.Application.DTOs.CategoryDTOs;
using Business.Domain.Enums;
using FluentAssertions;
using WebApi.Tests.Support;

namespace WebApi.Tests.Controllers;

/// <summary>
/// HTTP contract of CategoryController: public read of global categories enum.
/// </summary>
public class CategoryControllerTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CategoryControllerTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_ReturnsOk_WithAllEnumCategories()
    {
        var response = await _client.GetAsync("/api/Category");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var categories = await response.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>();
        categories.Should().NotBeNull();
        categories.Should().HaveCount(Enum.GetValues<Category>().Length);

        var haircut = categories!.FirstOrDefault(c => c.Name == nameof(Category.Haircut));
        haircut.Should().NotBeNull();
        haircut!.Id.Should().Be((int)Category.Haircut);
        haircut.DisplayName.Should().Be("Haircut");
        haircut.Slug.Should().Be("haircut");

        var beardTrim = categories.FirstOrDefault(c => c.Name == nameof(Category.BeardTrim));
        beardTrim.Should().NotBeNull();
        beardTrim!.DisplayName.Should().Be("Beard Trim");
    }
}
