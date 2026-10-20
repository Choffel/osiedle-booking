using Business.Application.DTOs;
using Business.Application.ExtensionMethods;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services;

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
        var existingBusiness = await _businessRepository.ExistsByOwnerIdAsync(
            createBusinessDto.OwnerId, 
            cancellationToken);
        
        if (existingBusiness)
        {
            return Result<BusinessResponseDto>.Failure(BusinessErrors.AlreadyExists);
        }
        
        var business = Domain.Aggregates.Business.Create(
            createBusinessDto.Name, 
            createBusinessDto.OwnerId, 
            createBusinessDto.Description, 
            createBusinessDto.Location);
        
        _businessRepository.Add(business, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return business.ToResponseDto();
    }

    public async Task<Result<BusinessResponseDto>> GetBusinessByIdAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var businessIdValue = BusinessId.From(businessId);
        
        var business = await _businessRepository.GetBusinessByIdAsync(businessIdValue, cancellationToken);
        if (business == null)
            return BusinessErrors.NotFound;

        return business.ToResponseDto();
    }

    public async Task<Result<BusinessResponseDto>> UpdateBusinessInfoAsync(UpdateBusinessInfoDto updateBusinessInfoDto,
        CancellationToken cancellationToken = default)
    {
        var businessId = BusinessId.From(updateBusinessInfoDto.BusinessId);
        var business = await _businessRepository.GetBusinessByIdAsync(businessId, cancellationToken);

        if (business is null)
        {
            return Result<BusinessResponseDto>.Failure(BusinessErrors.NotFound);
        }
        
        business.UpdateInfo(updateBusinessInfoDto.NewName, updateBusinessInfoDto.NewDescription, updateBusinessInfoDto.NewLocation);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return business.ToResponseDto();
    }

    public async Task<Result<BusinessResponseDto>> ArchiveBusinessAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var id = BusinessId.From(businessId);
        
        var business = await _businessRepository.GetBusinessByIdAsync(id, cancellationToken);

        if (business is null)
            return BusinessErrors.NotFound;

        
        business.ArchiveBusiness();
        
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return  business.ToResponseDto();
    }
}