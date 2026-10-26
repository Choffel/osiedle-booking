namespace Business.Application.DTOs.EmployeeDTOs;

public record WorkingHoursDto(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime
);