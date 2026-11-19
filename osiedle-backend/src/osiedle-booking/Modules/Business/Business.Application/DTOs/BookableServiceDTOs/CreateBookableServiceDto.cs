using Business.Domain.Enums;

namespace Business.Application.DTOs.BookableServiceDTOs;

public record CreateBookableServiceDto(
    Guid EmployeeId,
    Category Category,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
);
