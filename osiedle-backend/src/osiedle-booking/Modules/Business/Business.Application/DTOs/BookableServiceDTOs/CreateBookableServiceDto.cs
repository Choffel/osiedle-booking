namespace Business.Application.DTOs.BookableServiceDTOs;

public record CreateBookableServiceDto(
    Guid EmployeeId,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
    );
