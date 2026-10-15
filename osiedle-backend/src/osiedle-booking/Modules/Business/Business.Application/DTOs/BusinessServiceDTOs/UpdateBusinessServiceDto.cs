namespace Business.Application.DTOs.BusinessServiceDTOs;

public record UpdateBusinessServiceDto(
    Guid EmployeeId,
    string ServiceName,
    string ServiceDescription,
    decimal ServicePrice,
    TimeSpan ServiceDuration
    );