using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.ExtensionMethods;
using Business.Application.Interfaces;
using Business.Application.Interfaces.BookableService;
using Business.Application.Interfaces.Employee;
using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services;

public class BookableServiceManagementService : IBookableServiceManagementService
{
    private readonly IBookableServiceRepository _bookableServiceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookableServiceManagementService(IBookableServiceRepository bookableServiceRepository,
        IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _bookableServiceRepository = bookableServiceRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<BookableServiceResponseDto>> CreateBookableServiceAsync(CreateBookableServiceDto bookableServiceDto, CancellationToken ct)
    {
        var employeeId = EmployeeId.From(bookableServiceDto.EmployeeId);
        
        
        bool employeeExists = await _employeeRepository.ExistsEmployeeAsync(employeeId, ct);
        if (!employeeExists)
        {
            return Result.Failure<BookableServiceResponseDto>(BookableServiceErrors.EmployeeNotFound);
        }

        bool isNameTaken = await _bookableServiceRepository.ExistsByNameForEmployeeAsync(bookableServiceDto.ServiceName, employeeId, ct);
        if (isNameTaken)
        {
            return Result.Failure<BookableServiceResponseDto>(BookableServiceErrors.NameAlreadyTaken);
        }
        
        var bookableService = BookableService.Create(
            employeeId,
            bookableServiceDto.ServiceName,
            bookableServiceDto.ServiceDescription,
            bookableServiceDto.ServicePrice,
            bookableServiceDto.ServiceDuration
        );
        
        _bookableServiceRepository.Add(bookableService, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(bookableService.ToDto());
    }

    public async Task<Result<BookableServiceResponseDto>> UpdateBookableServiceAsync(UpdateBookableServiceDto dto, CancellationToken ct)
    { 
        
    var serviceId = BookableServiceId.From(dto.Id);
    var employeeId = EmployeeId.From(dto.EmployeeId);

    
    bool belongsToEmployee = await _bookableServiceRepository.BelongsToEmployeeAsync(serviceId, employeeId, ct);
    if (!belongsToEmployee)
    {
        return Result.Failure<BookableServiceResponseDto>(
            BookableServiceErrors.NotFoundOrAccessDenied);
    }
    
    var bookableService = await _bookableServiceRepository.GetByIdAsync(serviceId, ct);
    if (bookableService is null)
    {
        return Result.Failure<BookableServiceResponseDto>(
            BookableServiceErrors.NotFound);
    }
    
    bool isNameChanged = !string.Equals(bookableService.ServiceName, dto.ServiceName, StringComparison.Ordinal);
    if (isNameChanged)
    {
        bool isNameTaken = await _bookableServiceRepository.ExistsByNameForEmployeeAsync(
            dto.ServiceName, 
            employeeId, 
            ct);

        if (isNameTaken)
        {
            return Result.Failure<BookableServiceResponseDto>(
                BookableServiceErrors.NameAlreadyTaken);
        }
    }
    
    bookableService.UpdateInfo(
        dto.ServiceName,
        dto.ServiceDescription,
        dto.ServicePrice,
        dto.ServiceDuration
    );
    
    await _unitOfWork.SaveChangesAsync(ct);
    
    return Result.Success(bookableService.ToDto());
    
    }

    public async Task<Result<BookableServiceResponseDto>> DeleteBookableServiceAsync(Guid bookableServiceId, CancellationToken ct)
    {
        var serviceId = BookableServiceId.From(bookableServiceId);

        
        var bookableService = await _bookableServiceRepository.GetByIdAsync(serviceId, ct);
    
     
        if (bookableService is null)
        {
            return Result.Failure<BookableServiceResponseDto>(
                BookableServiceErrors.NotFound);
        }
        
        if (bookableService.Status == ServiceStatus.Archived)
        {
            return Result.Failure<BookableServiceResponseDto>(
                BookableServiceErrors.AlreadyArchived);
        }
        
        bookableService.ArchiveService(); 

        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(bookableService.ToDto());
    }

    public  async Task<Result> ActivateBookableServiceAsync(Guid employeeId, Guid bookableServiceId, CancellationToken ct)
    {
        var serviceId = BookableServiceId.From(bookableServiceId);
        var ownerId = EmployeeId.From(employeeId);

        bool belongsToEmployee = await  _bookableServiceRepository.BelongsToEmployeeAsync(serviceId, ownerId, ct);
        
        if (!belongsToEmployee)
        {
            return Result.Failure(BookableServiceErrors.NotFoundOrAccessDenied);
        }

        var bookableService = await _bookableServiceRepository.GetByIdAsync(serviceId, ct);
        
        if (bookableService is null)
        {
            return Result.Failure(BookableServiceErrors.NotFound);
        }
        
        bookableService.ActivateService();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeactivateBookableServiceAsync(Guid employeeId, Guid bookableServiceId, CancellationToken ct)
    {
        var serviceId = BookableServiceId.From(bookableServiceId);
        var ownerId = EmployeeId.From(employeeId);

        bool belongsToEmployee = await  _bookableServiceRepository.BelongsToEmployeeAsync(serviceId, ownerId, ct);
        
        if (!belongsToEmployee)
        {
            return Result.Failure(BookableServiceErrors.NotFoundOrAccessDenied);
        }

        var bookableService = await _bookableServiceRepository.GetByIdAsync(serviceId, ct);
        
        if (bookableService is null)
        {
            return Result.Failure(BookableServiceErrors.NotFound);
        }
        
        bookableService.DeactivateService();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}