using Business.Application.DTOs.EmployeeDTOs;
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

    public Task<Business.Domain.Entities.WorkingHours?> GetWorkingHoursAsync(EmployeeId employeeId, DateOnly date, CancellationToken ct = default)
    {
        return _context.WorkingHours
            .AsNoTracking()
            .SingleOrDefaultAsync(w => w.EmployeeId == employeeId && w.Date == date, ct);
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

    public void AddWorkingHours(Business.Domain.Entities.WorkingHours workingHours)
    {
        _context.WorkingHours.Add(workingHours);
    }

    public void UpdateWorkingHours(Business.Domain.Entities.WorkingHours workingHours)
    {
        _context.WorkingHours.Update(workingHours);
    }

    public void RemoveWorkingHours(Business.Domain.Entities.WorkingHours workingHours)
    {
        _context.WorkingHours.Remove(workingHours);
    }
}