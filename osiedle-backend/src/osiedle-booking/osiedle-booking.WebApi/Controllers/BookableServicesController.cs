using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.Interfaces.BookableService;
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
        return (await _bookableServiceManagementService.UpdateBookableServiceAsync(bookableServiceId, dto, ct))
            .ToActionResult();
    }

    [HttpDelete("{bookableServiceId:guid}")]
    public async Task<ActionResult<BookableServiceResponseDto>> DeleteBookableServiceAsync(
        Guid bookableServiceId,
        CancellationToken ct = default)
    {
        return (await _bookableServiceManagementService.DeleteBookableServiceAsync(bookableServiceId, ct))
            .ToActionResult();
    }

    [HttpPost("{bookableServiceId:guid}/activate")]
    public async Task<ActionResult> ActivateBookableServiceAsync(
        Guid bookableServiceId,
        [FromQuery] Guid employeeId,
        CancellationToken ct = default)
    {
        return (await _bookableServiceManagementService.ActivateBookableServiceAsync(employeeId, bookableServiceId, ct))
            .ToActionResult();
    }

    [HttpPost("{bookableServiceId:guid}/deactivate")]
    public async Task<ActionResult> DeactivateBookableServiceAsync(
        Guid bookableServiceId,
        [FromQuery] Guid employeeId,
        CancellationToken ct = default)
    {
        return (await _bookableServiceManagementService.DeactivateBookableServiceAsync(employeeId, bookableServiceId, ct))
            .ToActionResult();
    }
}
