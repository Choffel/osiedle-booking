using Business.Domain.ValueObjects.Category;

namespace Business.Domain.Tests.ValueObjects;

public class CategoryValueObjectsTests
{
    // ---------- Slug ----------

    [Theory]
    [InlineData("manicure", "manicure")]
    [InlineData("  Hair Cut  ", "hair-cut")]
    [InlineData("ALREADY-UPPER", "already-upper")]
    [InlineData("two   spaces", "two-spaces")]
    [InlineData("already-good", "already-good")]
    [InlineData("nail-2", "nail-2")]
    public void Slug_Create_NormalizesInput(string input, string expected)
    {
        Assert.Equal(expected, Slug.Create(input).Value);
        Assert.Equal(expected, Slug.From(input).Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Slug_Create_WithEmptyValue_Throws(string? value)
    {
        Assert.Throws<ArgumentException>(() => Slug.Create(value!));
    }

    [Theory]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    [InlineData("two--hyphens")]
    [InlineData("under_score")]
    [InlineData("dot.slug")]
    [InlineData("кириллица")]
    public void Slug_Create_WithInvalidFormat_Throws(string value)
    {
        Assert.Throws<ArgumentException>(() => Slug.Create(value));
    }

    [Fact]
    public void Slug_Create_With100Chars_Succeeds()
    {
        Assert.Equal(100, Slug.Create(new string('a', 100)).Value.Length);
    }

    [Fact]
    public void Slug_Create_With101Chars_Throws()
    {
        Assert.Throws<ArgumentException>(() => Slug.Create(new string('a', 101)));
    }

    [Fact]
    public void Slug_ToString_ReturnsValue()
    {
        Assert.Equal("hair-cut", Slug.Create("Hair Cut").ToString());
        // A default-constructed struct must not produce "Slug { Value = }" noise
        // in logs/DTOs (same bug class as the Guid value objects fixed earlier).
        Assert.Equal(string.Empty, default(Slug).ToString());
    }

    // ---------- CategoryId ----------

    [Fact]
    public void CategoryId_New_CreatesUniqueIdentifiers()
    {
        Assert.NotEqual(CategoryId.New(), CategoryId.New());
        Assert.NotEqual(Guid.Empty, CategoryId.New().Value);
    }

    [Fact]
    public void CategoryId_From_WithEmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(() => CategoryId.From(Guid.Empty));
    }

    [Fact]
    public void CategoryId_From_KeepsProvidedValue()
    {
        var value = Guid.NewGuid();

        Assert.Equal(value, CategoryId.From(value).Value);
    }

    [Fact]
    public void CategoryId_FromNullable_HandlesMissingValues()
    {
        Assert.Null(CategoryId.FromNullable(null));
        Assert.Null(CategoryId.FromNullable(Guid.Empty));

        var value = Guid.NewGuid();
        Assert.Equal(value, CategoryId.FromNullable(value)!.Value.Value);
    }

    [Fact]
    public void CategoryId_ImplicitConversionAndToString_UseGuidValue()
    {
        var id = CategoryId.New();
        Guid raw = id;

        Assert.Equal(id.Value, raw);
        Assert.Equal(id.Value.ToString(), id.ToString());
    }
}
