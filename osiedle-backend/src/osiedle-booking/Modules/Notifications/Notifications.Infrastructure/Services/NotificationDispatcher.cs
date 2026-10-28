using Notifications.Application.Interfaces;
using Notifications.Domain.Enums;

namespace Notifications.Infrastructure.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    public Task DispatchAsync<TModel>(NotificationChannel channel, string recipient, string subject, string templateName,
        TModel model, CancellationToken ct)
    {
        
    }
}