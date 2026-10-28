using Notifications.Domain.Enums;

namespace Notifications.Application.Interfaces;

public interface INotificationDispatcher
{
    Task DispatchAsync<TModel>(
        NotificationChannel channel, 
        string recipient, 
        string subject, 
        string templateName, 
        TModel model, 
        CancellationToken ct);
}