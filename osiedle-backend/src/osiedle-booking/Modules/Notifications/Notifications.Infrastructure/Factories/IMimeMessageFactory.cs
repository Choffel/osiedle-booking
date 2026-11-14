using MimeKit;

namespace Notifications.Infrastructure.Factories;

public interface IMimeMessageFactory
{
    MimeMessage CreateMimeMessage(string recipient, string subject, string body);
}