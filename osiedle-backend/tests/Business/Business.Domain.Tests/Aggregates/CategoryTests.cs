using CategoryAggregate = Business.Domain.Aggregates.Category;

namespace Business.Domain.Tests.Aggregates;

public class CategoryTests
{
    private static CategoryAggregate NewCategory(string name = "Manicure", string slug = "manicure", int displayOrder = 1) =>
        CategoryAggregate.Create(name, slug, displayOrder);

    [Fact]
    public void Create_WithValidData_InitializesAggregate()
    {
        var before = DateTime.UtcNow;

        var category = CategoryAggregate.Create("  Manicure Sets  ", "Manicure Sets", 3);

        Assert.NotEqual(Guid.Empty, category.Id.Value);
        Assert.Equal("Manicure Sets", category.Name);
        Assert.Equal("manicure-sets", category.Slug.Value);
        Assert.Equal(3, category.DisplayOrder);
        Assert.Null(category.DeletedAt);
        Assert.InRange(category.CreatedAt, before, DateTime.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyName_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() => CategoryAggregate.Create(name!, "manicure", 1));
    }

    [Fact]
    public void Create_With100CharsName_Succeeds()
    {
        var category = CategoryAggregate.Create(new string('a', 100), "slug", 0);

        Assert.Equal(100, category.Name.Length);
    }

    [Fact]
    public void Create_With101CharsName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CategoryAggregate.Create(new string('a', 101), "slug", 0));
    }

    [Fact]
    public void Create_WithNegativeDisplayOrder_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CategoryAggregate.Create("Manicure", "manicure", -1));
    }

    [Fact]
    public void Update_AppliesTrimmedNameAndOrder()
    {
        var category = NewCategory();

        category.Update("  Gel Polish  ", 7);

        Assert.Equal("Gel Polish", category.Name);
        Assert.Equal(7, category.DisplayOrder);
    }

    [Fact]
    public void Update_AfterArchive_Throws()
    {
        var category = NewCategory();
        category.Archive();

        Assert.Throws<InvalidOperationException>(() => category.Update("New name", 2));
    }

    [Fact]
    public void Archive_SetsDeletedAt()
    {
        var before = DateTime.UtcNow;
        var category = NewCategory();

        category.Archive();

        Assert.NotNull(category.DeletedAt);
        Assert.InRange(category.DeletedAt!.Value, before, DateTime.UtcNow);
    }

    [Fact]
    public void Archive_Twice_Throws()
    {
        var category = NewCategory();
        category.Archive();

        Assert.Throws<InvalidOperationException>(() => category.Archive());
    }
}
