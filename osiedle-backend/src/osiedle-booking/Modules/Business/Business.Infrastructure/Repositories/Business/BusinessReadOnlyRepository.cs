using Business.Application.DTOs.BusinessDTOs;
using Business.Application.Interfaces.Business;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Repositories.Business;

//For UI
public class BusinessReadOnlyRepository : IBusinessReadOnlyRepository
{
    private readonly BusinessDbContext _dbContext;
    
    public BusinessReadOnlyRepository(BusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<BusinessShortInfoDto?> GetShortInfoAsync(BusinessId businessId, CancellationToken ct = default)
    {
        return await _dbContext.Businesses
            .AsNoTracking()
            .Where(b => b.Id == businessId)
            .Select(b => new BusinessShortInfoDto(
                b.Id.Value,
                b.OwnerId.Value,
                b.Name,
                b.Description,
                b.Location.ToString()))
            .FirstOrDefaultAsync(ct);
    }
}