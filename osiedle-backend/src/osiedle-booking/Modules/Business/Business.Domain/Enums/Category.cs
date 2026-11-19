namespace Business.Domain.Enums;

public enum Category
{
    Haircut = 1,
    BeardTrim = 2,
    Shaving = 3,
    Styling = 4,
    Coloring = 5,
    HairCare = 6,
    HairWash = 7,
    KidsHaircut = 8,
    Combo = 9,
    FacialCare = 10,
    Waxing = 11
}
public static class CategoryExtensions
{
    public static string ToDisplayName(this Category category) => category switch
    {
        Category.Haircut => "Haircut",
        Category.BeardTrim => "Beard Trim",
        Category.Shaving => "Shaving",
        Category.Styling => "Styling",
        Category.Coloring => "Coloring",
        Category.HairCare => "Hair Care",
        Category.HairWash => "Hair Wash",
        Category.KidsHaircut => "Kids Haircut",
        Category.Combo => "Combo",
        Category.FacialCare => "Facial Care",
        Category.Waxing => "Waxing",
        _ => category.ToString()
    };
}
