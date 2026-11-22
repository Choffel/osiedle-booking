using Business.Application.DTOs;
using Business.Application.DTOs.BusinessDTOs;
using Business.Application.ExtensionMethods;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.Services.BusinessServices;

public class BusinessService : IBusinessManagementService
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private const int DefaultPageSize = 3;
    private const int MaxPageSize = 50;

    public BusinessService(IBusinessRepository businessRepository, IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BusinessResponseDto>> CreateBusinessAsync(CreateBusinessDto createBusinessDto, CancellationToken cancellationToken = default)
    {
        var ownerId = UserId.From(createBusinessDto.OwnerId);
        
        var existingBusiness = await _businessRepository.ExistsByOwnerIdAsync(
            ownerId, 
            cancellationToken);
        
        if (existingBusiness)
        {
            return Result<BusinessResponseDto>.Failure(BusinessErrors.AlreadyExists);
        }
        
        if (!CategoryExtensions.TryFromSlug(createBusinessDto.Category, out var category))
        {
            return Result<BusinessResponseDto>.Failure(BusinessErrors.InvalidCategory);
        }

        var location = Location.Create(createBusinessDto.Location.City, createBusinessDto.Location.Street);

        var business = Domain.Aggregates.Business.Create(
            createBusinessDto.Name, 
            ownerId, 
            createBusinessDto.Description, 
            location,
            category
            );
        
        _businessRepository.Add(business, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return business.ToResponseDto();
    }

    public async Task<Result<BusinessResponseDto>> GetBusinessByIdAsync(BusinessId businessId, CancellationToken cancellationToken = default)
    {
        var business = await _businessRepository.GetBusinessByIdAsync(businessId, cancellationToken);
        if (business == null)
            return BusinessErrors.NotFound;

        return business.ToResponseDto();
    }

    public async Task<Result<PagedResult<BusinessShortInfoDto>>> GetBusinessesAsync(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? city = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var pagedBusinesses = await _businessRepository.GetBusinessesAsync(
            normalizedPage,
            normalizedPageSize,
            city,
            cancellationToken);

        return Result.Success(pagedBusinesses);
    }

    public async Task<Result<BusinessResponseDto>> UpdateBusinessInfoAsync(UserId userId, BusinessId businessId,
        UpdateBusinessInfoDto updateBusinessInfoDto, CancellationToken cancellationToken = default)
    {
        var business = await _businessRepository.GetBusinessByIdAsync(businessId, cancellationToken);

        if (business is null)
        {
            return Result<BusinessResponseDto>.Failure(BusinessErrors.NotFound);
        }
        if (business.OwnerId != userId)
            return Result<BusinessResponseDto>.Failure(BusinessErrors.Forbidden);
        
        var newLocation = Location.Create(updateBusinessInfoDto.NewLocation.City, updateBusinessInfoDto.NewLocation.Street);
        business.UpdateInfo(updateBusinessInfoDto.NewName, updateBusinessInfoDto.NewDescription, newLocation);
        
        if (!CategoryExtensions.TryFromSlug(updateBusinessInfoDto.NewCategory, out var category))
        {
            return Result<BusinessResponseDto>.Failure(BusinessErrors.InvalidCategory);
        }

        business.ChangeCategory(category);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return business.ToResponseDto();
    }

    public async Task<Result<BusinessResponseDto>> ArchiveBusinessAsync(UserId userId, BusinessId businessId, CancellationToken cancellationToken = default)
    {
        var business = await _businessRepository.GetBusinessByIdAsync(businessId, cancellationToken);

        if (business is null)
            return BusinessErrors.NotFound;
        if (business.OwnerId != userId)
            return Result<BusinessResponseDto>.Failure(BusinessErrors.Forbidden);

        business.ArchiveBusiness();
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return business.ToResponseDto();
    }
}
