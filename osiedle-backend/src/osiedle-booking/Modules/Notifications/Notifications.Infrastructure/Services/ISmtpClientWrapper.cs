using MimeKit;

namespace Notifications.Infrastructure.Services;

public interface ISmtpClientWrapper
{
    Task SendMimeMessageAsync(MimeMessage message, CancellationToken cancellationToken);
}