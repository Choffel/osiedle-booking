using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.WorkingHoursDTOs;

public record WorkingHoursDto(
    DateOnly Date,
    TimeSlot TimeSlot
);