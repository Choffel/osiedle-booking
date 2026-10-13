using Business.Domain.ValueObject;

namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeCreateDto(
    BusinessId? BusinessId,
    string FirstName,
    string LastName,
    string Position,
    string? Review,
    string Email
    );