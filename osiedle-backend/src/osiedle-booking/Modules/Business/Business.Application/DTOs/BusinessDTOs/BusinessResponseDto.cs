using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.DTOs.BusinessDTOs;
using Business.Application.DTOs.EmployeeDTOs;

namespace Business.Application.DTOs;

public record BusinessResponseDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Description,
    LocationDto Location,
    IReadOnlyList<ServiceShortInfoDto> Sevices,
    IReadOnlyList<EmployeeShortInfoDto> Employees,
    string Status,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
