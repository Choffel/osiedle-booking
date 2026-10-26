namespace Business.Application.DTOs.EmployeeDTOs;

public record UpdateEmployeeProfileDto(
    string FirstName,
    string LastName,
    string Position,
    string? Review,
    string Email
    );
