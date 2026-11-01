using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeShortInfoDto(
    Guid EmployeeId,
    EmployeeName EmployeeName,
    string Position
    );
