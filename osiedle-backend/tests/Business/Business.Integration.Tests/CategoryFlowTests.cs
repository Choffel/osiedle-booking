using Business.Domain.Enums;

namespace Business.Integration.Tests;

public sealed class CategoryFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task GetCategoriesAsync_ReturnsAllEnumCategories()
    {
        using var scope = CreateScope();
        var service = Service(scope);

        var result = await service.GetCategoriesAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(Enum.GetValues<Category>().Length, result.Value.Count);
        Assert.Contains(result.Value, c => c.Name == nameof(Category.Haircut) && c.DisplayName == "Haircut");
        Assert.Contains(result.Value, c => c.Name == nameof(Category.Haircut) && c.Slug == "haircut");
        Assert.Contains(result.Value, c => c.Name == nameof(Category.BeardTrim) && c.DisplayName == "Beard Trim");
        Assert.Contains(result.Value, c => c.Name == nameof(Category.Shaving) && c.DisplayName == "Shaving");
    }
}
