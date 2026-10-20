using Business.Application.DTOs.BookableServiceDTOs;

namespace Business.Application.ExtensionMethods;

public static  class BookableMappingExtensions
{
    public static BookableServiceResponseDto ToDto(this Domain.Entities.BookableService bookableService)
    {
        return new BookableServiceResponseDto(
            bookableService.Id.Value,           
            bookableService.EmployeeId.Value,   
            bookableService.ServiceName,
            bookableService.Description,
            bookableService.Price,
            bookableService.Duration
        );
    }
}