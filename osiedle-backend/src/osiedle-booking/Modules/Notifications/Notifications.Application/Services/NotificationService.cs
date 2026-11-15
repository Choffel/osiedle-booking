using Notifications.Application.Interfaces;
using Notifications.Domain.Enums;
using osiedle_booking.BuildingBlocks.Interfaces;

namespace Notifications.Application.Services;

public class NotificationService : INotificationModule
{
    private readonly INotificationDispatcher _dispatcher;

    public NotificationService(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task SendVerificationLinkAsync(string email, string confirmationLink, CancellationToken ct = default)
    {
        var model = new { Link = confirmationLink };
        
        await _dispatcher.DispatchAsync(
            channel: NotificationChannel.Email,
            recipient: email,
            subject: "Link podtwierdzenia konta",
            templateName: "VerificationLink", 
            model: model,
            ct: ct
        );   
    }
}