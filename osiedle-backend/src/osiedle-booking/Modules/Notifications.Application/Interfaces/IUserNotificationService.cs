using System.Threading;
using System.Threading.Tasks;
using Notifications.Application.DTOs.User;


namespace Notifications.Application.Interfaces;

public interface IUserNotificationService
{
    Task SendRegistrationConfirmationAsync(RegistrationConfirmationDto dto, CancellationToken ct = default);
    
    Task SendPasswordResetAsync(PasswordResetDto dto, CancellationToken ct = default);
    
    Task SendAccountDeactivationAsync(UserAccountDeactivationDto dto, CancellationToken ct = default);
}