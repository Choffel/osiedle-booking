using Business.Domain.Enums;
using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.BookableServiceDTOs;

public record UpdateBookableServiceDto(
    EmployeeId EmployeeId,
    Category Category,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
);
