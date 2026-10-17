namespace Business.Application.DTOs.BookableServiceDTOs;

public record UpdateBookableServiceDto(
    Guid EmployeeId,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
    );
