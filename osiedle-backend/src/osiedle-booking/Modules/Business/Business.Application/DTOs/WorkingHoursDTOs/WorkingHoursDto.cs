namespace Business.Application.DTOs.WorkingHoursDTOs;

public record WorkingHoursDto(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime
);
