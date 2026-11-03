namespace Business.Application.DTOs.BookableServiceDTOs;

public record ServiceShortInfoDto(
    Guid BookableServiceId,
    Guid EmployeeId,
    string ServiceName,
    decimal Price,
    TimeSpan Duration
    );
