
using Business.Application.DTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces;

public interface IBusinessRepository
{
    Task<Result<Domain.Aggregates.Business>> AddBusinessAsync(Guid businessId, CancellationToken cancellationToken = default);
    
    Task<Result<Domain.Aggregates.Business>> GetBusinessByIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    
    Task<Result<Domain.Aggregates.Business>> UpdateBusinessInfoAsync(UpdateBusinessInfoDto updateBusinessInfoDto, CancellationToken cancellationToken = default);
    
    Task<Result<Domain.Aggregates.Business>> ArchiveBusinessByIdAsync(Guid businessId, CancellationToken cancellationToken = default);
}