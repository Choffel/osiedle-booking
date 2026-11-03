namespace Business.Application.DTOs.EmployeeDTOs;

public record SetWorkingHoursDto(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime
    );
