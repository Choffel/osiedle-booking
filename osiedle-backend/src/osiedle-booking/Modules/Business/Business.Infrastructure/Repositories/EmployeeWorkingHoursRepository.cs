using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Domain.Entities;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Business.Infrastructure.Repositories;

public class EmployeeWorkingHoursRepository : IEmployeeWorkingHoursRepository
{
    private readonly BusinessDbContext _dbContext;
    
    public EmployeeWorkingHoursRepository(BusinessDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async  Task<IReadOnlyCollection<WorkingHoursDto>> GetWorkingHoursForRangeAsync(EmployeeId employeeId, DateOnly startDate, DateOnly endDate,
        CancellationToken ct = default)
    {
        return await _dbContext.WorkingHours
            .AsNoTracking()
            .Where(w => w.EmployeeId == employeeId
                        && w.Date >= startDate
                        && w.Date <= endDate)
            .Select(w => new WorkingHoursDto(
                w.Date,
                w.StartTime,
                w.EndTime))
            .ToListAsync(ct);
    }

    public async  Task<WorkingHours?> GetWorkingHoursByDateAsync(EmployeeId employeeId, DateOnly date, CancellationToken ct = default)
    {
        return await _dbContext.WorkingHours
            .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.Date == date, ct);
    }

    public void AddWorkingHours(WorkingHours workingHours)
    {
        _dbContext.WorkingHours.Add(workingHours);
    }

    public void RemoveWorkingHours(WorkingHours workingHours)
    {
        _dbContext.WorkingHours.Remove(workingHours);
    }
}