using Notifications.Domain.Enums;
using Notifications.Infrastructure.Factories;
using Notifications.Infrastructure.Services;

namespace Notifications.Infrastructure.Strategies;

public class EmailNotificationChannelStrategy
{
    private readonly IMimeMessageFactory _messageFactory;
    private readonly ISmtpClientWrapper _smtpClientWrapper;

    public NotificationChannel Channel => NotificationChannel.Email;

    public EmailNotificationChannelStrategy(
        IMimeMessageFactory messageFactory, 
        ISmtpClientWrapper smtpClientWrapper)
    {
        _messageFactory = messageFactory;
        _smtpClientWrapper = smtpClientWrapper;
    }

    public async Task SendAsync(string recipient, string subject, string body, CancellationToken ct = default)
    {
        var mimeMessage = _messageFactory.CreateMimeMessage(recipient, subject, body);
        await _smtpClientWrapper.SendMimeMessageAsync(mimeMessage, ct);
    }
}