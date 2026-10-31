using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.DTOs.WorkingHoursDTOs;
using Business.Domain.Entities;
using Business.Domain.ValueObjects;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeWorkingHoursRepository
{
    Task<IReadOnlyCollection<WorkingHoursDto>> GetWorkingHoursForRangeAsync(
        WorkingHoursId workingHoursId,
        EmployeeId employeeId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken ct);

 
    Task<WorkingHours?> GetWorkingHoursByDateAsync(
        WorkingHoursId workingHoursId,
        EmployeeId employeeId, 
        DateOnly date, 
        CancellationToken ct);
    
    
    void AddWorkingHours(WorkingHours workingHours);
    
    void RemoveWorkingHours(WorkingHours workingHours);
}