namespace Notifications.Infrastructure.Options;

public class SmtpOptions
{
    public const string SectionName = "SmtpSettings";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 1025;
    public bool UseSsl { get; init; } = false;
    public string SenderEmail { get; init; } = "no-reply@osiedlebooking.com";
    public string SenderName { get; init; } = "Osiedle Booking";
    public string? UserName { get; init; }
    public string? Password { get; init; }
}