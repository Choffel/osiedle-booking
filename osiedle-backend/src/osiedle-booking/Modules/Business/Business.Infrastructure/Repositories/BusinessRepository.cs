using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
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

    public Task<BusinessAggregate> AddBusinessAsync(
        BusinessAggregate business, CancellationToken cancellationToken = default)
    {
         var add =  _context.Businesses.Add(business);

         return Task.FromResult(add.Entity);
    }

    public async Task<BusinessAggregate> GetBusinessByIdAsync(
        Guid businessId, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses
            .Include(b => b.OwnerId)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken);
        
        
        return business;
    }

    public async Task<bool> ExistsByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        var exist = await _context.Businesses.AnyAsync(b => b.OwnerId == ownerId, ct);
        
        return exist;
    }
         

    public async Task<BusinessAggregate> ArchiveBusinessByIdAsync(
        Guid businessId, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken);
        
        
        return business;
    }

    public async Task UpdateBusinessInfoAsync(BusinessAggregate business, CancellationToken ct = default)
    {
        var entry = _context.Entry(business);
        
        if (entry.State == EntityState.Detached)
        {
            _context.Businesses.Update(business);
        }
        
    }
}