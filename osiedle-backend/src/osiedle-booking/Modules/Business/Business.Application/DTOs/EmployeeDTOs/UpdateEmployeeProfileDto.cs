using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.EmployeeDTOs;

public record UpdateEmployeeProfileDto(
    EmployeeId EmployeeId,
    string FirstName,
    string LastName,
    string Position,
    string? Review,
    string Email
    );