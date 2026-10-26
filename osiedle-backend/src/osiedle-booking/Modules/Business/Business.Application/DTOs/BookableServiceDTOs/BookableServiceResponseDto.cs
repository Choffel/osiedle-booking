namespace Business.Application.DTOs.BookableServiceDTOs;

public record BookableServiceResponseDto(
    Guid EmployeeId,
    Guid BookableServiceId,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
    );
