namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeResponseDto(
    Guid Id,
    Guid? BusinessId,
    string FirstName,
    string LastName,
    string Position,
    string? Review,
    string Email
    );