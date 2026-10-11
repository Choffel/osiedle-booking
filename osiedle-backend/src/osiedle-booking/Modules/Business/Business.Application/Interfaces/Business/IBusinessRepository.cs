using Business.Application.DTOs;

namespace Business.Application.Interfaces;

public interface IBusinessRepository
{
    Task<Domain.Aggregates.Business> AddBusinessAsync(Domain.Aggregates.Business business, CancellationToken ct = default);
    Task<Domain.Aggregates.Business> GetBusinessByIdAsync(Guid businessId, CancellationToken ct = default);
    Task<bool> ExistsByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<Domain.Aggregates.Business> ArchiveBusinessByIdAsync(Guid businessId, CancellationToken ct = default);
    
    Task UpdateBusinessInfoAsync(Domain.Aggregates.Business business, CancellationToken ct = default);
}
