namespace Business.Application.DTOs.EmployeeDTOs;

public record SetWorkingHoursDto(
    Guid EmployeeId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime
    );
