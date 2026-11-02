using Business.Application.DTOs.BusinessDTOs;
using Business.Application.Interfaces.Business;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks.ValueObjects;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Infrastructure.Repositories.Business;

public class BusinessRepository : IBusinessRepository
{
    private readonly BusinessDbContext _context;
    public BusinessRepository(BusinessDbContext context)
    {
        _context = context;
    }
    
    public async Task<BusinessAggregate?> GetByIdAsync(BusinessId businessId, CancellationToken ct = default)
    {
        return await _context.Businesses
            .Include(b => b.Services)
            .FirstOrDefaultAsync(b => b.Id == businessId, ct);
    }

    public async Task<bool> ExistsByIdAsync(BusinessId businessId, CancellationToken ct = default)
    {
        return await _context.Businesses
            .AnyAsync(b => b.Id == businessId, ct);
    }

    public async Task<bool> ExistsByOwnerIdAsync(UserId userId, CancellationToken ct = default)
    {
        return await _context.Businesses
            .AnyAsync(b => b.OwnerId == userId, ct);
    }

    public void Add(BusinessAggregate business)
    {
        _context.Businesses.Add(business);
    }
}