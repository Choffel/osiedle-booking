namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeCreateDto(
    Guid BusinessId,
    string FirstName,
    string LastName,
    string Position,
    string? Review
    );