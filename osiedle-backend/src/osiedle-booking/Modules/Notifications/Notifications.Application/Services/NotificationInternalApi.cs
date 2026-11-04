using Notifications.Application.Interfaces;
using osiedle_booking.BuildingBlocks.Interfaces;

namespace Notifications.Application.Services;

public class NotificationInternalApi : INotificationInternalApi
{
    private readonly INotificationModule _notificationModule; 
    
    public async Task SendVerificationLinkAsync(string email, string code, CancellationToken ct) 
    {
        await _notificationModule.SendVerificationLinkAsync(email, code, ct);
    }
}