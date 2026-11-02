using System.Collections.ObjectModel;
using System.Security.Claims;
using Bookings.Application.Abstractions.Services;
using Bookings.Application.DTOs;
using Bookings.Domain.Entities.Booking.Value_objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.WebApi.Extensions;


namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }
    
    [HttpGet("available-slots")]
    public async Task<Result<IReadOnlyList<TimeSlot>>> GetBookingsAsync(
        [FromQuery] GetAvailableSlotsRequest request,
        CancellationToken ct)
    {
        var result = await _bookingService.GetAvailableSlotsAsync(request, ct);
        return result;
    }
    
    [HttpPost("booking")]
    public async Task<IActionResult> CreateBookingAsync([FromQuery]  CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookingService.CreateBookingAsync(request, ct);
        return result.ToActionResult();
    }
    
    [Authorize]
    [HttpGet("reservations")]
    public async Task<IActionResult> GetMyReservationsAsync(CancellationToken ct)
    {
        var clientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var result = await _bookingService.GetMyReservationsAsync(ClientId.From(clientId), ct);
        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [Authorize]
    [HttpDelete("reservations")]
    public async Task<IActionResult> CancelBookingByIdAsync(CancelBookingRequest request, CancellationToken ct)
    {
        var clientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        var result = await _bookingService.CancelBookingAsync(request, ClientId.From(clientId),  ct);
        return result.ToActionResult();
    }
}