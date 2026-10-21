using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.Entities;
using Business.Domain.ValueObjects;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeWorkingHoursRepository
{
    Task<IReadOnlyCollection<WorkingHoursDto>> GetWorkingHoursForRangeAsync(
        EmployeeId employeeId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken ct);

 
    Task<WorkingHours?> GetWorkingHoursByDateAsync(
        EmployeeId employeeId, 
        DateOnly date, 
        CancellationToken ct);
    
    
    void AddWorkingHours(WorkingHours workingHours);
    
    void UpdateWorkingHours(WorkingHours workingHours);
    
    void RemoveWorkingHours(WorkingHours workingHours);
}