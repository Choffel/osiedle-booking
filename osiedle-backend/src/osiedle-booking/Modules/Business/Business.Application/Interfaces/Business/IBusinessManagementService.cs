using Business.Application.DTOs;
using Business.Application.DTOs.BusinessDTOs;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Business;

public interface IBusinessManagementService
{
    Task<Result<BusinessResponseDto>> CreateBusinessAsync(CreateBusinessDto dto, CancellationToken ct = default);
    Task<Result<BusinessResponseDto>> GetBusinessByIdAsync(BusinessId businessId, CancellationToken ct = default);
    Task<Result<BusinessResponseDto>> UpdateBusinessInfoAsync(BusinessId businessId, UpdateBusinessInfoDto dto, CancellationToken ct = default);
    Task<Result<BusinessResponseDto>> ArchiveBusinessAsync(BusinessId businessId, CancellationToken ct = default);
}