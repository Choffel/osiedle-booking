namespace Business.Application.DTOs.BusinessServiceDTOs;

public record BusinessServiceResponseDto(
    Guid EmployeeId,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
    );