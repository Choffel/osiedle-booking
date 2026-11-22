using Business.Domain.Enums;

namespace Business.Application.DTOs.BookableServiceDTOs;

public record BookableServiceResponseDto(
    Guid EmployeeId,
    Guid BookableServiceId,
    Category Category,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
);
