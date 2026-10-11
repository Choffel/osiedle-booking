using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Intarfaces;
using Business.Infrastructure.Persistence;

namespace Business.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly BusinessDbContext _context;
    
    public EmployeeRepository(BusinessDbContext context)
    {
        _context = context;
    }
    
    public Task<EmployeeCreateDto> CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken ct = default)
    {
        
    }

    public Task<EmployeeResponseDto> UpdateEmployeeAsync(EmployeeUpdateDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<EmployeeResponseDto> GetEmployeeByIdAsync(Guid employeeId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task ArchiveEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}