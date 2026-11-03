using Notifications.Application.Interfaces;
using Notifications.Domain.Enums;

namespace Notifications.Infrastructure.Services;

public class NotificationDispatcher : INotificationDispatcher
{   
    private readonly IReadOnlyDictionary<NotificationChannel,INotificationChannelStrategy> _notificationChannelStrategies;
    private readonly ITemplateRender _templateRenderer;

    public NotificationDispatcher(IEnumerable<NotificationChannel> notificationChannels, ITemplateRender templateRenderer)
    {
        _notificationChannelStrategies = 
    }
    
    public Task DispatchAsync<TModel>(NotificationChannel channel, string recipient, string subject, string templateName,
        TModel model, CancellationToken ct)
    {
        var strategy = 
    }
}