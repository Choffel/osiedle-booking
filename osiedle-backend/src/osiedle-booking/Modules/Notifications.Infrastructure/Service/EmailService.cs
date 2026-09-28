using Notifications.Application.DTOs.Booking;
using Notifications.Application.Interfaces;
using Notifications.Infrastructure.Templates;

namespace Notifications.Infrastructure.Service;

public class EmailService : IEmailService 
{
    
    
    
    public async Task SendBookingConfirmationAsync(BookingConfirmationDto dto)
    {
        string htmlBody = EmailTemplates.GetBookingConfirmationTemplate(
            clientName: dto.ClientName,
            businessName: dto.BusinessName,
            serviceName: dto.ServiceName,
            appointmentTime: dto.AppointmentTime,
            address: dto.Address,
            price: dto.Price
        );
        
        await SendEmailAsync(
            toEmail: dto.ClientEmail, 
            subject: $"Potwierdzenie rezerwacji — {dto.BusinessName}", 
            bodyHtml: htmlBody
        );
    }
}