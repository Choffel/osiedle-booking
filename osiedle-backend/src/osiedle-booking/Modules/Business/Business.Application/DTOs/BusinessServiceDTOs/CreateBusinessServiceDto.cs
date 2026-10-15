namespace Business.Application.DTOs.BusinessServiceDTOs;

public record CreateBusinessServiceDto(
    Guid EmployeeId,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
    );