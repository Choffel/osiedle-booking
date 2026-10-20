namespace Business.Application.DTOs.BookableServiceDTOs;

public record UpdateBookableServiceDto(
    Guid Id,
    Guid EmployeeId,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
    );
