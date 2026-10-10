namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeUpdateDto(
    string FirstName,
    string LastName,
    string Position,
    string? Review
    );