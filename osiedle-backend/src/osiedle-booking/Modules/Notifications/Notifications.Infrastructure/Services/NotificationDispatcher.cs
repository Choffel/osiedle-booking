using Notifications.Application.Interfaces;
using Notifications.Domain.Enums;

namespace Notifications.Infrastructure.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IReadOnlyDictionary<NotificationChannel, INotificationChannelStrategy> _strategies;
    private readonly ITemplateRender _templateRenderer;

    public NotificationDispatcher(
        IEnumerable<INotificationChannelStrategy> strategies, 
        ITemplateRender templateRenderer)
    {
        _strategies = strategies.ToDictionary(s => s.Channel);
        _templateRenderer = templateRenderer;
    }
    
    public async Task DispatchAsync<TModel>(
        NotificationChannel channel, 
        string recipient, 
        string subject, 
        string templateName,
        TModel model, 
        CancellationToken ct)
    {
        if (!_strategies.TryGetValue(channel, out var strategy))
        {
            throw new InvalidOperationException($"Strategy for channel '{channel}' is not registered.");
        }
        
        string body = await _templateRenderer.RenderAsync(templateName, model, ct);

        await strategy.SendAsync(recipient, subject, body, ct);
    }
}