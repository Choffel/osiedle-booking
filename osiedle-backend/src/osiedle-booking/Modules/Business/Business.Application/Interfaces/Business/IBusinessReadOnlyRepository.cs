using Business.Application.DTOs.BusinessDTOs;
using Business.Domain.ValueObjects;

namespace Business.Application.Interfaces.Business;

public interface IBusinessReadOnlyRepository
{
    Task<BusinessShortInfoDto?> GetShortInfoAsync(BusinessId businessId, CancellationToken ct = default);
}