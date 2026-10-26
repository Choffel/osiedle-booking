using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.BookableServiceDTOs;

public record UpdateBookableServiceDto(
    EmployeeId EmployeeId,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
    );
