using Business.Application.DTOs;
using Business.Application.DTOs.BusinessDTOs;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Business;

public interface IBusinessManagementService
{
    Task<Result<BusinessResponseDto>> CreateBusinessAsync(CreateBusinessDto createBusinessDto, CancellationToken cancellationToken);
    
    Task<Result<BusinessResponseDto>> GetBusinessByIdAsync(BusinessId businessId, CancellationToken cancellationToken);
    
    Task<Result<BusinessResponseDto>> UpdateBusinessInfoAsync(BusinessId businessId, UpdateBusinessInfoDto updateBusinessInfoDto, CancellationToken cancellationToken);
    
    Task<Result<BusinessResponseDto>> ArchiveBusinessAsync(BusinessId businessId, CancellationToken cancellationToken);
    
}