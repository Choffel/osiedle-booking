using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.DTOs.WorkingHoursDTOs;
using Business.Domain.Entities;
using Business.Domain.ValueObjects;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeWorkingHoursRepository
{
    Task<WorkingHours?> GetByDateAsync(EmployeeId employeeId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyCollection<WorkingHoursDto>> GetForRangeAsync(
        EmployeeId employeeId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken ct = default);
    
    void Add(WorkingHours workingHours);
    
    void Remove(WorkingHours workingHours);
}