using System.Threading;
using System.Threading.Tasks;
using Notifications.Application.DTOs.Business;


namespace Notifications.Application.Interfaces;

public interface IBusinessOnboardingNotificationService
{
    Task SendRegistrationApprovedAsync(BusinessRegistrationConfirmationDto dto, CancellationToken ct = default);
    
    Task SendRegistrationRejectedAsync(BusinessRegistrationRejectionDto dto, CancellationToken ct = default);
    
}