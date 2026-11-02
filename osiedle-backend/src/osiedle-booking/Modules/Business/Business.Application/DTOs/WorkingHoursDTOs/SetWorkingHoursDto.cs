namespace Business.Application.DTOs.WorkingHoursDTOs;

public record SetWorkingHoursDto(
    Guid WorkingHoursId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime
    );
