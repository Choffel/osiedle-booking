using Bookings.Appliation.Abstractions.Services;
using Bookings.Appliation.DTOs;
using Bookings.Domain.Entities.Booking.Value_objects;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController
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
    public async Task<Result<bool>> CreateBookingAsync([FromQuery]  CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookingService.CreateBookingAsync(request, ct);
        return result;
    }
    
    [HttpGet("reservations")]
    public async Task<Result<IReadOnlyCollection<MyReservationDTO>>> GetMyReservationsAsync(CancellationToken ct)
    {
        var result = await _bookingService.GetMyReservationsAsync(ct);
        return result;
    }
}