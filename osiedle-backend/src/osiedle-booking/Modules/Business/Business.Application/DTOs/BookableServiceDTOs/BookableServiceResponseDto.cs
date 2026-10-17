namespace Business.Application.DTOs.BookableServiceDTOs;

public record BookableServiceResponseDto(
    Guid EmployeeId,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
    );
