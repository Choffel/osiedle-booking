namespace osiedle_booking.BuildingBlocks.Interfaces;

public interface INotificationModule
{
    Task SendVerificationLinkAsync(string email, string link, CancellationToken ct = default);
}