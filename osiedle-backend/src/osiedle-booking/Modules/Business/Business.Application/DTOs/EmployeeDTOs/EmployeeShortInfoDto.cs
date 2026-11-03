namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeShortInfoDto(
    Guid EmployeeId,
    string FirstName,
    string LastName,
    string Position
    );
