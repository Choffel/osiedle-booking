using Microsoft.AspNetCore.Mvc;
using Notifications.Application.Interfaces;


namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationEmailController : ControllerBase
{
    private readonly INotificationInternalApi _notificationInternalApi;
    
    public NotificationEmailController(INotificationInternalApi notificationSender)
    {
        _notificationInternalApi = notificationSender;
    }
    
    
    [HttpPost("send-link")]
    public async Task<IActionResult> SendLinkAsync(string email, string link, CancellationToken ct)
    {
        await _notificationInternalApi.SendVerificationLinkAsync(email, link, ct);
        return Ok();
    }
}