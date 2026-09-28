using Notifications.Application.DTOs.Booking;

namespace Notifications.Application.Interfaces;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(BookingConfirmationDto dto);
    
    Task SendRegistrationConfirmationAsync(RegistrationConfirmationDto dto);
    
    Task SendPasswordResetAsync(PasswordResetDto dto);
    
    //добавление фирмы обновление данных фирмы и т.д.
}