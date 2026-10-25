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

namespace Business.Application.Services.BookableItemServices;

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

        var bookableService = BookableService.Create(
            employeeId,
            bookableServiceDto.ServiceName,
            bookableServiceDto.ServiceDescription,
            bookableServiceDto.ServicePrice,
            bookableServiceDto.ServiceDuration
        );

        bool employeeExists = await _employeeRepository.ExistsEmployeeAsync(employeeId, ct);
        if (!employeeExists)
        {
            return Result.Failure<BookableServiceResponseDto>(BookableServiceErrors.EmployeeNotFound);
        }

        bool isNameTaken = await _bookableServiceRepository.ExistsByNameForEmployeeAsync(bookableService.ServiceName, employeeId, ct);
        if (isNameTaken)
        {
            return Result.Failure<BookableServiceResponseDto>(BookableServiceErrors.NameAlreadyTaken);
        }

        _bookableServiceRepository.Add(bookableService, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(bookableService.ToDto());
    }

    public async Task<Result<BookableServiceResponseDto>> UpdateBookableServiceAsync(BookableServiceId serviceId, UpdateBookableServiceDto dto, CancellationToken ct)
    { 
        
    var bookableService = await _bookableServiceRepository.GetByIdAsync(serviceId, ct);
    if (bookableService is null)
    {
        return Result.Failure<BookableServiceResponseDto>(
            BookableServiceErrors.NotFound);
    }
    
    //TODO: Relocate checking to domain layer
    if (bookableService.EmployeeId != dto.EmployeeId)
    {
        return Result.Failure<BookableServiceResponseDto>(
            BookableServiceErrors.NotFoundOrAccessDenied);
    }
    
    
    bool isNameChanged = !string.Equals(bookableService.ServiceName, dto.ServiceName, StringComparison.Ordinal);
    if (isNameChanged)
    {
        bool isNameTaken = await _bookableServiceRepository.ExistsByNameForEmployeeAsync(
            dto.ServiceName, 
            dto.EmployeeId, 
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

    public async Task<Result<BookableServiceResponseDto>> DeleteBookableServiceAsync(BookableServiceId bookableServiceId, CancellationToken ct)
    {
        
        var bookableService = await _bookableServiceRepository.GetByIdAsync(bookableServiceId, ct);
    
     
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

    public  async Task<Result> ActivateBookableServiceAsync(EmployeeId employeeId, BookableServiceId bookableServiceId, CancellationToken ct)
    {
        var bookableService = await _bookableServiceRepository.GetByIdAsync(bookableServiceId, ct);
        if (bookableService is null)
        {
            return Result.Failure(BookableServiceErrors.NotFound);
        }
    
        
        if (bookableService.EmployeeId != employeeId)
        {
            return Result.Failure(BookableServiceErrors.NotFoundOrAccessDenied);
        }
    
        
        bookableService.ActivateService();
        
        await _unitOfWork.SaveChangesAsync(ct);
    
        return Result.Success();
    }

    public async Task<Result> DeactivateBookableServiceAsync(EmployeeId employeeId, BookableServiceId bookableServiceId, CancellationToken ct)
    {
        var bookableService = await _bookableServiceRepository.GetByIdAsync(bookableServiceId, ct);
        
        if (bookableService is null)
        {
            return Result.Failure(BookableServiceErrors.NotFound);
        }

        if (bookableService.EmployeeId != employeeId)
        {
            return Result.Failure(BookableServiceErrors.NotFoundOrAccessDenied);
        }
        
        bookableService.DeactivateService();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}