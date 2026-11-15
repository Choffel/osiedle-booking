using Notifications.Domain.Enums;

namespace Notifications.Application.Interfaces;

public interface INotificationChannelStrategy
{
    NotificationChannel Channel { get; }
    
    Task SendAsync(string recipient, string subject, string body, CancellationToken ct);
}