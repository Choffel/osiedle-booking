using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces.Employee;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Repositories.Employee;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly BusinessDbContext _context;
    
    public EmployeeRepository(BusinessDbContext context)
    {
        _context = context;
    }
    
    public Task<Domain.Aggregates.Employee?> GetByIdAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        return _context.Employees
            .Include(e => e.BookableServices)
            .SingleOrDefaultAsync(e => e.Id == employeeId, ct);
    }

    public Task<bool> ExistsEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        return _context.Employees
            .AsNoTracking()
            .AnyAsync(e => e.Id == employeeId, ct);
    }
    
    public void Add(Domain.Aggregates.Employee employee)
    {
        _context.Employees.Add(employee);
    }
}