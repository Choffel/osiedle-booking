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


    public Task<Domain.Aggregates.Employee?> GetEmployeeByIdAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        return _context.Employees
            .AsNoTracking()
            .Include(e => e.BookableServices)
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

    public async Task<EmployeeStatusDto?> GetStatusAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == employeeId)
            .Select(e => new EmployeeStatusDto(
                e.Status,
                e.BusinessId != null ? e.BusinessId.Value : null
            ))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<EmployeeShortInfoDto?> GetShortInfoAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == employeeId)
            .Select(e => new EmployeeShortInfoDto(
                e.Id.Value,
                e.FirstName,
                e.LastName,
                e.Position
            ))
            .SingleOrDefaultAsync(ct);
    }

    public void AddEmployee(Domain.Aggregates.Employee employee)
    {
        _context.Employees.Add(employee);
    }
}