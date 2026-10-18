using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.Interfaces.BookableService;
using Business.Domain.Entities;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Repositories;

public class BookableServiceRepository : IBookableServiceRepository
{
    private readonly BusinessDbContext _dbContext;

    public BookableServiceRepository(BusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Task <BookableService> GetByIdAsync(BookableServiceId id, CancellationToken ct)
    {
        return  _dbContext.BookableServices
            .AsNoTracking()
            .SingleOrDefaultAsync(bookableService => bookableService.Id == id, ct);
            
    }

    public async Task<ServiceShortInfoDto?> GetBasicInfoByIdAsync(BookableServiceId id, CancellationToken ct)
    {
        return await _dbContext.BookableServices
            .AsNoTracking()
            .Where(s => s.Id == id) 
            .Select(s => new ServiceShortInfoDto(
                s.Id.Value,         
                s.EmployeeId.Value,  
                s.ServiceName,
                s.Price,
                s.Duration
            ))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ServiceShortInfoDto>> GetShortInfosByEmployeeIdAsync(EmployeeId employeeId, CancellationToken ct)
    {
        return await _dbContext.BookableServices
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId)
            .Select(s => new ServiceShortInfoDto(
                s.Id.Value,
                s.EmployeeId.Value,
                s.ServiceName,
                s.Price,
                s.Duration
            ))
            .ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(BookableServiceId id, CancellationToken ct)
    {
        return _dbContext.BookableServices
            .AsNoTracking()
            .AnyAsync(s => s.Id == id, ct);
    }

    public Task<bool> BelongsToEmployeeAsync(BookableServiceId id, EmployeeId employeeId, CancellationToken ct)
    {
        return _dbContext.BookableServices
            .AsNoTracking()
            .AnyAsync(s => s.Id == id && s.EmployeeId == employeeId, ct);
    }

    public void Add(BookableService service, CancellationToken ct)
    {
        _dbContext.BookableServices.Add(service);
    }

    public void Update(BookableService service, CancellationToken ct)
    {
        _dbContext.BookableServices.Update(service);
    }

    public void Delete(BookableService service, CancellationToken ct)
    {
        _dbContext.BookableServices.Remove(service);
    }
}
