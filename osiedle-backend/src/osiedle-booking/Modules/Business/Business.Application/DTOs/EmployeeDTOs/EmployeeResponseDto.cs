using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeResponseDto(
    Guid Id,
    Guid? BusinessId,
    EmployeeName Name,
    string Position,
    string? Review,
    string Email
    );