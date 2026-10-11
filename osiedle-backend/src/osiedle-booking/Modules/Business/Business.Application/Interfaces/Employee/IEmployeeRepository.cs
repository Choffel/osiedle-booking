using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.Entities;

namespace Business.Application.Intarfaces;

public interface IEmployeeRepository
{
    Task<Employee> CreateEmployeeAsync(Employee employee, CancellationToken ct = default);

    Task<Employee> UpdateEmployeeAsync(Employee employee, CancellationToken ct = default);

    Task<Employee> GetEmployeeByIdAsync(Employee employee, CancellationToken ct = default);
    
    Task<>
}