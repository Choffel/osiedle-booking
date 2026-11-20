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
        
        if (!Enum.TryParse<Category>(createBusinessDto.Category, true, out var category) ||
            !Enum.IsDefined(category))
        {
            return Result<BusinessResponseDto>.Failure(BusinessErrors.InvalidCategory);
        }

        var business = Domain.Aggregates.Business.Create(
            createBusinessDto.Name, 
            ownerId, 
            createBusinessDto.Description, 
            createBusinessDto.Location,
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
        
        business.UpdateInfo(updateBusinessInfoDto.NewName, updateBusinessInfoDto.NewDescription, updateBusinessInfoDto.NewLocation);
        
        if (!Enum.TryParse<Category>(updateBusinessInfoDto.NewCategory, true, out var category) ||
            !Enum.IsDefined(category))
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
        return  business.ToResponseDto();
    }
}