using Business.Application.Interfaces;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Infrastructure.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly ApplicationDbContext _context;

    public BusinessRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessAggregate> AddBusinessAsync(
        BusinessAggregate business, CancellationToken cancellationToken = default)
    {
        await _context.Businesses.AddAsync(business, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return business;
    }

    public async Task<BusinessAggregate> GetBusinessByIdAsync(
        Guid businessId, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return business;
    }

    public Task<bool> ExistsByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
        => _context.Businesses.AnyAsync(b => b.OwnerId == ownerId, ct);

    public async Task<BusinessAggregate> ArchiveBusinessByIdAsync(
        Guid businessId, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return business;
    }
}