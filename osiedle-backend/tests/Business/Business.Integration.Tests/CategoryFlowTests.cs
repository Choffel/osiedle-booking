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
        Assert.Contains(result.Value, c => c.Id == (int)Category.Haircut && c.DisplayName == "Haircut");
        Assert.Contains(result.Value, c => c.Id == (int)Category.Haircut && c.Slug == "haircut");
        Assert.Contains(result.Value, c => c.Id == (int)Category.BeardTrim && c.DisplayName == "Beard Trim");
        Assert.Contains(result.Value, c => c.Id == (int)Category.Shaving && c.DisplayName == "Shaving");
    }
}
