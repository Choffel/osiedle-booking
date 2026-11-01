using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.DTOs.WorkingHoursDTOs;
using Business.Application.Interfaces.Employee;
using Business.Domain.Entities;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Repositories.Employee;

public class EmployeeWorkingHoursRepository : IEmployeeWorkingHoursRepository
{
    private readonly BusinessDbContext _dbContext;
    
    public EmployeeWorkingHoursRepository(BusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    
    public async Task<WorkingHours?> GetByDateAsync(EmployeeId employeeId, DateOnly date, CancellationToken ct = default)
    {
        return await _dbContext.WorkingHours
            .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.Date == date, ct);
    }

    public async Task<IReadOnlyCollection<WorkingHoursDto>> GetForRangeAsync(
        EmployeeId employeeId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken ct = default)
    {
        return await _dbContext.WorkingHours
            .AsNoTracking()
            .Where(w => w.EmployeeId == employeeId 
                        && w.Date >= startDate 
                        && w.Date <= endDate)
            .Select(w => new WorkingHoursDto(
                w.Date,
                w.Slot))
            .ToListAsync(ct);
    }
    
    public void Add(WorkingHours workingHours)
    {
        _dbContext.WorkingHours.Add(workingHours);
    }
    
    public void Remove(WorkingHours workingHours)
    {
        _dbContext.WorkingHours.Remove(workingHours);
    }
}