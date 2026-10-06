
using Business.Application.DTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces;

public interface IBusinessRepository
{
    Task<Result<Domain.Aggregates.Business>> AddBusinessAsync(Domain.Aggregates.Business business, CancellationToken ct = default);
    Task<Result<Domain.Aggregates.Business>> GetBusinessByIdAsync(Guid businessId, CancellationToken ct = default);
    Task<Result<Domain.Aggregates.Business>> ArchiveBusinessByIdAsync(Guid businessId, CancellationToken ct = default);
}
