using MimeKit;
using Notifications.Infrastructure.Options;

namespace Notifications.Infrastructure.Factories;

public class MimeMessageFactory : IMimeMessageFactory
{

    private readonly SmtpOptions _smtpOptions;


    public MimeMessageFactory(SmtpOptions smtpOptions)
    {
        _smtpOptions = smtpOptions;
    }
    
    public MimeMessage CreateMimeMessage(string recipient, string subject, string body)
    {
        var message = new MimeMessage();
        
        message.From.Add(new MailboxAddress(_smtpOptions.SenderName, _smtpOptions.SenderEmail));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };
        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }
}