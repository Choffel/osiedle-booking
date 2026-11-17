using Business.Application.DTOs.Category;
using Business.Application.Interfaces.Category.Services;
using Business.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Business.Integration.Tests;

/// <summary>
/// End-to-end flows through CategoryService with real EF repositories and a real
/// (SQLite) unique index — the layer where duplicate checks, slug normalization and
/// the soft-delete query filter actually interact.
/// </summary>
public sealed class CategoryFlowTests : IntegrationTestBase
{
    private static CategoryCreateRequestDTO Req(string name, string slug, int order = 1) =>
        new(name, slug, order);

    private static async Task<osiedle_booking.BuildingBlocks.Result> CreateAsync(
        ICategoryService service, string name, string slug, int order = 1) =>
        await service.CreateAsync(Req(name, slug, order));

    // ---------- creation ----------

    [Fact]
    public async Task Create_PersistsNormalizedCategory()
    {
        using var scope = CreateScope();
        var service = Service(scope);

        var result = await CreateAsync(service, "  Manicure Sets  ", "Manicure Sets", 3);

        Assert.True(result.IsSuccess);

        using var dbScope = CreateScope();
        var row = await Db(dbScope).Categories.SingleAsync();
        Assert.Equal("Manicure Sets", row.Name);
        Assert.Equal("manicure-sets", row.Slug.Value);
        Assert.Equal(3, row.DisplayOrder);
        Assert.Null(row.DeletedAt);
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsAlreadyExists()
    {
        using var scope = CreateScope();
        var service = Service(scope);
        Assert.True((await CreateAsync(service, "Manicure", "manicure", 1)).IsSuccess);

        var result = await CreateAsync(service, "Manicure", "manicure-2", 2);

        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.AlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task Create_DuplicateSlugAfterNormalization_ReturnsAlreadyExists()
    {
        using var scope = CreateScope();
        var service = Service(scope);
        Assert.True((await CreateAsync(service, "Hair salon", "Hair-Cut", 1)).IsSuccess);

        // "hair cut" normalizes to the very same slug "hair-cut".
        var result = await CreateAsync(service, "Barber", "hair cut", 2);

        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.AlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task Create_DuplicateNameDifferingOnlyByCase_ReturnsAlreadyExists()
    {
        using var scope = CreateScope();
        var service = Service(scope);
        Assert.True((await CreateAsync(service, "Manicure", "manicure", 1)).IsSuccess);

        // Same visible name for a salon catalog customer — must not create a twin.
        var result = await CreateAsync(service, "manicure", "manicure-2", 2);

        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.AlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task Create_DuplicateNameWithSurroundingWhitespace_ReturnsAlreadyExistsNotError()
    {
        using var scope = CreateScope();
        var service = Service(scope);
        Assert.True((await CreateAsync(service, "Manicure", "manicure", 1)).IsSuccess);

        // The duplicate check sees the raw string, the aggregate stores it trimmed —
        // " Manicure " must be recognized as a duplicate, not crash on the unique index.
        var result = await CreateAsync(service, "  Manicure  ", "manicure-2", 2);

        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.AlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task Create_RecreatingNameOfArchivedCategory_ReturnsAlreadyExistsNotError()
    {
        using var scope = CreateScope();
        var service = Service(scope);
        Assert.True((await CreateAsync(service, "Manicure", "manicure", 1)).IsSuccess);
        Assert.True((await service.ArchiveBySlug(Business.Domain.ValueObjects.Category.Slug.Create("manicure"))).IsSuccess);

        // Archived rows are hidden by the query filter but still occupy the unique
        // index, so this insert hits it — must surface as a clean conflict, not a 500.
        var result = await CreateAsync(service, "Manicure", "manicure-new", 2);

        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.AlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task Create_InvalidSlug_ThrowsArgumentExceptionAndPersistsNothing()
    {
        using var scope = CreateScope();
        var service = Service(scope);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateAsync(service, "Nails", "!!!"));

        using var dbScope = CreateScope();
        Assert.Equal(0, await Db(dbScope).Categories.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task Create_NameTooLong_ThrowsArgumentExceptionAndPersistsNothing()
    {
        using var scope = CreateScope();
        var service = Service(scope);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateAsync(service, new string('a', 101), "long-name"));

        using var dbScope = CreateScope();
        Assert.Equal(0, await Db(dbScope).Categories.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task Create_EmptyName_ThrowsArgumentException()
    {
        using var scope = CreateScope();
        var service = Service(scope);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateAsync(service, "   ", "some-slug"));
    }

    [Fact]
    public async Task Create_NegativeDisplayOrder_ThrowsArgumentOutOfRangeException()
    {
        using var scope = CreateScope();
        var service = Service(scope);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CreateAsync(service, "Manicure", "manicure", -1));
    }

    // ---------- listing ----------

    [Fact]
    public async Task GetCategories_OrdersByDisplayOrderZeroLast_ThenName()
    {
        using var scope = CreateScope();
        var service = Service(scope);
        Assert.True((await CreateAsync(service, "Bravo", "bravo", 2)).IsSuccess);
        Assert.True((await CreateAsync(service, "Unordered", "unordered", 0)).IsSuccess);
        Assert.True((await CreateAsync(service, "Alpha", "alpha", 1)).IsSuccess);
        Assert.True((await CreateAsync(service, "Charlie", "charlie", 1)).IsSuccess);

        var result = await service.GetCategoriesAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { "Alpha", "Charlie", "Bravo", "Unordered" },
            result.Value!.Select(c => c.Name).ToArray());
    }

    [Fact]
    public async Task GetCategories_EmptyCatalog_ReturnsNotFound()
    {
        using var scope = CreateScope();
        var service = Service(scope);

        // Documents current behaviour: a fresh salon with no categories answers
        // 404 instead of an empty list — flagged for a product decision.
        var result = await service.GetCategoriesAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.NotFound.Code, result.Error.Code);
    }

    // ---------- archiving ----------

    [Fact]
    public async Task Archive_UnknownSlug_ReturnsNotFound()
    {
        using var scope = CreateScope();
        var service = Service(scope);

        var result = await service.ArchiveBySlug(Business.Domain.ValueObjects.Category.Slug.Create("does-not-exist"));

        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task Archive_HidesCategoryFromListing_AndSecondArchiveReturnsNotFound()
    {
        using var scope = CreateScope();
        var service = Service(scope);
        Assert.True((await CreateAsync(service, "Pedicure", "pedicure", 1)).IsSuccess);

        Assert.True((await service.ArchiveBySlug(Business.Domain.ValueObjects.Category.Slug.Create("pedicure"))).IsSuccess);

        using var listScope = CreateScope();
        var listing = await Service(listScope).GetCategoriesAsync();
        Assert.False(listing.IsSuccess); // nothing left in this test's database

        // The archived row itself must be gone from reads...
        using var dbScope = CreateScope();
        Assert.Equal(0, await Db(dbScope).Categories.CountAsync());
        Assert.Equal(1, await Db(dbScope).Categories.IgnoreQueryFilters().CountAsync());
        Assert.NotNull((await Db(dbScope).Categories.IgnoreQueryFilters().SingleAsync()).DeletedAt);

        // ...so archiving again cannot find it (no 500 from the double-archive guard).
        var second = await Service(listScope).ArchiveBySlug(Business.Domain.ValueObjects.Category.Slug.Create("pedicure"));
        Assert.False(second.IsSuccess);
        Assert.Equal(CategoryErrors.NotFound.Code, second.Error.Code);
    }
}
