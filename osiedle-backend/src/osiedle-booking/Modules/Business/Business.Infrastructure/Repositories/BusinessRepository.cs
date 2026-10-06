using Business.Application.Interfaces;
using Business.Domain.Errors;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Infrastructure.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly ApplicationDbContext _context;

    public BusinessRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BusinessAggregate>> AddBusinessAsync(
        BusinessAggregate business, CancellationToken cancellationToken = default)
    {
        await _context.Businesses.AddAsync(business, cancellationToken);
        return Result<BusinessAggregate>.Success(business);
    }

    public async Task<Result<BusinessAggregate>> GetBusinessByIdAsync(
        Guid businessId, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken);

        return business is null
            ? Result<BusinessAggregate>.Failure(BusinessErrors.NotFound)
            : Result<BusinessAggregate>.Success(business);
    }

    public async Task<Result<BusinessAggregate>> ArchiveBusinessByIdAsync(
        Guid businessId, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken);

        if (business is null)
            return Result<BusinessAggregate>.Failure(BusinessErrors.NotFound);

        
        //domain method in repo
        var archiveResult = business.ArchiveBusiness();
            
        //todo: fix this
        if (archiveResult.IsFailure)
            return Result<BusinessAggregate>.Failure(archiveResult.Error);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<BusinessAggregate>.Success(business);
    }
}