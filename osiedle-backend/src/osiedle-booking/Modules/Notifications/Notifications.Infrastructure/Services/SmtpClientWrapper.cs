using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Notifications.Infrastructure.Options;

namespace Notifications.Infrastructure.Services;

public class SmtpClientWrapper : ISmtpClientWrapper
{
    private readonly SmtpOptions _smtpOptions;
    
    public SmtpClientWrapper(SmtpOptions smtpOptions)
    {  
       _smtpOptions = smtpOptions; 
    }
    
    
    public  async Task SendMimeMessageAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();

        var secureSmtpOptions = _smtpOptions.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None;
        
        await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, secureSmtpOptions, cancellationToken);
        
        await client.SendAsync(message, cancellationToken);
        
        await client.DisconnectAsync(true, cancellationToken);
    }
}