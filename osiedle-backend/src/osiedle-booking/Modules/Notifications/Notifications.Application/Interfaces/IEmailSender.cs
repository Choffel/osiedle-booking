namespace Notifications.Application.Interfaces;

public interface IEmailSender
{
    Task SendCodeAsync(string recipient, string subject, string body, CancellationToken ct);
}