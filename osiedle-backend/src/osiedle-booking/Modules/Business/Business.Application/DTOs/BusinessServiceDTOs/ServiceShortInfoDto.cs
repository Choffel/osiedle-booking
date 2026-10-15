namespace Business.Application.DTOs.BusinessServiceDTOs;

public record ServiceShortInfoDto(
    Guid BusinessServiceId,
    Guid EmployeeId,
    string ServiceName,
    decimal Price,
    TimeSpan Duration
    );