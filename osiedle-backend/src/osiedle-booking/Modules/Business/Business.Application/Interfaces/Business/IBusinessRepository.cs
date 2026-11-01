using Business.Application.DTOs.BusinessDTOs;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.Interfaces.Business;

public interface IBusinessRepository
{
    Task<Domain.Aggregates.Business?> GetByIdAsync(BusinessId businessId, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(BusinessId businessId, CancellationToken ct = default);
    Task<bool> ExistsByOwnerIdAsync(UserId userId, CancellationToken ct = default);
    void Add(Domain.Aggregates.Business business);
}
