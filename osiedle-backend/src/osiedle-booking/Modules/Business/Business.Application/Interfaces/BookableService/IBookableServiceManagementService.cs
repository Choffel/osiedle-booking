using Business.Application.DTOs.BookableServiceDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.BookableService;

public interface IBookableServiceManagementService
{
    Task<Result<BookableServiceResponseDto>>CreateBookableServiceAsync(CreateBookableServiceDto bookableServiceDto, CancellationToken ct);
    
    Task<Result<BookableServiceResponseDto>> UpdateBookableServiceAsync(Guid bookableServiceId, UpdateBookableServiceDto bookableServiceDto, CancellationToken ct);
    
    Task<Result<BookableServiceResponseDto>> DeleteBookableServiceAsync(Guid bookableServiceId, CancellationToken ct);
    
    Task<Result> ActivateBookableServiceAsync(Guid employeeId,Guid bookableServiceId, CancellationToken ct);
    
    Task<Result> DeactivateBookableServiceAsync(Guid employeeId,Guid bookableServiceId, CancellationToken ct);
}