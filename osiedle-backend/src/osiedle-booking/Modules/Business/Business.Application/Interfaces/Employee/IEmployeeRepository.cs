using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.ValueObjects;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeRepository
{
    Task<Domain.Aggregates.Employee?> GetByIdAsync(EmployeeId employeeId, CancellationToken ct = default);
    
    Task<bool> ExistsEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default);
    
    void Add(Domain.Aggregates.Employee employee);
}
