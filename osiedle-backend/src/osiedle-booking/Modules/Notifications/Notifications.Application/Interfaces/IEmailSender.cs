using System.Threading;
using System.Threading.Tasks;

namespace Notifications.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken token = default);
    
    Task SendTemplateAsync<TModel>(string to, string subject, string templateId, TModel model, CancellationToken token = default);
}