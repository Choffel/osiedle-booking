using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.ValueObjects;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeRepository
{
    Task<Domain.Aggregates.Employee> GetEmployeeByIdAsync(EmployeeId employeeId, CancellationToken ct = default);
    
    Task<bool> ExistsEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default);
    
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    
    Task<EmployeeStatusDto?> GetStatusAsync(EmployeeId employeeId, CancellationToken ct = default);
    
    Task<EmployeeShortInfoDto?> GetShortInfoAsync(EmployeeId employeeId, CancellationToken ct = default);
    
    void AddEmployee(Domain.Aggregates.Employee employee);
}
