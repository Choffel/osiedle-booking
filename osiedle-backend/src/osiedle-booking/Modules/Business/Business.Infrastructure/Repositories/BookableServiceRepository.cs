using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.Interfaces.BookableService;
using Business.Domain.Entities;
using Business.Domain.ValueObject;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks;

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
            .Where(s => s.Id == id) // 1. Фильтруем по Strongly Typed ID прямо в БД
            .Select(s => new ServiceShortInfoDto(
                s.Id.Value,          // 2. Распаковываем Value Object в Guid для DTO
                s.EmployeeId.Value,  // 3. Распаковываем Value Object в Guid для DTO
                s.ServiceName,
                s.Price,
                s.Duration
            ))
            .SingleOrDefaultAsync(ct);
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
