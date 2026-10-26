using Business.Application.Interfaces;
using Business.Infrastructure.Persistence;

namespace Business.Infrastructure.UnitOtWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly BusinessDbContext _dbContext;

    public UnitOfWork(BusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}