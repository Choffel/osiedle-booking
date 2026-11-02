using Business.Domain.Enums;

namespace Business.Application.DTOs.EmployeeDTOs;

public record ChangeEmploymentStatusDto(
    Guid EmployeeId,
    EmploymentStatus NewStatus,
    string? Reason = null
    );
