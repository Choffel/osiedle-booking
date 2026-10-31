using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.WorkingHoursDTOs;

public record SetWorkingHoursDto(
    WorkingHoursId WorkingHoursId,
    DateOnly Date,
    TimeSlot Slot
    );
