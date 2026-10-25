using Business.Application.DTOs.BookableServiceDTOs;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.BookableService;

public interface IBookableServiceManagementService
{
    Task<Result<BookableServiceResponseDto>>CreateBookableServiceAsync(CreateBookableServiceDto bookableServiceDto, CancellationToken ct);
    
    Task<Result<BookableServiceResponseDto>> UpdateBookableServiceAsync(BookableServiceId serviceId, UpdateBookableServiceDto bookableServiceDto, CancellationToken ct);
    
    Task<Result<BookableServiceResponseDto>> DeleteBookableServiceAsync(BookableServiceId serviceId, CancellationToken ct);
    
    Task<Result> ActivateBookableServiceAsync(EmployeeId employeeId,BookableServiceId serviceId, CancellationToken ct);
    
    Task<Result> DeactivateBookableServiceAsync(EmployeeId employeeId,BookableServiceId serviceId, CancellationToken ct);
}