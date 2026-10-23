using Business.Application.DTOs.BusinessDTOs;
using Business.Application.Interfaces.Business;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Infrastructure.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly BusinessDbContext _context;
    public BusinessRepository(BusinessDbContext context)
    {
        _context = context;
    }
    
    
    public async Task<bool> ExistsByIdAsync(BusinessId businessId, CancellationToken ct = default)
    {
        return await _context.Businesses.AnyAsync(b => b.Id == businessId, ct);
    }

    public async Task<bool> ExistsByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        var exist = await _context.Businesses.AnyAsync(b => b.OwnerId == ownerId, ct);
        
        return exist;
    }

    public async Task<BusinessAggregate?> GetBusinessByIdAsync(BusinessId businessId, CancellationToken ct)
    {
        var business = await _context.Businesses
            .Include(s => s.Services)
            .FirstOrDefaultAsync(b => b.Id == businessId, ct);
        return business;
    }

    public Task<bool> EmployeeBelongsToBusinessAsync(
        EmployeeId employeeId, 
        BusinessId businessId, 
        CancellationToken ct = default)
    {
        return _context.Employees
            .AnyAsync(e => e.Id == employeeId && e.BusinessId == businessId, ct);
    }

    public Task<bool> BookableServiceBelongsToBusinessAsync(BookableServiceId serviceId, BusinessId businessId, CancellationToken ct)
    {
        return _context.Businesses
            .AnyAsync(b => b.Id == businessId && b.Services.Any(s => s.Id == serviceId), ct);
    }

    public Task<bool> BookableServiceBelongsToEmployeeAsync(BookableServiceId serviceId, EmployeeId employeeId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<BusinessShortInfoDto?> GetShortInfoAsync(BusinessId businessId, CancellationToken ct)
    {
        var businessDto = await _context.Businesses
            .AsNoTracking()
            .Where(b => b.Id == businessId)
            .Select(s => new BusinessShortInfoDto(
                s.Id.Value,
                s.OwnerId,
                s.Name,
                s.Description,
                s.Location
            ))
            .FirstOrDefaultAsync(ct);

        return businessDto;
    }

    public void Add(BusinessAggregate business, CancellationToken ct)
    {
        _context.Businesses.Add(business);
    }
}