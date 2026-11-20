using Business.Application.DTOs;
using Business.Application.DTOs.CategoryDTOs;
using Business.Application.ExtensionMethods;
using Business.Application.Interfaces.Categories.Services;
using Business.Application.Interfaces.Business;
using Business.Domain.Enums;
using Business.Domain.Errors;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services.CategoryServices;

public class CategoryService : ICategoryService
{
    private readonly IBusinessRepository _businessRepository;

    public CategoryService(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }

    public Task<Result<IReadOnlyList<CategoryResponseDTO>>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = Enum.GetValues<Domain.Enums.Category>()
            .Select(c => new CategoryResponseDTO(
                (int)c,
                c.ToString(),
                c.ToDisplayName(),
                c.ToSlug()))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CategoryResponseDTO>>.Success(categories));
    }

    public async Task<Result<IReadOnlyList<BusinessResponseDto>>> GetAllBusinessesByCategoryAsync(
        string category,
        CancellationToken ct = default)
    {
        if (!CategoryExtensions.TryFromSlug(category, out var parsedCategory))
        {
            return Result<IReadOnlyList<BusinessResponseDto>>.Failure(BusinessErrors.InvalidCategory);
        }

        var businesses = await _businessRepository.GetBusinessesByCategoryAsync(parsedCategory, ct);
        var result = businesses
            .Select(b => b.ToResponseDto())
            .ToList();

        return Result<IReadOnlyList<BusinessResponseDto>>.Success(result);
    }
}
