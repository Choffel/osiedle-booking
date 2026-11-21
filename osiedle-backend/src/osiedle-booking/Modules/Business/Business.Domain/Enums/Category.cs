using System.Collections.Frozen;
using System.Text.RegularExpressions;

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
public static partial class CategoryExtensions
{
    private static readonly FrozenDictionary<Category, string> DisplayNames;
    private static readonly FrozenDictionary<Category, string> Slugs;
    private static readonly FrozenDictionary<string, Category> CategoriesBySlug;

    static CategoryExtensions()
    {
        var values = Enum.GetValues<Category>();

        DisplayNames = values.ToFrozenDictionary(
            c => c,
            c => SplitCamelCaseRegex().Replace(c.ToString(), " ")
        );

        Slugs = values.ToFrozenDictionary(
            c => c,
            c => SplitCamelCaseRegex().Replace(c.ToString(), "-").ToLowerInvariant()
        );

        CategoriesBySlug = Slugs.ToFrozenDictionary(
            kvp => kvp.Value,
            kvp => kvp.Key,
            StringComparer.OrdinalIgnoreCase
        );
    }

    public static string ToDisplayName(this Category category) =>
        DisplayNames.TryGetValue(category, out var name)
            ? name
            : throw new ArgumentOutOfRangeException(nameof(category), category, null);

    public static string ToSlug(this Category category) =>
        Slugs.TryGetValue(category, out var slug)
            ? slug
            : throw new ArgumentOutOfRangeException(nameof(category), category, null);

    public static bool TryFromSlug(string? slug, out Category category)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            category = default;
            return false;
        }

        return CategoriesBySlug.TryGetValue(slug.Trim(), out category);
    }

    [GeneratedRegex("(?<=[a-z0-9])(?=[A-Z])")]
    private static partial Regex SplitCamelCaseRegex();
}
