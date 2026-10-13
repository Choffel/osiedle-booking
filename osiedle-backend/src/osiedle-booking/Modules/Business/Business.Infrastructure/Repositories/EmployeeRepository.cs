using Business.Application.Interfaces.Employee;
using Business.Domain.Aggregates;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Business.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly BusinessDbContext _context;
    
    public EmployeeRepository(BusinessDbContext context)
    {
        _context = context;
    }


    public Task<Employee?> GetEmployeeByIdAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        return _context.Employees
            .AsNoTracking()
            .Include(e => e.BusinessServices)
            .SingleOrDefaultAsync(e => e.Id == employeeId, ct);
    }

    public Task<bool> ExistsEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        return _context.Employees
            .AsNoTracking()
            .AnyAsync(e => e.Id == employeeId, ct);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return _context.Employees
            .AsNoTracking()
            .AnyAsync(e => e.Email == email.ToLower().Trim(), ct);
    }

    public void AddEmployee(Employee employee)
    {
        _context.Employees.Add(employee);
    }

    public void UpdateEmployee(Employee employee)
    {
        _context.Employees.Update(employee);
    }

    public void DeleteEmployee(Employee employee)
    {
        _context.Employees.Remove(employee);
    }
}