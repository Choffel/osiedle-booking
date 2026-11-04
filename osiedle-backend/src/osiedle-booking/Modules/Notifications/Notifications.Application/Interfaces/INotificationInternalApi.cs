namespace Notifications.Application.Interfaces;

public interface INotificationInternalApi
{
    Task SendVerificationLinkAsync(string email, string link, CancellationToken ct);
}