using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.DTOs.EmployeeDTOs;

public record EmployeeCreateDto(
    Guid? BusinessId,
    string FirstName,
    string LastName,
    string Position,
    string? Review,
    string Email
    );