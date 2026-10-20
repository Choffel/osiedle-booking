using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.Entities;
using Business.Domain.ValueObjects;

namespace Business.Application.Interfaces;

public interface IEmployeeWorkingHoursRepository
{
    Task<IReadOnlyCollection<WorkingHoursDto>> GetWorkingHoursForRangeAsync(
        EmployeeId employeeId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken ct = default);

 
    Task<WorkingHours?> GetWorkingHoursByDateAsync(
        EmployeeId employeeId, 
        DateOnly date, 
        CancellationToken ct = default);

    
    void AddWorkingHours(WorkingHours workingHours);
    
    void RemoveWorkingHours(WorkingHours workingHours);
}