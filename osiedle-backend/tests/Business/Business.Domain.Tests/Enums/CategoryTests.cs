using Business.Domain.Enums;

namespace Business.Domain.Tests.Enums;

public class CategoryTests
{
    [Fact]
    public void Category_ContainsExpectedBarbershopAndSalonCategories()
    {
        Assert.True(Enum.IsDefined(Category.Haircut));
        Assert.True(Enum.IsDefined(Category.BeardTrim));
        Assert.True(Enum.IsDefined(Category.Shaving));
        Assert.True(Enum.IsDefined(Category.Styling));
        Assert.True(Enum.IsDefined(Category.Coloring));
        Assert.True(Enum.IsDefined(Category.HairCare));
        Assert.True(Enum.IsDefined(Category.HairWash));
        Assert.True(Enum.IsDefined(Category.KidsHaircut));
        Assert.True(Enum.IsDefined(Category.Combo));
        Assert.True(Enum.IsDefined(Category.FacialCare));
        Assert.True(Enum.IsDefined(Category.Waxing));
    }

    [Theory]
    [InlineData(Category.Haircut, "Haircut")]
    [InlineData(Category.BeardTrim, "Beard Trim")]
    [InlineData(Category.Shaving, "Shaving")]
    [InlineData(Category.Styling, "Styling")]
    [InlineData(Category.Coloring, "Coloring")]
    [InlineData(Category.HairCare, "Hair Care")]
    [InlineData(Category.HairWash, "Hair Wash")]
    [InlineData(Category.KidsHaircut, "Kids Haircut")]
    [InlineData(Category.Combo, "Combo")]
    [InlineData(Category.FacialCare, "Facial Care")]
    [InlineData(Category.Waxing, "Waxing")]
    public void ToDisplayName_ReturnsExpectedEnglishName(Category category, string expectedDisplayName)
    {
        Assert.Equal(expectedDisplayName, category.ToDisplayName());
    }

    [Theory]
    [InlineData(Category.Haircut, "haircut")]
    [InlineData(Category.BeardTrim, "beard-trim")]
    [InlineData(Category.HairCare, "hair-care")]
    [InlineData(Category.KidsHaircut, "kids-haircut")]
    public void ToSlug_ReturnsUrlFriendlySlug(Category category, string expectedSlug)
    {
        Assert.Equal(expectedSlug, category.ToSlug());
    }

    [Theory]
    [InlineData("haircut", Category.Haircut)]
    [InlineData("BEARD-TRIM", Category.BeardTrim)]
    [InlineData(" hair-care ", Category.HairCare)]
    public void TryFromSlug_ReturnsCategory(string slug, Category expectedCategory)
    {
        Assert.True(CategoryExtensions.TryFromSlug(slug, out var category));
        Assert.Equal(expectedCategory, category);
    }

    [Fact]
    public void TryFromSlug_WithInvalidSlug_ReturnsFalse()
    {
        Assert.False(CategoryExtensions.TryFromSlug("invalid-category", out _));
    }
}
