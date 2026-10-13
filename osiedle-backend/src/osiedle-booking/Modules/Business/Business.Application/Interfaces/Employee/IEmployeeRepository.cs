using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeRepository
{
    Task<Domain.Aggregates.Employee> GetEmployeeByIdAsync(EmployeeId employeeId, CancellationToken ct = default);
    
    Task<bool> ExistsEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default);
    
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    
    
    void AddEmployee(Domain.Aggregates.Employee employee);
    void UpdateEmployee(Domain.Aggregates.Employee employee);
    void DeleteEmployee(Domain.Aggregates.Employee employee);
}