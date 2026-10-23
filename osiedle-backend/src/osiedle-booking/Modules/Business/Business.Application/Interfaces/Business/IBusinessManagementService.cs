using Business.Application.DTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Business;

public interface IBusinessManagementService
{
    Task<Result<BusinessResponseDto>> CreateBusinessAsync(CreateBusinessDto createBusinessDto, CancellationToken cancellationToken = default);
    
    Task<Result<BusinessResponseDto>> GetBusinessByIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    
    Task<Result<BusinessResponseDto>> UpdateBusinessInfoAsync(Guid businessId, UpdateBusinessInfoDto updateBusinessInfoDto, CancellationToken cancellationToken = default);
    
    Task<Result<BusinessResponseDto>> ArchiveBusinessAsync(Guid businessId, CancellationToken cancellationToken = default);
    
}