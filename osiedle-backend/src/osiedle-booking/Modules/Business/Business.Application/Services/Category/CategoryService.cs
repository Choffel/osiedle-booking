using Business.Application.DTOs.Category;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Category.Repositories;
using Business.Application.Interfaces.Category.Services;
using Business.Domain.Errors;
using Business.Domain.ValueObjects.Category;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services.Category;

public class CategoryService : ICategoryService
{
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly ICategoryReadRepository _categoryReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CategoryService(ICategoryWriteRepository categoryWriteRepository,
        ICategoryReadRepository categoryReadRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryWriteRepository = categoryWriteRepository;
        _categoryReadRepository = categoryReadRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result> CreateAsync(CategoryCreateRequestDTO request, CancellationToken ct = default)
    {
        if (await _categoryReadRepository.ValidateCategory(new CategoryInfoDTO(Guid.Empty, 
                request.Name, request.Slug, request.DisplayOrder), ct))
        {
            return Result.Failure(CategoryErrors.AlreadyExists);
        }
        
        var category = Domain.Aggregates.Category.Create(request.Name, request.Slug, request.DisplayOrder);
        _categoryWriteRepository.Add(category);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success();
    }
    
    public async Task<Result<IReadOnlyList<CategoryResponseDTO>>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await _categoryReadRepository.GetAllAsync(ct);

        if (categories.Count == 0)
            return Result<IReadOnlyList<CategoryResponseDTO>>.Failure(CategoryErrors.NotFound);
        
        var categoryDTOs = categories.Select(c =>
            new CategoryResponseDTO(c.Name, c.Slug, c.DisplayOrder)).ToList();
        
        return Result<IReadOnlyList<CategoryResponseDTO>>.Success(categoryDTOs);
    }

    public async Task<Result> ArchiveBySlug(Slug slug, CancellationToken ct = default)
    {
        var category = await _categoryReadRepository.GetCategoryBySlugAsync(slug, ct);
        if (category is null)
            return Result.Failure(CategoryErrors.NotFound);

        category.Archive();
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}