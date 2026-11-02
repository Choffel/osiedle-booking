using Business.Application.DTOs.BookableServiceDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.BookableService;

public interface IBookableServiceManagementService
{
    Task<Result<BookableServiceResponseDto>>CreateBookableServiceAsync(CreateBookableServiceDto bookableServiceDto, CancellationToken ct);
    
    Task<Result<BookableServiceResponseDto>> UpdateBookableServiceAsync(Guid serviceId, UpdateBookableServiceDto bookableServiceDto, CancellationToken ct);
    
    Task<Result<BookableServiceResponseDto>> DeleteBookableServiceAsync(Guid serviceId, CancellationToken ct);
    
    Task<Result> ActivateBookableServiceAsync(Guid employeeId, Guid serviceId, CancellationToken ct);
    
    Task<Result> DeactivateBookableServiceAsync(Guid employeeId, Guid serviceId, CancellationToken ct);
}
