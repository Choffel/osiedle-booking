using Business.Domain.Enums;

namespace Business.Application.DTOs.BookableServiceDTOs;

public record ServiceShortInfoDto(
    Guid BookableServiceId,
    Guid EmployeeId,
    Category Category,
    string ServiceName,
    decimal Price,
    TimeSpan Duration
);
