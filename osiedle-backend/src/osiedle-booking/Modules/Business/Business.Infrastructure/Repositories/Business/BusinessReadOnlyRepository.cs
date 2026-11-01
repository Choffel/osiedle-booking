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
        var business = await _dbContext.Businesses
            .AsNoTracking()
            .Where(b => b.Id == businessId)
            .Select(b => new
            {
                b.Id,
                b.OwnerId,
                b.Name,
                b.Description,
                b.Location
            })
            .FirstOrDefaultAsync(ct);

        return business is null
            ? null
            : new BusinessShortInfoDto(
                business.Id.Value,
                business.OwnerId.Value,
                business.Name,
                business.Description,
                business.Location.ToString());
    }
}