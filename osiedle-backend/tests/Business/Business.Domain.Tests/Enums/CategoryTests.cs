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
    [InlineData(Category.Haircut, "Стрижка")]
    [InlineData(Category.BeardTrim, "Стрижка бороды")]
    [InlineData(Category.Shaving, "Бритье")]
    [InlineData(Category.Styling, "Укладка")]
    [InlineData(Category.Coloring, "Окрашивание")]
    [InlineData(Category.HairCare, "Уход за волосами")]
    [InlineData(Category.HairWash, "Мытье головы")]
    [InlineData(Category.KidsHaircut, "Детская стрижка")]
    [InlineData(Category.Combo, "Комплекс")]
    [InlineData(Category.FacialCare, "Уход за лицом")]
    [InlineData(Category.Waxing, "Восковая депиляция")]
    public void ToDisplayName_ReturnsExpectedRussianName(Category category, string expectedDisplayName)
    {
        Assert.Equal(expectedDisplayName, category.ToDisplayName());
    }
}
