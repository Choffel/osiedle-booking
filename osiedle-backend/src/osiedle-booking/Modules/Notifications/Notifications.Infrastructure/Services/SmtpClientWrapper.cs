using MailKit.Security;
using MimeKit;
using Notifications.Infrastructure.Options;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Notifications.Infrastructure.Services;

public class SmtpClientWrapper : ISmtpClientWrapper
{
    private readonly SmtpOptions _smtpOptions;
    
    public SmtpClientWrapper(SmtpOptions smtpOptions)
    {  
       _smtpOptions = smtpOptions; 
    }
    
    
    public async Task SendMimeMessageAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();

        var options = _smtpOptions.Port switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            _ when _smtpOptions.UseSsl => SecureSocketOptions.StartTls,
            _ => SecureSocketOptions.None
        };

        await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, options, cancellationToken);
        await client.AuthenticateAsync(_smtpOptions.UserName, _smtpOptions.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}