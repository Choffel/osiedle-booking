using System.Threading;
using System.Threading.Tasks;
using Notifications.Application.DTOs.Booking;

namespace Notifications.Application.Interfaces;

public interface IBookingNotificationService
{
    Task SendBookingConfirmationAsync(BookingConfirmationDto dto, CancellationToken ct = default);
}