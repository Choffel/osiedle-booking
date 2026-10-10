using Business.Infrastructure.Persistence;
using osiedle_booking.BuildingBlocks.Interfaces;

namespace Business.Infrastructure.UnitOtWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly BusinessDbContext _dbContext;

    public UnitOfWork(BusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Task SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}