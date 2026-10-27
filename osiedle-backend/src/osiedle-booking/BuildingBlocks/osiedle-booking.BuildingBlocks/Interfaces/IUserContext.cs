namespace osiedle_booking.BuildingBlocks.Interfaces;

public interface IUserContext
{
    Guid UserId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    public string RefreshToken { get; }
    string? IpAddress { get; }
}