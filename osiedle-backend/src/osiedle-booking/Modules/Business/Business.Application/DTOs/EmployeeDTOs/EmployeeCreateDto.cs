using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeCreateDto(
    BusinessId? BusinessId,
    EmployeeName Name,
    string Position,
    string? Review,
    string Email
    );