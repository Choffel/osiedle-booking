using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces.Employee;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Repositories.Employee;

public class EmployeeReadOnlyRepository : IEmployeeReadOnlyRepository
{
    
    private readonly BusinessDbContext _context;

    public EmployeeReadOnlyRepository(BusinessDbContext context)
    {
        _context = context;
    }
    public async Task<EmployeeShortInfoDto?> GetShortInfoAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == employeeId)
            .Select(e => new EmployeeShortInfoDto(
                e.Id.Value,
                e.EmployeeName,
                e.Position
            ))
            .FirstOrDefaultAsync(ct);
    }
}