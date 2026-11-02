using Business.Application.DTOs;
using Business.Application.DTOs.BusinessDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Business;

public interface IBusinessManagementService
{
    Task<Result<BusinessResponseDto>> CreateBusinessAsync(CreateBusinessDto dto, CancellationToken ct = default);
    Task<Result<BusinessResponseDto>> GetBusinessByIdAsync(Guid businessId, CancellationToken ct = default);
    Task<Result<BusinessResponseDto>> UpdateBusinessInfoAsync(Guid businessId, UpdateBusinessInfoDto dto, CancellationToken ct = default);
    Task<Result<BusinessResponseDto>> ArchiveBusinessAsync(Guid businessId, CancellationToken ct = default);
}
