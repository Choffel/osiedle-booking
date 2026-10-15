namespace Business.Application.DTOs.EmployeeDTOs;

public record WorkingHoursDto(
    TimeOnly StartTime,
    TimeOnly EndTime
);