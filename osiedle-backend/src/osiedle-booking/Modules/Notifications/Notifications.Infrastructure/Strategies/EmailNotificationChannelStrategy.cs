using Notifications.Application.Interfaces;
using Notifications.Domain.Enums;
using Notifications.Infrastructure.Factories;
using Notifications.Infrastructure.Services;

namespace Notifications.Infrastructure.Strategies;

public class EmailNotificationChannelStrategy : INotificationChannelStrategy
{
    private readonly IMimeMessageFactory _mimeMessageFactory;
    private readonly ISmtpClientWrapper _smtpClientWrapper;

    public NotificationChannel Channel => NotificationChannel.Email;

    public EmailNotificationChannelStrategy(
        IMimeMessageFactory mimeMessageFactory,
        ISmtpClientWrapper smtpClientWrapper)
    {
        _mimeMessageFactory = mimeMessageFactory;
        _smtpClientWrapper = smtpClientWrapper;
    }

    public async Task SendAsync(string recipient, string subject, string body, CancellationToken ct)
    {
        var message = _mimeMessageFactory.CreateMimeMessage(recipient, subject, body);

        await _smtpClientWrapper.SendMimeMessageAsync(message, ct);
    }
}