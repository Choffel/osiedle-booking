using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.Interfaces.BookableService;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.WebApi.Extensions;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookableServicesController : ControllerBase
{
    private readonly IBookableServiceManagementService _bookableServiceManagementService;

    public BookableServicesController(IBookableServiceManagementService bookableServiceManagementService)
    {
        _bookableServiceManagementService = bookableServiceManagementService;
    }

    [HttpPost]
    public async Task<ActionResult<BookableServiceResponseDto>> CreateBookableServiceAsync(
        [FromBody] CreateBookableServiceDto dto,
        CancellationToken ct = default)
    {
        return (await _bookableServiceManagementService.CreateBookableServiceAsync(dto, ct)).ToActionResult();
    }

    [HttpPut("{bookableServiceId:guid}")]
    public async Task<ActionResult<BookableServiceResponseDto>> UpdateBookableServiceAsync(
        Guid bookableServiceId,
        [FromBody] UpdateBookableServiceDto dto,
        CancellationToken ct = default)
    {
        var serviceId = BookableServiceId.From(bookableServiceId);
        
        return (await _bookableServiceManagementService.UpdateBookableServiceAsync(serviceId, dto, ct))
            .ToActionResult();
    }

    [HttpDelete("{bookableServiceId:guid}")]
    public async Task<ActionResult<BookableServiceResponseDto>> DeleteBookableServiceAsync(
        Guid bookableServiceId,
        CancellationToken ct = default)
    {
        
        var serviceId = BookableServiceId.From(bookableServiceId);
        
        return (await _bookableServiceManagementService.DeleteBookableServiceAsync(serviceId, ct))
            .ToActionResult();
    }

    [HttpPost("{bookableServiceId:guid}/activate")]
    public async Task<ActionResult> ActivateBookableServiceAsync(
        Guid bookableServiceId,
        [FromQuery] Guid employeeId,
        CancellationToken ct = default)
    {
        var serviceId = BookableServiceId.From(bookableServiceId);
        var employee = EmployeeId.From(employeeId);
        
        return (await _bookableServiceManagementService.ActivateBookableServiceAsync(employee, serviceId, ct))
            .ToActionResult();
    }

    [HttpPost("{bookableServiceId:guid}/deactivate")]
    public async Task<ActionResult> DeactivateBookableServiceAsync(
        Guid bookableServiceId,
        [FromQuery] Guid employeeId,
        CancellationToken ct = default)
    {
        var employee = EmployeeId.From(employeeId);
        var serviceId = BookableServiceId.From(bookableServiceId);
        
        return (await _bookableServiceManagementService.DeactivateBookableServiceAsync(employee, serviceId, ct))
            .ToActionResult();
    }
}
