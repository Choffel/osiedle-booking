using Business.Domain.Enums;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.EmployeeDTOs;

public record ChangeEmploymentStatusDto(
    EmployeeId EmployeeId,
    EmploymentStatus NewStatus,
    string? Reason = null
    );