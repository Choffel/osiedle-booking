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
//TODO: Chenge to english/polish or delete 
public static class CategoryExtensions
{
    public static string ToDisplayName(this Category category) => category switch
    {
        Category.Haircut => "Стрижка",
        Category.BeardTrim => "Стрижка бороды",
        Category.Shaving => "Бритье",
        Category.Styling => "Укладка",
        Category.Coloring => "Окрашивание",
        Category.HairCare => "Уход за волосами",
        Category.HairWash => "Мытье головы",
        Category.KidsHaircut => "Детская стрижка",
        Category.Combo => "Комплекс",
        Category.FacialCare => "Уход за лицом",
        Category.Waxing => "Восковая депиляция",
        _ => category.ToString()
    };
}
