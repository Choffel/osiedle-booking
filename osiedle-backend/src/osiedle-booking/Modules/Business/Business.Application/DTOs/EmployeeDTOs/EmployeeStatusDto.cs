using Business.Domain.Enums;

namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeStatusDto(
    EmploymentStatus Status,
    Guid? BusinessId
    );
